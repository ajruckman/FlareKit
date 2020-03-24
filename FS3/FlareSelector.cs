#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Superset.Common;
using Superset.Web.State;

namespace FS3
{
    public class FlareSelector<T>
        where T : IEquatable<T>
    {
        public delegate IEnumerable<IOption<T>> DataGetter();

        public readonly  bool       Multiple;
        private readonly DataGetter _dataGetter;

        // internal int BatchCount => (int) Math.Ceiling(_dataGetter.Invoke().Count() / (double) BatchSize);
        internal int BatchCount => (int) Math.Ceiling(Data().Count / (double) BatchSize);

        private const int BatchSize = 1000;

        // internal readonly UpdateTrigger OnDataChange      = new UpdateTrigger();
        internal readonly UpdateTrigger OnSelectionChange = new UpdateTrigger();

        internal (UpdateTrigger, List<IOption<T>>)[]? Batches { get; set; }

        public FlareSelector(DataGetter dataGetter, bool multiple)
        {
            _dataGetter = dataGetter;
            Multiple    = multiple;

            // _selected = new List<Option<T>>();
            // _selected.AddRange(GenerateBatches().Where(v => v.Selected));
            foreach (IOption<T> option in Data())
            {
                if (option.Selected)
                {
                    Select(option);
                    if (Multiple)
                        break;
                }
            }

            GenerateBatches();
            CacheMatched();

            // Task.Run(() =>
            // {
            //     Thread.Sleep(2000);
            //     /*foreach (IOption<T> ID in Batches[1].Item2)
            //     {
            //         ID.OptionText += " <-";
            //     }*/
            //
            //     Batches[1].Item1.Trigger();
            // });
        }

        private List<IOption<T>>? _dataCache;

        private List<IOption<T>> Data()
        {
            return _dataCache ??= new List<IOption<T>>(_dataGetter.Invoke());
        }

        public void InvalidateData()
        {
            _dataCache = null;
            Batches    = null;
            GenerateBatches();

            foreach (IOption<T> option in Data())
            {
                if (option.Selected)
                {
                    Select(option);
                    if (Multiple)
                        return;
                }
            }

            CacheMatched();
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

        public RenderFragment Render()
        {
            Console.WriteLine("Render()");

            void Fragment(RenderTreeBuilder builder)
            {
                builder.OpenComponent<__FlareSelector<T>>(0);
                builder.AddAttribute(1, "FlareSelector", this);
                builder.CloseComponent();
            }

            return Fragment;
        }

        private readonly Dictionary<T, bool> _matchedCache = new Dictionary<T, bool>();
        public           string              FilterValue { get; private set; } = "";

        private void CacheMatched()
        {
            for (var batchID = 0; batchID < Batches.Length; batchID++)
            {
                Console.Write("Processing batch " + batchID + "... ");
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

                Console.WriteLine("done, " + needsUpdate);
                if (needsUpdate)
                    Batches[batchID].Item1.Trigger();
            }

            // foreach (var option in Data())
            // {
            //     if (FilterValue                                                                 == "" ||
            //         option.OptionText?.IndexOf(FilterValue, StringComparison.OrdinalIgnoreCase) >= 0)
            //     {
            //         _matchedCache[option.ID] = true;
            //     }
            //     else
            //     {
            //         _matchedCache.Remove(option.ID);
            //     }
            // }
            //
            // OnDataChange.Trigger();
        }

        private readonly OrderedDictionary _selected = new OrderedDictionary();

        public void Select(IOption<T> option)
        {
            if (!_selected.Contains(option.ID))
            {
                Console.WriteLine($"Select({option.ID}) ++");
                if (!Multiple)
                {
                    _selected.Clear();
                    _selected[option.ID] = option;
                }
                else
                {
                    _selected[option.ID] = option;
                }
            }
            else
            {
                Console.WriteLine($"Select({option.ID}) --");
                if (!Multiple)
                {
                    _selected.Clear();
                }
                else
                {
                    _selected.Remove(option.ID);
                }
            }

            OnSelectionChange.Trigger();
        }

        public List<IOption<T>> Selected()
        {
            List<IOption<T>> result = new List<IOption<T>>();
            foreach (
                object? option in _selected.Values)
            {
                if (option != null)
                    result.Add((IOption<T>) option);
            }

            return result;
        }

        internal void Select(int batchID, int optionIndex)
        {
            IOption<T> option = Batches[batchID].Item2[optionIndex];
            Select(option);
            Batches[batchID].Item1.Trigger();
        }

        public bool IsOptionSelected(IOption<T> option)
        {
            return _selected.Contains(option.ID);
        }

        public bool IsOptionShown(IOption<T> option)
        {
            var c = _matchedCache.ContainsKey(option.ID);
            return _matchedCache.ContainsKey(option.ID);
        }

        public void UpdateFilterValue(string value)
        {
            Console.WriteLine("Filter -> " + value);
            FilterValue = value;
            CacheMatched();
        }
    }
}