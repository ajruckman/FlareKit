#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NaturalSort.Extension;
using Superset.Logging;

namespace FlareTables
{
    public partial class FlareTable<T>
    {
        public delegate IEnumerable<T> DataGetter();

        public delegate object? ValueGetter(T data, string id);

        private readonly DataGetter     _dataGetter;
        private readonly ValueGetter    _valueGetter;
        private          int            _currentSortIndex;
        private          LinkedList<T>? _data;
        private readonly object         _dataLock = new object();

        internal bool RegexMode;

        private List<T>? _matchedRowCache;
        private List<T>? _sortedRowCache;

        // ReSharper disable once MemberCanBePrivate.Global
        public IEnumerable<T> AllRows()
        {
            bool dataChange = _matchedRowCache == null || _sortedRowCache == null;

            Log.Update("[Data] AllRows");
            // _data ??= _dataGetter.Invoke().ToList();

            lock (_dataLock)
            {
                _data ??= new LinkedList<T>(_dataGetter.Invoke());

                if (_matchedRowCache == null)
                {
                    Log.Update("[Data] Re-matching rows");
                    List<T> result         = new List<T>();
                    var     numRows        = 0;
                    var     numRowsMatched = 0;

                    foreach (T row in _data)
                    {
                        numRows++;

                        var matched = true;
                        foreach (Column? column in _columns.Values)
                        {
                            if (column?.ID == null) continue;

                            if (column.Shown != true) continue;
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
                                if (!column.CompiledFilterValue?.IsMatch(RowValue(row, column.ID) ?? "") ?? false)
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

                    RowCount = numRowsMatched;
                    ResetCurrentPage();

                    _matchedRowCache = result;
                }

                if (_sortedRowCache == null)
                {
                    Log.Update("[Data] Re-sorting rows");
                    _sortedRowCache = Sort(ref _matchedRowCache);
                }

                if (dataChange)
                {
                    OnPageUpdate.Invoke();
                    ExecutePending();
                }

                return _sortedRowCache ?? new List<T>();
            }
        }

        public IEnumerable<T> Rows()
        {
            return AllRows().Skip(Skip).Take(PageSize).ToList();
        }

        private List<T>? Sort(ref List<T>? data)
        {
            if (!data.Any()) return data;

            List<Column> indices =
                _columns.Values.Cast<Column>()
                        .Where(v => v.SortDirection != SortDirections.Neutral)
                        .Where(v => v.Shown)
                        .OrderBy(v => v.SortIndex)
                        .ToList();

            if (!indices.Any()) return data;

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

            return query.ToList();
        }

        private string? RowValue(T v, string id)
        {
            return _valueGetter.Invoke(v, id)?.ToString();
        }

        private object? ReflectionValueGetter(T data, string id)
        {
            return ((Column) _columns[id]).Property.GetValue(data);
        }

        private static bool Match(string? str, string? term)
        {
            return term == null || str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // ReSharper disable once MemberCanBeInternal
        public async Task ToggleRegexMode()
        {
            RegexMode = !RegexMode;

            if (RegexMode)
            {
                foreach (Column? c in _columns.Values)
                    c?.TryCompileFilter();
            }

            if (_storageProvider != null)
                await _storageProvider.SetItemAsync($"FlareTable_{_identifier}_!RegexMode", RegexMode);

            _matchedRowCache = null;
            _sortedRowCache  = null;

            OnRegexToggle.Invoke();
            ExecutePending();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void InvalidateData()
        {
            lock (_dataLock)
            {
                Log.Update();
                _data            = new LinkedList<T>(_dataGetter.Invoke());
                _matchedRowCache = null;
                _sortedRowCache  = null;
                UpdateTableControls.Trigger();
                UpdateTableBody.Trigger();
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void PrependRow(T row, int? limit = null)
        {
            lock (_dataLock)
            {
                _data ??= new LinkedList<T>(_dataGetter.Invoke());

                _data.AddFirst(row);
                if (limit != null && _data.Count > limit)
                    for (var i = 0; i < _data.Count - limit; i++)
                        _data.RemoveLast();

                _matchedRowCache = null;
                _sortedRowCache  = null;
                UpdateTableControls.Trigger();
                UpdateTableBody.Trigger();
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void AppendRow(T row, int? limit = null)
        {
            lock (_dataLock)
            {
                _data ??= new LinkedList<T>(_dataGetter.Invoke());

                _data.AddLast(row);
                if (limit != null && _data.Count > limit)
                    for (var i = 0; i < _data.Count - limit; i++)
                        _data.RemoveFirst();

                _matchedRowCache = null;
                _sortedRowCache  = null;
                UpdateTableControls.Trigger();
                UpdateTableBody.Trigger();
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void InvalidateRows()
        {
            lock (_dataLock)
            {
                Log.Update();
                _matchedRowCache = null;
                _sortedRowCache  = null;
                UpdateTableControls.Trigger();
                UpdateTableBody.Trigger();
            }
        }

        private void InvalidateSort()
        {
            Log.Update();
            UpdateTableControls.Trigger();
            UpdateTableBody.Trigger();
        }

        private void InvalidatePage()
        {
            Log.Update();
            UpdateTableBody.Trigger();
        }
    }
}