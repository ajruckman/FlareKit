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
        private readonly IEnumerable<object>               _data;
        private readonly Type                              _dataType;
        private readonly PropertyInfo[]                    _props;
        private readonly FlareLib.FlareLib.StateHasChanged _stateUpdater;
        private readonly ValueGetter                       _valueGetter;
        private          int                               _currentSortIndex = 0;

        public readonly PageStateHandler Paginate;

        public TableStateHandler(
            IEnumerable<object>               data,
            FlareLib.FlareLib.StateHasChanged stateHasChanged,
            ValueGetter                       valueGetter     = null,
            int                               paginationRange = 3,
            int                               defaultPageSize = 25
        )
        {
            _data         = data;
            _stateUpdater = stateHasChanged;

            if (valueGetter == null)
            {
                _dataType    = data.GetType().GetGenericArguments()[0];
                _props       = _dataType.GetProperties();
                _valueGetter = DataValue;
            }
            else
            {
                _valueGetter = valueGetter;
            }

            Paginate = new PageStateHandler(_stateUpdater, paginationRange, defaultPageSize);
        }

        public void InitColumn(string id)
        {
            if (_columnData.ContainsKey(id)) return;
            _columnData[id] = new Column {ID = id};
        }

        public void UpdateColumn(object value, string id)
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

        public void UpdateSort(string id)
        {
//            foreach ((string key, Column _) in _columnData.Where(v => v.Key != name)) _columnData[key].SortDir = null;

            if (_columnData[id].SortDir == null)
                _columnData[id].SortDir = 'a';
            else if (_columnData[id].SortDir == 'a')
                _columnData[id].SortDir = 'd';
            else
                _columnData[id].SortDir = null;

            _columnData[id].SortIndex = _currentSortIndex;
            _currentSortIndex++;

            Console.WriteLine($"{id} -> {_columnData[id].SortDir}");

            _stateUpdater.Invoke();
        }

        public void ResetSorting()
        {
            foreach ((string key, Column _) in _columnData)
            {
                _columnData[key].SortDir   = null;
                _columnData[key].SortIndex = null;
                _columnData[key].Value     = null;
            }

            _currentSortIndex = 0;

            _stateUpdater.Invoke();
        }

        public string SortDir(string id)
        {
            if (_columnData[id].SortDir == null)
                return "SortDirNeutral";
            return _columnData[id].SortDir == 'a' ? "SortDirAsc" : "SortDirDesc";
        }

        public void UpdatePageSize(int size)
        {
            Paginate.PageSize = size;
            _stateUpdater.Invoke();
        }

        public IEnumerable<object> Data()
        {
            Console.WriteLine("DATA");
            List<object> data = _data.ToList();

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

//            foreach ((string _, Column value) in _columnData)
//                if (value.SortDir != null)

            Sort(ref data);

            Paginate.RowCount = data.Count;

            data = data.Skip(Paginate.Skip).Take(Paginate.PageSize).ToList();

            return data;
        }

        private string Val(object v, string id)
        {
            return _valueGetter.Invoke(v, id)?.ToString();
        }

        private void Sort(ref List<object> data)
        {
            if (!data.Any()) return;

            List<Column> indices = _columnData.Where(v => v.Value.SortDir != null).Select(v => v.Value)
                                              .OrderBy(v => v.SortIndex).ToList();

            if (indices.Any())
            {
                IOrderedEnumerable<object> query;
                bool                       desc;

                Column first = indices.First();
                desc = first.SortDir == 'd';

                Console.WriteLine($"1 {first.ID} | {first.SortIndex} -> {first.Value} {(desc ? "desc" : "asc")}");

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
                        desc = index.SortDir == 'd';

                        Console.WriteLine(
                            $"2 {index.ID} | {index.SortIndex} -> {index.Value} {(desc ? "desc" : "asc")}");

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

//            List<object> res = data;
//
//            foreach (Column index in indices)
//            {
//                bool desc = index.SortDir == 'd';
//
//                if (!desc)
//                    data = data.OrderBy(v => Val(v, index.ID).ToString(),
//                        StringComparer.OrdinalIgnoreCase.WithNaturalSort()).ToList();
//                else
//                    data = data.OrderByDescending(v => Val(v, index.ID).ToString(),
//                        StringComparer.OrdinalIgnoreCase.WithNaturalSort()).ToList();
//                
//                
//            }
//
//            if (desc)
//            {
//                data = data.OrderBy(v => Val(v, id).ToString(), StringComparer.OrdinalIgnoreCase.WithNaturalSort())
//                           .ToList();
//            }
//            else
//            {
//                data = data.OrderByDescending(v => Val(v, id).ToString(),
//                    StringComparer.OrdinalIgnoreCase.WithNaturalSort()).ToList();
//            }
        }

        private static bool Match(string str, string term)
        {
            return str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class Column
        {
            public string ID;
            public char?  SortDir;
            public int?   SortIndex;
            public string Value;
        }
    }
}