using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;

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
            _columnData[id] = new Column();
        }

        public void UpdateColumn(UIChangeEventArgs args, string id)
        {
            _columnData[id].Value = (string) args.Value == "" ? null : (string) args.Value;

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

            return prop.GetValue(data)?.ToString();
        }

        public void UpdateSort(string name)
        {
            foreach ((string key, Column _) in _columnData.Where(v => v.Key != name)) _columnData[key].SortDir = null;

            if (_columnData[name].SortDir == null)
                _columnData[name].SortDir = 'a';
            else
                _columnData[name].SortDir = _columnData[name].SortDir == 'a' ? 'd' : 'a';

            _stateUpdater.Invoke();
        }

        public void ResetSorting()
        {
            foreach ((string key, Column _) in _columnData)
            {
                _columnData[key].SortDir = null;
                _columnData[key].Value   = null;
            }

            _stateUpdater.Invoke();
        }

        public string SortDir(string name)
        {
            if (_columnData[name].SortDir == null)
                return "SortDirNeutral";
            return _columnData[name].SortDir == 'a' ? "SortDirAsc" : "SortDirDesc";
        }

        public void UpdatePageSize(UIChangeEventArgs args)
        {
            Paginate.PageSize = int.Parse((string) args.Value);
            _stateUpdater.Invoke();
        }

        public IEnumerable<object> Data()
        {
            IEnumerable<object> data = _data;

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

            foreach ((string id, Column value) in _columnData)
                if (value.SortDir != null)
                {
                    if (value.SortDir == 'a')
                        Sort(ref data, id, false);
                    else if (value.SortDir == 'd')
                        Sort(ref data, id, true);
                    break;
                }

            IEnumerable<object> enumerable = data.ToArray();

            Paginate.RowCount = enumerable.Count();

            data = enumerable.Skip(Paginate.Skip).Take(Paginate.PageSize).ToList();

            return data;
        }

        string Val(object v, string _id)
        {
            return _valueGetter.Invoke(v, _id)?.ToString();
        }

        private void Sort(ref IEnumerable<object> data, string id, bool desc)
        {
            IEnumerable<object> enumerable = data as object[] ?? data.ToArray();

            if (!enumerable.Any()) return;

            bool isSortable = enumerable.First() is IComparable;

            if (!desc)
                if (isSortable)
                    data = enumerable.OrderBy(v => Val(v, id).Length)
                                     .ThenBy(v => Val(v, id)).ToList();
                else
                    data = enumerable.OrderBy(v => Val(v, id)?.Length)
                                     .ThenBy(v => Val(v, id)?.ToString());

            else if (isSortable)
                data = enumerable.OrderByDescending(v => Val(v, id).Length)
                                 .ThenByDescending(v => Val(v, id)).ToList();
            else
                data = enumerable.OrderByDescending(v => Val(v, id)?.Length)
                                 .ThenByDescending(v => Val(v, id)?.ToString());
        }

        private static bool Match(string str, string term)
        {
            return str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class Column
        {
            public char?  SortDir;
            public string Value;
        }
    }
}