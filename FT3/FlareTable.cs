#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blazored.SessionStorage;
using NaturalSort.Extension;
using Newtonsoft.Json;
using Superset.Common;

namespace FT3
{
    public class FlareTable<T>
    {
        public delegate IEnumerable<T> DataGetter();

        public delegate object ValueGetter(T data, string id);

        private readonly ListDictionary _columns = new ListDictionary();

        private readonly DataGetter _dataGetter;
        private readonly string     _identifier;

        private readonly bool                   _sessionConfig;
        private readonly ISessionStorageService _sessionStorage;
        private readonly ValueGetter            _valueGetter;

        internal readonly PageStateHandler PageState;
        internal readonly UpdateTrigger    UpdatePageState    = new UpdateTrigger();
        internal readonly UpdateTrigger    UpdateTableBody    = new UpdateTrigger();
        internal readonly UpdateTrigger    UpdateTableHead    = new UpdateTrigger();
        internal readonly UpdateTrigger    UpdateFilterValues = new UpdateTrigger();

        private int             _currentSortIndex;
        private IEnumerable<T>? _data;

        internal bool RegexMode;

        /// <summary>
        ///     Creates a FlareTable object without persistent values.
        /// </summary>
        public FlareTable(
            DataGetter  dataGetter,
            ValueGetter valueGetter = null,
            bool        regexMode   = false
        )
        {
            _dataGetter  = dataGetter;
            _valueGetter = valueGetter ?? ReflectionValueGetter;
            RegexMode    = regexMode;

            PageState                   =  new PageStateHandler(3, 25);
            PageState.OnPageStateChange += UpdateTableBody.Trigger;
        }

        /// <summary>
        ///     Creates a FlareTable object that accesses session storage to
        ///     load and store persistent values.
        /// </summary>
        public FlareTable(
            DataGetter             dataGetter,
            ISessionStorageService sessionStorage,
            string                 identifier,
            ValueGetter            valueGetter = null,
            bool                   regexMode   = false
        )
        {
            _dataGetter     = dataGetter;
            _sessionConfig  = true;
            _sessionStorage = sessionStorage;
            _identifier     = identifier;
            _valueGetter    = valueGetter ?? ReflectionValueGetter;

            RegexMode                   =  regexMode;
            PageState                   =  new PageStateHandler(3, 25);
            PageState.OnPageStateChange += UpdateTableBody.Trigger;
        }

        // private FlareTable(
        //     DataGetter             dataGetter,
        //     ISessionStorageService sessionStorage,
        //     string                 identifier,
        //     ValueGetter            valueGetter = null,
        //     bool                   regexMode   = false
        // )
        // {
        //     _dataGetter     = dataGetter;
        //     _valueGetter    = valueGetter ?? ReflectionValueGetter;
        //     _sessionConfig  = true;
        //     _sessionStorage = sessionStorage;
        //     _identifier     = identifier;
        //     RegexMode       = regexMode;
        //
        //     PageState                   =  new PageStateHandler(3, 25);
        //     PageState.OnPageStateChange += UpdateTableBody.Trigger;
        // }

        public List<Column> Columns => _columns.Values.Cast<Column>().ToList();

        public async Task LoadSessionValues()
        {
            if (!_sessionConfig)
                throw new ArgumentException(
                    "Called LoadSessionValues(), but a ISessionStorageService was not passed to the FlareTable constructor.");

            var newMode = await _sessionStorage.GetItemAsync<bool>($"FlareTable_{_identifier}_!RegexMode");
            if (newMode != RegexMode)
                await ToggleRegexMode();
        }

        public IEnumerable<T> AllRows()
        {
            _data ??= _dataGetter.Invoke();

            List<T> result         = new List<T>();
            var     numRows        = 0;
            var     numRowsMatched = 0;

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

            PageState.RowCount = numRowsMatched;
            UpdatePageState.Trigger();

            return result;
        }

        public IEnumerable<T> Rows() => AllRows().Skip(PageState.Skip).Take(PageState.PageSize).ToList();

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

            data = query.ToList();
        }

        private string RowValue(T v, string id) => _valueGetter.Invoke(v, id).ToString();

        private object ReflectionValueGetter(T data, string id) => ((Column) _columns[id]).Property.GetValue(data);

        public async Task RegisterColumn(
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

            if (_sessionConfig)
            {
                c.Key = $"FlareTable_{_identifier}_{id}";
                var stored = await _sessionStorage.GetItemAsync<string>($"FlareTable_{_identifier}_{id}");

                if (stored == null)
                    await StoreColumnConfig(c);
                else
                    await LoadColumnConfig(stored, c);
            }

            if (RegexMode)
                try
                {
                    c.CompiledFilterValue = new Regex(filterValue, RegexOptions.Compiled);
                }
                catch (Exception e)
                {
                    c.CompiledFilterValue = new Regex("a^", RegexOptions.Compiled);
                }

            _columns.Add(id, c);
        }

