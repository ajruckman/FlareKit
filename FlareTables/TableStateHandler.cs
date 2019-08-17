using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace FlareTables
{
    public static class Shared
    {
        public delegate void StateHasChanged();
    }

    [SuppressMessage("ReSharper", "MemberCanBeInternal")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class TableStateHandler
    {
        private readonly Dictionary<string, Column> _columnData = new Dictionary<string, Column>();
        private readonly IEnumerable<object>        _data;

        private readonly Type                    _dataType;
        private readonly PropertyInfo[]          _props;
        private readonly Shared.StateHasChanged  _stateUpdater;

        public readonly PageStateHandler Paginate;

        public TableStateHandler(
            IEnumerable<object>     data,
            Shared.StateHasChanged  stateHasChanged,
            int                     paginationRange = 3,
            int                     defaultPageSize = 25
        )
        {
            _data         = data;
            _stateUpdater = stateHasChanged;

            _dataType = data.GetType().GetGenericArguments()[0];
            _props    = _dataType.GetProperties();

            Paginate = new PageStateHandler(_stateUpdater, paginationRange, defaultPageSize);
        }

        public void InitColumn(string name)
        {
            if (_columnData.ContainsKey(name)) return;
            if (_props.All(v => v.Name != name))
                throw new ArgumentException(
                    $"Field name '{name}' does not exist in type '{_dataType.Name}'");

            _columnData[name] = new Column
            {
                Property = _props.First(v => v.Name == name)
            };
        }

        public void UpdateColumn(UIChangeEventArgs args, string name)
        {
            _columnData[name].Value = (string) args.Value == "" ? null : (string) args.Value;

            _stateUpdater.Invoke();
        }

        public string ColumnValue(string name)
        {
            return _columnData[name].Value;
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
                foreach ((string _, Column value) in _columnData)
                {
                    if (value.Value == null) continue;

                    bool matches = Match(value.Property.GetValue(v)?.ToString(), value.Value);
                    if (!matches) return false;
                }

                return true;
            }).ToList();

            foreach ((string s, Column value) in _columnData)
                if (value.SortDir != null)
                {
                    PropertyInfo prop = _props.First(v => v.Name == s);
                    if (value.SortDir == 'a')
                        Sort(ref data, prop, false);
                    else if (value.SortDir == 'd')
                        Sort(ref data, prop, true);
                    break;
                }

            IEnumerable<object> enumerable = data.ToArray();

            Paginate.RowCount = enumerable.Count();

            data = enumerable.Skip(Paginate.Skip).Take(Paginate.PageSize).ToList();

            return data;
        }

        private static void Sort(ref IEnumerable<object> data, PropertyInfo prop, bool desc)
        {
            IEnumerable<object> enumerable = data as object[] ?? data.ToArray();

            if (!enumerable.Any()) return;

            bool isSortable = enumerable.First() is IComparable;

            if (!desc)
                if (isSortable)
                    data = enumerable.OrderBy(v => prop.GetValue(v)).ToList();
                else
                    data = enumerable.OrderBy(v => prop.GetValue(v)?.ToString());
            else if (isSortable)
                data = enumerable.OrderByDescending(v => prop.GetValue(v)).ToList();
            else
                data = enumerable.OrderByDescending(v => prop.GetValue(v)?.ToString());
        }

        private static bool Match(string str, string term)
        {
            return str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class Column
        {
            public PropertyInfo Property;
            public char?        SortDir;
            public string       Value;
        }
    }
}