#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Superset.Common;

namespace FT3
{
    public class FlareTable<T>
    {
        public delegate IEnumerable<T> DataGetter();

        public delegate object ValueGetter(T data, string id);

        internal readonly UpdateTrigger UpdateTableHead = new UpdateTrigger();
        internal readonly UpdateTrigger UpdateTableBody = new UpdateTrigger();

        private readonly DataGetter     _dataGetter;
        private readonly ValueGetter    _valueGetter;
        private readonly ListDictionary _columns = new ListDictionary();

        public FlareTable(DataGetter dataGetter, ValueGetter valueGetter = null)
        {
            _dataGetter = dataGetter;

            _valueGetter = valueGetter ?? ReflectionValueGetter;
        }

        public IEnumerable<T> Rows()
        {
            foreach (T row in _dataGetter.Invoke())
            {
                var matched = true;
                foreach (Column column in _columns.Values)
                {
                    if (!column.Shown) continue;
                    if (!Match(RowValue(row, column.ID), column.FilterValue))
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                    yield return row;
            }
        }

        private string RowValue(T v, string id)
        {
            return _valueGetter.Invoke(v, id).ToString();
        }

        private object ReflectionValueGetter(T data, string id)
        {
            return ((Column) _columns[id]).Property.GetValue(data);
        }

        public void RegisterColumn(
            string         id,
            string         displayName   = null,
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

            _columns.Add(id, new Column
            {
                ID            = id,
                DisplayName   = displayName ?? id,
                Shown         = shown,
                SortDirection = sortDirection,
                FilterValue   = filterValue,
                Property      = t
            });
        }

        public string GetColumnFilter(string id)
        {
            return ((Column) _columns[id])?.FilterValue;
        }

        public void SetColumnFilter(string id, string filter)
        {
            Console.WriteLine($"FILTER | {id} -> {filter}");
            ((Column) _columns[id]).FilterValue = filter;
            UpdateTableBody.Trigger();
        }

        public void SetColumnVisibility(string id, bool shown)
        {
            ((Column) _columns[id]).Shown = shown;
            UpdateTableHead.Trigger();
            UpdateTableBody.Trigger();
        }

        public List<Column> Columns      => _columns.Values.Cast<Column>().ToList();
        public List<Column> ShownColumns => _columns.Values.Cast<Column>().Where(v => v.Shown).ToList();

        private static bool Match(string str, string term)
        {
            return str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool ColumnShown(string id)
        {
            return ((Column) _columns[id])?.Shown ?? false;
        }
    }

    public enum SortDirections
    {
        Neutral,
        Ascending,
        Descending
    }

    public sealed class Column
    {
        public   string         ID;
        public   string         DisplayName;
        internal bool           Shown;
        internal SortDirections SortDirection;
        internal string         FilterValue;
        internal PropertyInfo   Property;
    }
}