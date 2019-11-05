using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using NaturalSort.Extension;

namespace FlareTables
{
    [SuppressMessage("ReSharper", "MemberCanBeInternal")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class TableStateHandler
    {
        public delegate object ValueGetter(object data, string id);

        private readonly Dictionary<string, Column>        _columnData = new Dictionary<string, Column>();
        private readonly Proxy.Data                        _data;
        private readonly Type                              _dataType;
        private readonly PropertyInfo[]                    _props;
        private readonly FlareLib.FlareLib.StateHasChanged _stateUpdater;
        private readonly ValueGetter                       _valueGetter;
        private          int                               _currentSortIndex = 0;

        public readonly PageStateHandler Paginate;

        public TableStateHandler(
            Proxy.Data                        data,
            FlareLib.FlareLib.StateHasChanged stateHasChanged,
            Type                              type,
            int                               paginationRange = 3,
            int                               defaultPageSize = 25
        )
        {
            _dataType    = type;
            _props       = _dataType.GetProperties();
            _valueGetter = DataValue;

            _data         = data;
            _stateUpdater = stateHasChanged;
            Paginate      = new PageStateHandler(_stateUpdater, paginationRange, defaultPageSize);
        }

        public TableStateHandler(
            Proxy.Data                        data,
            FlareLib.FlareLib.StateHasChanged stateHasChanged,
            ValueGetter                       valueGetter,
            int                               paginationRange = 3,
            int                               defaultPageSize = 25
        )
        {
            _valueGetter = valueGetter;

            _data         = data;
            _stateUpdater = stateHasChanged;
            Paginate      = new PageStateHandler(_stateUpdater, paginationRange, defaultPageSize);
        }

        public void InitColumn(string id)
        {
            if (_columnData.ContainsKey(id)) return;
            _columnData[id] = new Column {ID = id};
        }

        public void UpdateColumValue(object value, string id)
        {
            _columnData[id].Value = value?.ToString() ?? "";

            _stateUpdater.Invoke();
        }

        public string ColumnValue(string id)
        {
            return _columnData[id].Value;
        }

        private string DataValue(object data, string id)
        {
            PropertyInfo prop = _props.FirstOrDefault(v => v.Name == id);

            if (prop == null)
                throw new ArgumentException(
                    $"Field name '{id}' does not exist in type '{_dataType.Name}'");

            if (prop.PropertyType == typeof(DateTime))
                return ((DateTime?) prop.GetValue(data))?.Ticks.ToString() ?? "";

            return prop.GetValue(data)?.ToString();
        }

        public enum SortDirections
        {
            Neutral,
            Ascending,
            Descending
        }

        public void UpdateSortDirection(string id)
        {
            if (_columnData[id].SortDir == SortDirections.Neutral)
                _columnData[id].SortDir = SortDirections.Ascending;
            else if (_columnData[id].SortDir == SortDirections.Ascending)
                _columnData[id].SortDir = SortDirections.Descending;
            else
                _columnData[id].SortDir = SortDirections.Neutral;

            _columnData[id].SortIndex = _currentSortIndex;
            _currentSortIndex++;

            _stateUpdater.Invoke();
        }

        public void UpdateSortDirection(string id, SortDirections direction)
        {
            _columnData[id].SortDir = direction;
            _columnData[id].SortIndex = _currentSortIndex;
            _currentSortIndex++;
            _stateUpdater.Invoke();
        }

        public void ResetSorting()
        {
            foreach ((string key, Column _) in _columnData)
            {
                _columnData[key].SortDir   = SortDirections.Neutral;
                _columnData[key].SortIndex = null;
                _columnData[key].Value     = null;
            }

            _currentSortIndex = 0;

            _stateUpdater.Invoke();
        }

        public string SortDir(string id)
        {
            if (_columnData[id].SortDir == SortDirections.Neutral)
                return "SortDirNeutral";
            return _columnData[id].SortDir == SortDirections.Ascending ? "SortDirAsc" : "SortDirDesc";
        }

        public void UpdatePageSize(int size)
        {
            Paginate.PageSize = size;
            _stateUpdater.Invoke();
        }

        public IEnumerable<object> Data()
        {
            IEnumerable<object> data = _data.Invoke();

            data = data.Where(v =>
            {
                foreach ((string id, Column value) in _columnData)
                {
                    if (value.Value == null) continue;

                    bool matches = Match(Val(v, id), value.Value);
                    if (!matches) return false;
                }

                return true;
            }).ToList();

            Sort(ref data);

            Paginate.RowCount = data.Count();

            data = data.Skip(Paginate.Skip).Take(Paginate.PageSize).ToList();

            return data;
        }

        private string Val(object v, string id)
        {
            return _valueGetter.Invoke(v, id)?.ToString();
        }

        private void Sort(ref IEnumerable<object> data)
        {
            if (!data.Any()) return;

            List<Column> indices =
                _columnData
                   .Where(v => v.Value.SortDir != SortDirections.Neutral)
                   .Select(v => v.Value)
                   .OrderBy(v => v.SortIndex).ToList();

            if (indices.Any())
            {
                Column first = indices.First();
                bool   desc  = first.SortDir == SortDirections.Descending;

                IOrderedEnumerable<object> query;

                if (!desc)
                    query = data.OrderBy(v => Val(v, first.ID).ToString(),
                        StringComparer.OrdinalIgnoreCase.WithNaturalSort());
                else
                    query = data.OrderByDescending(v => Val(v, first.ID).ToString(),
                        StringComparer.OrdinalIgnoreCase.WithNaturalSort());

                if (indices.Count > 1)
                {
                    foreach (Column index in indices.Skip(1))
                    {
                        desc = index.SortDir == SortDirections.Descending;

                        if (!desc)
                            query = query.ThenBy(v => Val(v, index.ID).ToString(),
                                StringComparer.OrdinalIgnoreCase.WithNaturalSort());
                        else
                            query = query.ThenByDescending(v => Val(v, index.ID).ToString(),
                                StringComparer.OrdinalIgnoreCase.WithNaturalSort());
                    }
                }

                data = query.ToList();
            }
        }

        private static bool Match(string str, string term)
        {
            return str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class Column
        {
            public string         ID;
            public SortDirections SortDir;
            public int?           SortIndex;
            public string         Value;
        }
    }
}