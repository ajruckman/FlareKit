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

        private const    int        BatchSize = 1000;
        private readonly DataGetter _dataGetter;

        private readonly Dictionary<T, bool> _matchedCache = new Dictionary<T, bool>();

        private readonly OrderedDictionary _selected = new OrderedDictionary();
        public readonly  bool              CloseOnSelect;

        public readonly bool Multiple;

        // internal readonly UpdateTrigger OnDataChange      = new UpdateTrigger();
        internal readonly UpdateTrigger OnSelectionChange = new UpdateTrigger();

        private List<IOption<T>>? _dataCache;

        public FlareSelector(DataGetter dataGetter, bool multiple, bool? closeOnSelect = null)
        {
            _dataGetter   = dataGetter;
            Multiple      = multiple;
            CloseOnSelect = closeOnSelect ?? !multiple;

            // // _selected = new List<Option<T>>();
            // // _selected.AddRange(GenerateBatches().Where(v => v.Selected));
            // foreach (IOption<T> option in Data())
            // {
            //     if (option.Selected)
            //     {
            //         Select(option);
            //         if (!Multiple)
            //             break;
            //     }
            // }

            GenerateBatches();
            CacheMatched();

            for (var batchID = 0; batchID < Batches.Length; batchID++)
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

        // internal int BatchCount => (int) Math.Ceiling(_dataGetter.Invoke().Count() / (double) BatchSize);
        internal int BatchCount => (int) Math.Ceiling(Data().Count / (double) BatchSize);

        internal (UpdateTrigger, List<IOption<T>>)[]? Batches     { get; set; }
        public   string                               FilterValue { get; private set; } = "";

        private List<IOption<T>> Data()
        {
            return _dataCache ??= new List<IOption<T>>(_dataGetter.Invoke());
        }

        public void InvalidateData()
        {
            _dataCache = null;
            Batches    = null;

            GenerateBatches();
            CacheMatched();

            for (var batchID = 0; batchID < Batches.Length; batchID++)
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

        // public void Select(IOption<T> option)
        // {
        //     if (!_selected.Contains(option.ID))
        //     {
        //         Console.WriteLine($"Select({option.ID}) ++");
        //         if (!Multiple)
        //         {
        //             _selected.Clear();
        //             _selected[option.ID] = option;
        //         }
        //         else
        //         {
        //             _selected[option.ID] = option;
        //         }
        //     }
        //     else
        //     {
        //         Console.WriteLine($"Select({option.ID}) --");
        //         if (!Multiple)
        //         {
        //             _selected.Clear();
        //         }
        //         else
        //         {
        //             _selected.Remove(option.ID);
        //         }
        //     }
        //
        //     OnSelectionChange.Trigger();
        // }

        // public List<IOption<T>> Selected()
        // {
        //     List<IOption<T>> result = new List<IOption<T>>();
        //     foreach (object? option in _selected.Values)
        //     {
        //         if (option != null)
        //             result.Add((IOption<T>) option);
        //     }
        //
        //     return result;
        // }

        internal List<(int, IOption<T>)> Selected()
        {
            List<(int, IOption<T>)> result = new List<(int, IOption<T>)>();
            foreach (object? option in _selected.Values)
            {
                if (option != null)
                    result.Add(((int, IOption<T>)) option);
            }

            return result;
        }

        internal void Select(int batchID, int optionIndex)
        {
            IOption<T> option = Batches[batchID].Item2[optionIndex];
            Select(batchID, option);
        }

        internal void Select(int batchID, IOption<T> option)
        {
            // Select(option);

            if (!_selected.Contains(option.ID))
            {
                Console.WriteLine($"Select({option.ID}) ++");
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

            Batches[batchID].Item1.Trigger();
            OnSelectionChange.Trigger();
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
            Console.WriteLine("Filter -> " + value);
            FilterValue = value;
            CacheMatched();
        }
    }
}