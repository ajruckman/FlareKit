#nullable enable

using System;
using System.Threading.Tasks;
using Blazored.SessionStorage;
using Newtonsoft.Json;

namespace FT3
{
    public partial class FlareTable<T>
    {
        private readonly int                     _initialPageSize;
        private readonly ISessionStorageService? _sessionStorage;

        private bool _loadingSessionValues;

        public async Task LoadSessionValues()
        {
            if (_sessionStorage == null)
                throw new ArgumentException(
                    "Called LoadSessionValues(), but a ISessionStorageService was not passed to the FlareTable constructor.");

            _loadingSessionValues = true;
            
            var newMode = await _sessionStorage.GetItemAsync<bool>($"FlareTable_{_identifier}_!RegexMode");
            if (newMode != RegexMode)
                await ToggleRegexMode();

            var pageSize = await _sessionStorage.GetItemAsync<int>($"FlareTable_{_identifier}_!PageSize");
            if (pageSize != 0 && pageSize != PageSize)
                await UpdatePageSize(pageSize);

            var pageNumber = await _sessionStorage.GetItemAsync<int>($"FlareTable_{_identifier}_!PageNum");
            if (pageNumber != 0 && CurrentPage != pageNumber)
                await JumpToPage(pageNumber);

            foreach (Column column in Columns)
            {
                column.Key = $"FlareTable_{_identifier}_{column.ID}";
                var stored = await _sessionStorage.GetItemAsync<string>($"FlareTable_{_identifier}_{column.ID}");

                if (stored == null)
                    await StoreColumnConfig(column);
                else
                    LoadColumnConfig(stored, column);
            }

            _matchedRowCache = null;
            _sortedRowCache  = null;

            _loadingSessionValues = false;
            ExecutePending(false);
        }

        private async Task StoreColumnConfig(Column column)
        {
            if (_sessionStorage != null)
                await _sessionStorage.SetItemAsync(column.Key, JsonConvert.SerializeObject(column));
        }

        private void LoadColumnConfig(string configString, Column column)
        {
            Column storedConfig = JsonConvert.DeserializeObject<Column>(configString);
            column.Shown         = storedConfig.Shown;
            column.SortDirection = storedConfig.SortDirection;
            column.SortIndex     = storedConfig.SortIndex;
            column.FilterValue   = storedConfig.FilterValue;
        }
    }
}