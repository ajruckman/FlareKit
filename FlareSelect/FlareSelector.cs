#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Superset.Common;
using Superset.Web.State;

namespace FlareSelect
{
    public class FlareSelector<T>
        where T : IEquatable<T>
    {
        public delegate IEnumerable<IOption<T>> DataGetter();

        public delegate bool IsDisabled();

        private readonly DataGetter _dataGetter;
        private readonly IsDisabled _isDisabled;

        private readonly Dictionary<T, bool> _matchedCache = new Dictionary<T, bool>();

        private readonly OrderedDictionary _selected = new OrderedDictionary();

        internal readonly int BatchSize = 1000;

        public readonly bool    ClearOnSelect;
        public readonly bool    CloseOnSelect;
        public readonly string  FilterPlaceholder;
        public readonly uint?   MinFilterValueLength;
        public readonly string  MinFilterValueNotice;
        public readonly string? EmptyPlaceholder;
        public readonly bool    Monospace;

        public readonly bool Multiple;

        // internal readonly UpdateTrigger OnDataChange      = new UpdateTrigger();
        internal readonly UpdateTrigger OnSelectionChange  = new UpdateTrigger();
        internal readonly UpdateTrigger OnFilterValueValidChange = new UpdateTrigger();
        internal readonly UpdateTrigger OnDataInvalidation = new UpdateTrigger();

        private List<IOption<T>>? _dataCache;

        public FlareSelector(
            DataGetter dataGetter,
            bool       multiple,
            string     filterPlaceholder    = "Filter options",
            bool       clearOnSelect        = false,
            bool?      closeOnSelect        = null,
            uint?      minFilterValueLength = null,
            string?    minFilterValueNotice = null,
            string?    emptyPlaceholder     = null,
            bool       monospace            = true,
            IsDisabled? isDisabled = null
        )
        {
            _dataGetter = dataGetter;

            Multiple             = multiple;
            FilterPlaceholder    = filterPlaceholder;
            ClearOnSelect        = clearOnSelect;
            CloseOnSelect        = closeOnSelect ?? !multiple;
            MinFilterValueLength = minFilterValueLength;
            MinFilterValueNotice = minFilterValueNotice ?? $"Filter by at least {MinFilterValueLength} characters";
            EmptyPlaceholder     = emptyPlaceholder;
            Monospace            = monospace;
            _isDisabled = isDisabled ?? (() => false);
            
            GenerateBatches();
            CacheMatched();

            for (var batchID = 0; batchID < Batches!.Length; batchID++)
            {
                (UpdateTrigger, List<IOption<T>>) b = Batches[batchID];
                foreach (IOption<T> option in b.Item2)
                {
                    if (option.Selected)
                    {
                        Select(batchID, option);
                        if (!Multiple)
                            break;
                    }
                }
            }
        }

        private int BatchCount => (int) Math.Ceiling(Data().Count / (double) BatchSize);

        public (UpdateTrigger, List<IOption<T>>)[]? Batches     { get; private set; }
        public string                               FilterValue { get; private set; } = "";

        public event Action<IEnumerable<IOption<T>>>? OnSelect;

        private List<IOption<T>> Data()
        {
            return _dataCache ??= new List<IOption<T>>(_dataGetter.Invoke());
        }

        public void InvalidateData(bool deselect = false)
        {
            _dataCache = null;
            Batches    = null;
            
            if (deselect)
                _selected.Clear();

            GenerateBatches();
            CacheMatched();

            for (var batchID = 0; batchID < Batches!.Length; batchID++)
            {
                (UpdateTrigger, List<IOption<T>>) b = Batches[batchID];
                foreach (IOption<T> option in b.Item2)
                {
                    if (option.Selected)
                    {
                        Select(batchID, option);
                        if (!Multiple)
                            break;
                    }
                }
            }

            OnDataInvalidation.ReDiff();
            
            if (deselect)
                OnSelectionChange.ReDiff();
        }

        private void GenerateBatches()
        {
            Batches = new (UpdateTrigger, List<IOption<T>>)[BatchCount];

            for (var i = 0; i < Data().Count; i += BatchSize)
            {
                int batchID = i / BatchSize;
                Batches[batchID] = (new UpdateTrigger(), Data().GetRange(i, Math.Min(BatchSize, Data().Count - i)));
            }
        }

        private RenderFragment? _renderer;

        public RenderFragment Render()
        {
            _renderer ??= builder =>
            {
                builder.OpenComponent<__FlareSelector<T>>(0);
                builder.AddAttribute(1, "FlareSelector", this);
                builder.CloseComponent();
            };

            return _renderer;
        }

        private void CacheMatched()
        {
            for (var batchID = 0; batchID < Batches!.Length; batchID++)
            {
                var needsUpdate = false;

                foreach (IOption<T> option in Batches[batchID].Item2)
                {
                    bool shownNow = _matchedCache.ContainsKey(option.ID);

                    bool shownNew =
                        FilterValue                                                                 == "" ||
                        option.OptionText?.IndexOf(FilterValue, StringComparison.OrdinalIgnoreCase) >= 0;

                    if (shownNow && !shownNew)
                    {
                        _matchedCache.Remove(option.ID);
                        needsUpdate = true;
                    }
                    else if (!shownNow && shownNew)
                    {
                        _matchedCache.Add(option.ID, true);
                        needsUpdate = true;
                    }
                }

                if (needsUpdate)
                    Batches[batchID].Item1.Trigger();
            }
        }

        [Obsolete("This method is not as fast as manually iterating over Batches.")]
        public IEnumerable<IOption<T>> Options()
        {
            if (MinFilterValueLength != null && FilterValue.Length < MinFilterValueLength)
            {
                yield return
                    new Option<T>
                    {
                        OptionText  = MinFilterValueNotice,
                        Placeholder = true,
                    };
                yield break;
            }

            for (var b = 0; b < Batches!.Length; b++)
            {
                foreach (IOption<T> t in Batches[b].Item2)
                {
                    yield return t;
                }
            }
        }

        internal IEnumerable<(int, IOption<T>)> Selected()
        {
            List<(int, IOption<T>)> result = new List<(int, IOption<T>)>();
            foreach (object? option in _selected.Values)
                if (option != null)
                    result.Add(((int, IOption<T>)) option);

            return result;
        }

        internal void Select(int batchID, int optionIndex)
        {
            IOption<T> option = Batches![batchID].Item2[optionIndex];
            Select(batchID, option);
        }

        internal bool AnySelected() => _selected.Values.Count > 0;

        internal void Select(int batchID, IOption<T> option, bool replace = false, bool update = true)
        {
            if (replace || !_selected.Contains(option.ID))
            {
                if (!Multiple)
                {
                    _selected.Clear();
                    _selected[option.ID] = (batchID, option);
                }
                else
                {
                    _selected[option.ID] = (batchID, option);
                }
            }
            else
            {
                if (!Multiple)
                {
                    _selected.Clear();
                }
                else
                {
                    _selected.Remove(option.ID);
                }
            }

            if (update)
            {
                Batches![batchID].Item1.Trigger();
                if (ClearOnSelect)
                {
                    FilterValue = "";
                    CacheMatched();
                }

                OnSelectionChange.Trigger();
                NotifySelectionChange();
            }
        }

        private void NotifySelectionChange()
        {
            
            if (OnSelect != null)
            {
                IOption<T>[] result = new IOption<T>[_selected.Count];
                for (var i = 0; i < _selected.Count; i++)
                {
                    (int, IOption<T>)? v = ((int, IOption<T>)?) _selected[i];
                    if (v != null)
                        result[i] = v.Value.Item2;
                }

                OnSelect?.Invoke(result);
            }
        }

        public void Select(T id, bool replace = false)
        {
            for (var batchID = 0; batchID < Batches!.Length; batchID++)
            {
                foreach (IOption<T> option in Batches[batchID].Item2)
                    if (option.ID.Equals(id))
                    {
                        Select(batchID, option, replace, false);
                        Batches[batchID].Item1.Trigger();
                        goto Done;
                    }
            }

            Done:
            
            if (ClearOnSelect)
            {
                FilterValue = "";
                CacheMatched();
            }

            OnSelectionChange.Trigger();
            NotifySelectionChange();
        }

        public void Select(IEnumerable<T> ids, bool replace = false)
        {
            if (replace)
                _selected.Clear();

            IEnumerable<T> idsEnum = ids as T[] ?? ids.ToArray();
            
            for (var batchID = 0; batchID < Batches!.Length; batchID++)
                foreach (IOption<T> option in Batches[batchID].Item2)
                    foreach (T id in idsEnum)
                        if (option.ID.Equals(id))
                            Select(batchID, option);
        }

        public void Deselect()
        {
            _selected.Clear();
            OnSelectionChange.Trigger();
            NotifySelectionChange();
        }

        public bool IsOptionSelected(IOption<T> option)
        {
            return _selected.Contains(option.ID);
        }

        public bool IsOptionShown(IOption<T> option)
        {
            return _matchedCache.ContainsKey(option.ID);
        }

        public void UpdateFilterValue(string value)
        {
            FilterValue = value;
            CacheMatched();
        }

        internal bool Disabled() => _isDisabled.Invoke();
    }
}