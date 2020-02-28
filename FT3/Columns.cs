#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FT3
{
    public partial class FlareTable<T>
    {
        [NotNull] private readonly ListDictionary _columns = new ListDictionary();
        public                     List<Column>   Columns => _columns.Values.Cast<Column>().ToList();

        public async Task RegisterColumn(
            string         id,
            string?        displayName   = null,
            bool           shown         = true,
            SortDirections sortDirection = SortDirections.Neutral,
            string         filterValue   = ""
        )
        {
            if (_columns.Contains(id))
                throw new ArgumentException($"Column ID '{id}' registered twice", nameof(id));

            PropertyInfo? t = typeof(T).GetProperty(id);
            if (t == null)
                throw new ArgumentException($"Property ID '{id}' does not exist in type '{typeof(T).FullName}'");

            Column c = new Column
            (
                id,
                displayName ?? id,
                shown,
                sortDirection,
                sortDirection == SortDirections.Neutral ? 0 : _currentSortIndex++,
                filterValue,
                t
            );

            if (_sessionStorage != null)
            {
                c.Key = $"FlareTable_{_identifier}_{id}";
                var stored = await _sessionStorage.GetItemAsync<string>($"FlareTable_{_identifier}_{id}");

                if (stored == null)
                    await StoreColumnConfig(c);
                else
                    LoadColumnConfig(stored, c);
            }

            if (RegexMode)
                c.TryCompileFilter();
            
            _columns.Add(id, c);
            
            _matchedRowCache = null;
            _sortedRowCache  = null;
        }

        public async Task SetColumnFilter(string id, string filter)
        {
            ((Column) _columns[id]).FilterValue = filter;

            if (RegexMode)
            {
                ((Column) _columns[id]).FilterValue = filter;
                ((Column) _columns[id]).TryCompileFilter();
                UpdateFilterValues.Trigger();
            }

            await StoreColumnConfig((Column) _columns[id]);

            _matchedRowCache = null;
            _sortedRowCache  = null;

            UpdateTableBody.Trigger();
        }

        public async Task SetColumnVisibility(string id, bool shown)
        {
            ((Column) _columns[id]).Shown = shown;

            await StoreColumnConfig((Column) _columns[id]);

            _matchedRowCache = null;
            _sortedRowCache  = null;

            UpdateTableBody.Trigger();
            UpdateTableHead.Trigger();
        }

        public async Task NextColumnSort(string id)
        {
            Column c = (Column) _columns[id];
            c.SortDirection = c.SortDirection switch
            {
                SortDirections.Neutral   => SortDirections.Ascending,
                SortDirections.Ascending => SortDirections.Descending,
                _                        => SortDirections.Neutral
            };

            c.SortIndex = _currentSortIndex++;
            
            await StoreColumnConfig((Column) _columns[id]);

            _sortedRowCache  = null;

            UpdateTableBody.Trigger();
        }
    }
}