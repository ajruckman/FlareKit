#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Superset.Logging;

namespace FlareTables
{
    public partial class FlareTable<T>
    {
        private readonly OrderedDictionary   _columns = new OrderedDictionary();
        public           IEnumerable<Column> Columns => _columns.Values.Cast<Column>().ToList();

        public void RegisterColumn(
            string         id,
            string?        displayName   = null,
            bool           shown         = true,
            SortDirections sortDirection = SortDirections.Neutral,
            string         filterValue   = "",
            string         width         = "unset",
            bool           monospace     = false,
            bool           filterable    = true,
            bool           sortable      = true
        )
        {
            if (_columns.Contains(id))
                throw new ArgumentException($"Column ID '{id}' is already registered.", nameof(id));

            PropertyInfo? t = null;
            if (_usingReflectionValueGetter && (filterable || sortable))
            {
                t = typeof(T).GetProperty(id);
                if (t == null)
                    throw new ArgumentException($"Property ID '{id}' does not exist in type '{typeof(T).FullName}'.");
            }

            Column c = new Column
            (
                id,
                displayName ?? id,
                shown,
                width,
                monospace,
                sortDirection,
                sortDirection == SortDirections.Neutral ? 0 : _currentSortIndex++,
                filterValue,
                filterable,
                sortable,
                t
            );

            c.Default = c.Clone();

            if (RegexMode)
                c.TryCompileFilter();

            _columns.Add(id, c);

            _matchedRowCache = null;
            _sortedRowCache  = null;
        }

        // ReSharper disable once MemberCanBeInternal
        public async Task SetColumnFilter(string id, string filter)
        {
            ((Column) _columns[id]).FilterValue = filter;

            if (RegexMode)
            {
                ((Column) _columns[id]).FilterValue = filter;
                ((Column) _columns[id]).TryCompileFilter();
            }

            await StoreColumnConfig((Column) _columns[id]);

            _matchedRowCache = null;
            _sortedRowCache  = null;

            Log.Update($"SetColumnFilter({id}, {filter})");

            OnColumnFilterUpdate.Invoke();
            ExecutePending();
        }

        // ReSharper disable once MemberCanBeInternal
        public async Task SetColumnVisibility(string id, bool shown)
        {
            ((Column) _columns[id]).Shown = shown;

            await StoreColumnConfig((Column) _columns[id]);

            _matchedRowCache = null;
            _sortedRowCache  = null;

            OnColumnVisibilityUpdate.Invoke(id);
            ExecutePending();
        }

        // ReSharper disable once MemberCanBeInternal
        public async Task NextColumnSort(string id)
        {
            Column c = (Column) _columns[id];

            if (c.SortDirection == SortDirections.Neutral)
                c.SortIndex = _currentSortIndex++;

            c.SortDirection = c.SortDirection switch
            {
                SortDirections.Neutral   => SortDirections.Ascending,
                SortDirections.Ascending => SortDirections.Descending,
                _                        => SortDirections.Neutral
            };

            if (c.SortDirection == SortDirections.Neutral)
                c.SortIndex = 0;

            await StoreColumnConfig((Column) _columns[id]);

            _sortedRowCache = null;

            OnColumnSortUpdate.Invoke();
            ExecutePending();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public async Task SetColumnSort(string id, SortDirections sortDirection, int index)
        {
            Column c = (Column) _columns[id];
            c.SortDirection = sortDirection;
            c.SortIndex     = index;

            await StoreColumnConfig((Column) _columns[id]);

            _sortedRowCache = null;

            OnColumnSortUpdate.Invoke();
            ExecutePending();
        }
    }
}