using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Superset.Common;

namespace FlareSelect
{
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeInternal")]
    public sealed class FlareSelector
    {
        internal readonly string                            InstanceID;
        internal readonly string                            SearchInputID;
        public            bool                              Focused;
        internal          IJSRuntime                        JSRuntime;
        public            string                            SearchTerm;
        public            List<IOption>                     Selected;
        internal          FlareLib.FlareLib.StateHasChanged StateHasChanged;

        public FlareSelector
        (
            Events.Options options,
            bool           multiple            = false,
            bool?          closeOnSelect       = null,
            bool           disabled            = false,
            ushort         minSearchTermLength = 0,
            string         minSearchTermText   = null,
            string         placeholder         = "",
            bool           clearSearchOnSelect = false
        )
        {
            Options             = options;
            Multiple            = multiple;
            CloseOnSelect       = closeOnSelect;
            Disabled            = disabled;
            MinSearchTermLength = minSearchTermLength;
            MinSearchTermText   = minSearchTermText;
            Placeholder         = placeholder;
            ClearSearchOnSelect = clearSearchOnSelect;

            InstanceID    = $"FlareSelect_{Guid.NewGuid().ToString().Replace("-", "")}";
            SearchInputID = $"{InstanceID}_SearchInput";

            if (CloseOnSelect == null)
                CloseOnSelect = !Multiple;

            UpdateSelected();
            OnUpdate?.Invoke(Selected);

            SelectionUpdateTrigger          =  new UpdateTrigger();
            SelectionUpdateTrigger.OnUpdate += UpdateSelected;

            if (minSearchTermLength != 0 && minSearchTermText == null)
                MinSearchTermText = $"Type at least {minSearchTermLength} characters";

            Global.ElementClickHandler.OnOuterClick += targetID =>
            {
                if (targetID == InstanceID) Unfocus();
            };

            Global.ElementClickHandler.OnInnerClick += targetID =>
            {
                if (targetID == InstanceID) Focus();
            };
        }

        public Events.Options Options { get; }

        // public Events.Options OptionsFiltered { get; }
        public bool   Multiple            { get; }
        public bool?  CloseOnSelect       { get; }
        public bool   Disabled            { get; }
        public ushort MinSearchTermLength { get; }
        public string MinSearchTermText   { get; }
        public string Placeholder         { get; }
        public bool   ClearSearchOnSelect { get; }

        [SuppressMessage("ReSharper", "MergeConditionalExpressionWhenPossible")]
        [SuppressMessage("ReSharper", "RedundantIfElseBlock")]
        public IEnumerable<IOption> Filtered
        {
            get
            {
                int searchTermLen = SearchTerm?.Length ?? 0;

                if (searchTermLen < MinSearchTermLength)
                {
                    return new[]
                    {
                        new Option
                        {
                            Disabled = true,
                            Text     = MinSearchTermText
                        }
                    };
                }

                if (SearchTerm == null)
                {
                    return Options.Invoke(SearchTerm).ToList();
                }
                else
                {
                    return Options.Invoke(SearchTerm).Where(v => Match(v.Text, SearchTerm));
                }

                // Console.WriteLine($"|| {SearchTerm} -> {SearchTerm == null} {OptionsFiltered == null}");
                // if (SearchTerm == null)
                //     return OptionsFiltered == null
                //         ? Options.Invoke(SearchTerm).ToList()
                //         : OptionsFiltered.Invoke(SearchTerm).ToList();
                //
                // return OptionsFiltered == null
                //     ? Options.Invoke(SearchTerm).Where(v => Match(v.DropdownText.ToString(), SearchTerm))
                //     : OptionsFiltered.Invoke(SearchTerm);
            }
        }

        public event Action<List<IOption>> OnUpdate;
        public event Action<string>        OnSearch;
        public readonly UpdateTrigger      SelectionUpdateTrigger;

        public RenderFragment Render()
        {
            void Fragment(RenderTreeBuilder builder)
            {
                builder.OpenComponent<_FlareSelector>(0);
                builder.AddAttribute(1, "FlareSelector", this);
                builder.CloseComponent();
            }

            return Fragment;
        }

        public void ClearSearch()
        {
            SearchTerm = null;
            JSRuntime.InvokeVoidAsync("window.clearInput", SearchInputID);
            OnSearch?.Invoke(SearchTerm);
        }

        public void UpdateSelected()
        {
            // Don't invoke _onUpdate here. When someone manually invokes
            // UpdateSelected they already know the selection was updated.
            Selected = !Multiple
                ? Options.Invoke(SearchTerm).Where(v => v.Selected).Take(1).ToList()
                : Options.Invoke(SearchTerm).Where(v => v.Selected).ToList();
        }

        public void Select(IOption option)
        {
            if (option.Placeholder) return;

            if (!Multiple)
            {
                Selected.Clear();
                Selected.Add(option);
            }
            else
            {
                if (IsSelected(option))
                    Selected.RemoveAll(v => v.ID.Equals(option.ID));
                else
                    Selected.Add(option);
            }

            if (CloseOnSelect == true)
            {
                Focused = false;
                StateHasChanged();
            }

            if (ClearSearchOnSelect)
                ClearSearch();
            OnUpdate?.Invoke(Selected);
        }

        public bool IsSelected(IOption option)
        {
            return !option.Placeholder && Selected.Any(v => v.ID.Equals(option.ID));
        }

        public void Deselect(IOption option)
        {
            if (option.Placeholder) return;

            if (!Multiple)
                Selected.Clear();
            else
                Selected.RemoveAll(v => v.ID.Equals(option.ID));

            OnUpdate?.Invoke(Selected);
        }

        private void Unfocus()
        {
            if (!Focused) return;

            Focused = false;
            StateHasChanged();
        }

        private void Focus()
        {
            if (Focused) return;

            Focused = true;
            StateHasChanged();
        }

        public void Search(ChangeEventArgs args)
        {
            Focused = true;

            var val = (string) args.Value;

            SearchTerm = string.IsNullOrEmpty(val) ? null : val;
            OnSearch?.Invoke(SearchTerm);
            StateHasChanged();
        }

        public static bool Match(string str, string term)
        {
            return str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public string ContainerClasses(string containerName)
        {
            var result = "";

            result += $"FlareSelect_{containerName} ";
            result += $"FlareSelect_{containerName}--{(!Focused ? "Unfocused" : "Focused")} ";
            result += $"FlareSelect_{containerName}--{(!Multiple ? "Single" : "Multiple")} ";
            result += $"FlareSelect_{containerName}--{(!Disabled ? "Enabled" : "Disabled")}";

            return result;
        }

        public void OnKeyUp(KeyboardEventArgs args, IOption option)
        {
            if (args.Key == "Enter") Select(option);
        }
    }
}