        private async Task StoreColumnConfig(Column column)
        {
            await _sessionStorage.SetItemAsync(column.Key, JsonConvert.SerializeObject(column));
        }

        private async Task LoadColumnConfig(string configString, Column column)
        {
            Column storedConfig = JsonConvert.DeserializeObject<Column>(configString);
            column.Shown         = storedConfig.Shown;
            column.SortDirection = storedConfig.SortDirection;
            column.SortIndex     = storedConfig.SortIndex;
            column.FilterValue   = storedConfig.FilterValue;
        }

        public string GetColumnFilter(string id) => ((Column) _columns[id])?.FilterValue;

        public async Task SetColumnFilter(string id, string filter)
        {
            ((Column) _columns[id]).FilterValue = filter;

            if (RegexMode)
            {
                try
                {
                    ((Column) _columns[id]).CompiledFilterValue = new Regex(filter, RegexOptions.Compiled);
                }
                catch (Exception e)
                {
                    ((Column) _columns[id]).CompiledFilterValue = new Regex("a^", RegexOptions.Compiled);
                }
            }

            if (_sessionConfig)
                await StoreColumnConfig((Column) _columns[id]);

            UpdateTableBody.Trigger();
        }

        public async Task SetColumnVisibility(string id, bool shown)
        {
            ((Column) _columns[id]).Shown = shown;

            if (_sessionConfig)
                await StoreColumnConfig((Column) _columns[id]);

            UpdateTableBody.Trigger();
            UpdateTableHead.Trigger();
        }

        private static bool Match(string str, string term) =>
            str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

        public bool ColumnShown(string id) => ((Column) _columns[id])?.Shown ?? false;

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
            if (_sessionConfig)
                await StoreColumnConfig((Column) _columns[id]);

            UpdateTableBody.Trigger();
        }

        public string GetColumnDisplayName(string id) => ((Column) _columns[id]).DisplayName;

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

        internal async Task ToggleRegexMode()
        {
            RegexMode = !RegexMode;

            if (RegexMode)
                foreach (Column c in _columns.Values)
                    try
                    {
                        c.CompiledFilterValue = new Regex(c.FilterValue, RegexOptions.Compiled);
                    }
                    catch (Exception e)
                    {
                        c.CompiledFilterValue = new Regex("a^", RegexOptions.Compiled);
                    }

            if (_sessionConfig)
                await _sessionStorage.SetItemAsync($"FlareTable_{_identifier}_!RegexMode", RegexMode);

            UpdateTableBody.Trigger();
            UpdateTableHead.Trigger();
        }

        public async Task Reset()
        {
            foreach (Column c in _columns.Values)
            {
                c.Shown         = true;
                c.FilterValue   = "";
                c.SortDirection = SortDirections.Neutral;
                c.SortIndex     = 0;
                await StoreColumnConfig(c);
            }

            PageState.First();

            UpdateTableBody.Trigger();
            UpdateTableHead.Trigger();
            UpdateFilterValues.Trigger();
        }

        public string AsCSV()
        {
            StringBuilder result = new StringBuilder();

            IEnumerable<Column> columns = Columns.Where(v => v.Shown).ToList();

            if (!columns.Any()) return "";

            List<string> headings = columns.Select(v => StringToCSVCell(v.DisplayName)).ToList();
            result.AppendLine(string.Join(',', headings));

            foreach (T row in AllRows())
            {
                List<string> line = columns.Select(column => StringToCSVCell(RowValue(row, column.ID))).ToList();
                result.AppendLine(string.Join(',', line));
            }

            return result.ToString();
        }

        // https://stackoverflow.com/a/6377656/9911189
        private static string StringToCSVCell(string str)
        {
            bool mustQuote = str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n");
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }

                sb.Append("\"");
                return sb.ToString();
            }

            return str;
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
        [JsonIgnore]   internal Regex        CompiledFilterValue;
        [JsonIgnore]   public   string       DisplayName;
        [JsonProperty] internal string       FilterValue;
        [JsonIgnore]   public   string       ID;
        [JsonIgnore]   internal string       Key;
        [JsonIgnore]   internal PropertyInfo Property;
        [JsonProperty] public   bool         Shown;

        [JsonProperty] internal SortDirections SortDirection;
        [JsonProperty] internal int            SortIndex;
    }
}