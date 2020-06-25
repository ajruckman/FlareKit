#nullable enable

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FlareTables
{
    public partial class FlareTable<T>
    {
        private readonly int               _initialPageSize;
        private readonly IStorageProvider? _storageProvider;

        private bool _loadingSessionValues;

        public async Task LoadStorageValues()
        {
            if (_storageProvider == null)
                throw new ArgumentException(
                    "Called LoadSessionValues(), but a ISessionStorageService was not passed to the FlareTable constructor.");

            _loadingSessionValues = true;

            var newMode = await _storageProvider.GetItemAsync<bool>($"FlareTable_{_identifier}_!RegexMode");
            if (newMode != RegexMode)
                await ToggleRegexMode();

            var pageSize = await _storageProvider.GetItemAsync<int>($"FlareTable_{_identifier}_!PageSize");
            if (pageSize != 0 && pageSize != PageSize)
                await UpdatePageSize(pageSize);

            var pageNumber = await _storageProvider.GetItemAsync<int>($"FlareTable_{_identifier}_!PageNum");
            if (pageNumber != 0 && CurrentPage != pageNumber)
                await JumpToPage(pageNumber);

            foreach (Column column in Columns)
            {
                column.Key = $"FlareTable_{_identifier}_{column.ID}";
                string? stored = null;
                try
                {
                    stored = await _storageProvider.GetItemAsync<string?>($"FlareTable_{_identifier}_{column.ID}");
                }
                catch
                {
                    // ignored
                }

                if (stored == null)
                    await StoreColumnConfig(column);
                else
                    await LoadColumnConfig(stored, column);
            }

            _matchedRowCache = null;
            _sortedRowCache  = null;

            _loadingSessionValues = false;
            ExecutePending();
        }

        private async Task StoreColumnConfig(Column column)
        {
            if (_storageProvider != null)
                await _storageProvider.SetItemAsync(column.Key, JsonConvert.SerializeObject(column));
        }

        private async Task LoadColumnConfig(string configString, Column column)
        {
            Column storedConfig = JsonConvert.DeserializeObject<Column>(configString);

            await SetColumnVisibility(column.ID, storedConfig.Shown);
            await SetColumnSort(column.ID, storedConfig.SortDirection, storedConfig.SortIndex);
            await SetColumnFilter(column.ID, storedConfig.FilterValue);
        }
    }
}