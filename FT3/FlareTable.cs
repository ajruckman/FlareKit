#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NaturalSort.Extension;
using Superset.Common;

namespace FT3
{
    public class FlareTable<T>
    {
        public delegate IEnumerable<T> DataGetter();

        public delegate object ValueGetter(T data, string id);

        private readonly ListDictionary _columns = new ListDictionary();

        private readonly DataGetter  _dataGetter;
        private readonly ValueGetter _valueGetter;

        internal readonly UpdateTrigger UpdatePageState = new UpdateTrigger();
        internal readonly UpdateTrigger UpdateTableBody = new UpdateTrigger();
        internal readonly UpdateTrigger UpdateTableHead = new UpdateTrigger();

        private int _currentSortIndex;

        private  IEnumerable<T>?  _data;
        internal PageStateHandler PageState;

        internal bool RegexMode;

        public FlareTable(DataGetter dataGetter, ValueGetter valueGetter = null, bool regexMode = false)
        {
            _dataGetter  = dataGetter;
            _valueGetter = valueGetter ?? ReflectionValueGetter;
            RegexMode    = regexMode;

            PageState                   =  new PageStateHandler(3, 25);
            PageState.OnPageStateChange += UpdateTableBody.Trigger;
        }

        public List<Column> Columns => _columns.Values.Cast<Column>().ToList();

        public IEnumerable<T> Rows()
        {
            _data ??= _dataGetter.Invoke();

            List<T> result         = new List<T>();
            int     numRows        = 0;
            int     numRowsMatched = 0;

            foreach (T row in _data)
            {
                numRows++;

                var matched = true;
                foreach (Column column in _columns.Values)
                {
                    if (!column.Shown) continue;
                    if (string.IsNullOrEmpty(column.FilterValue)) continue;

                    if (!RegexMode)
                    {
                        if (!Match(RowValue(row, column.ID), column.FilterValue))
                        {
                            matched = false;
                            break;
                        }
                    }
                    else
                    {
                        if (!column.CompiledFilterValue.IsMatch(RowValue(row, column.ID)))
                        {
                            matched = false;
                            break;
                        }
                    }
                }

                if (matched)
                {
                    result.Add(row);
                    numRowsMatched++;
                }
            }

            Sort(ref result);

            result = result.Skip(PageState.Skip).Take(PageState.PageSize).ToList();

            PageState.RowCount = numRowsMatched;
            UpdatePageState.Trigger();

            return result;
        }

        private void Sort(ref List<T> data)
        {
            if (!data.Any()) return;

            List<Column> indices =
                _columns.Values.Cast<Column>()
                        .Where(v => v.SortDirection != SortDirections.Neutral)
                        .Where(v => v.Shown)
                        .OrderBy(v => v.SortIndex)
                        .ToList();

            if (!indices.Any()) return;

            Console.WriteLine("Indices:");
            foreach (Column column in indices)
            {
                Console.WriteLine($"   {column.ID} -> {column.SortDirection} / {column.SortIndex}");
            }

            Column first = indices.First();
            bool   desc  = first.SortDirection == SortDirections.Descending;

            IOrderedEnumerable<T> query;

            if (!desc)
                query = data.OrderBy(v => RowValue(v, first.ID),
                    StringComparer.OrdinalIgnoreCase.WithNaturalSort());
            else
                query = data.OrderByDescending(v => RowValue(v, first.ID),
                    StringComparer.OrdinalIgnoreCase.WithNaturalSort());

            if (indices.Count > 1)
            {
                foreach (Column index in indices.Skip(1))
                {
                    desc = index.SortDirection == SortDirections.Descending;

                    if (!desc)
                        query = query.ThenBy(v => RowValue(v, index.ID),
                            StringComparer.OrdinalIgnoreCase.WithNaturalSort());
                    else
                        query = query.ThenByDescending(v => RowValue(v, index.ID),
                            StringComparer.OrdinalIgnoreCase.WithNaturalSort());
                }
            }

            data = query.ToList();
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

            Column c = new Column
            {
                ID            = id,
                DisplayName   = displayName ?? id,
                Shown         = shown,
                SortDirection = sortDirection,
                SortIndex     = sortDirection == SortDirections.Neutral ? 0 : _currentSortIndex++,
                FilterValue   = filterValue,
                Property      = t
            };

            if (RegexMode)
                c.CompiledFilterValue = new Regex(filterValue, RegexOptions.Compiled);

            _columns.Add(id, c);
        }

        public string GetColumnFilter(string id)
        {
            return ((Column) _columns[id])?.FilterValue;
        }

        public void SetColumnFilter(string id, string filter)
        {
            Console.WriteLine($"FILTER | {id} -> {filter}");

            ((Column) _columns[id]).FilterValue = filter;

            if (RegexMode)
                ((Column) _columns[id]).CompiledFilterValue = new Regex(filter, RegexOptions.Compiled);

            UpdateTableBody.Trigger();
        }

        public void SetColumnVisibility(string id, bool shown)
        {
            Console.WriteLine($"SetColumnVisibility({id}, {shown})");
            ((Column) _columns[id]).Shown = shown;
            UpdateTableBody.Trigger();
            UpdateTableHead.Trigger();
        }

        private static bool Match(string str, string term)
        {
            return str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool ColumnShown(string id)
        {
            return ((Column) _columns[id])?.Shown ?? false;
        }

        public void NextColumnSort(string id)
        {
            Column c = ((Column) _columns[id]);
            c.SortDirection = c.SortDirection switch
            {
                SortDirections.Neutral   => SortDirections.Ascending,
                SortDirections.Ascending => SortDirections.Descending,
                _                        => SortDirections.Neutral
            };

            c.SortIndex = _currentSortIndex++;
            Console.WriteLine($"{c.ID} -> {c.SortDirection} / {c.SortIndex}");
            UpdateTableBody.Trigger();
        }

        public string GetColumnDisplayName(string id)
        {
            return ((Column) _columns[id]).DisplayName;
        }

        public string ColumnSortButtonClass(string id) => ((Column) _columns[id]).SortDirection switch
        {
            SortDirections.Neutral    => "FlareTableFilter_SortButton--Neutral",
            SortDirections.Ascending  => "FlareTableFilter_SortButton--Ascending",
            SortDirections.Descending => "FlareTableFilter_SortButton--Descending",
            _                         => ""
        };

        public string ColumnSortButtonContent(string id) => ((Column) _columns[id]).SortDirection switch
        {
            SortDirections.Neutral    => "↕",
            SortDirections.Ascending  => "↑",
            SortDirections.Descending => "↓",
            _                         => ""
        };

        internal void ToggleRegexMode()
        {
            RegexMode = !RegexMode;
            Console.WriteLine($"RegexMode -> {RegexMode}");

            if (RegexMode)
                foreach (Column c in _columns.Values)
                    c.CompiledFilterValue = new Regex(c.FilterValue, RegexOptions.Compiled);

            UpdateTableBody.Trigger();
            UpdateTableHead.Trigger();
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
        internal Regex        CompiledFilterValue;
        public   string       DisplayName;
        internal string       FilterValue;
        public   string       ID;
        internal PropertyInfo Property;
        public   bool         Shown;

        internal SortDirections SortDirection;
        internal int            SortIndex;
    }
}