using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Superset.Common;

namespace FlareSelect
{
    internal sealed class SelectStateHandler
    {
        private readonly Events.OnUpdate                   _onUpdate;
        private readonly Events.OnSearch                   _onSearch;
        private readonly UpdateTrigger                     _triggerRefresh;
        private readonly FlareLib.FlareLib.StateHasChanged _stateHasChanged;

        internal readonly string InstanceID;

        private  bool         _focused;
        private  string       _searchTerm;
        internal List<Option> Selected;

        internal SelectStateHandler
        (
            Events.Options                    options,
            Events.Options                    filteredOptions,
            bool                              multiple,
            bool?                             closeOnSelect,
            bool                              disabled,
            Events.OnUpdate                   onUpdate,
            Events.OnSearch                   onSearch,
            UpdateTrigger                     triggerRefresh,
            FlareLib.FlareLib.StateHasChanged stateHasChanged
        )
        {
            Disabled        = disabled;
            Options         = options;
            OptionsFiltered = filteredOptions;
            Multiple        = multiple;
            CloseOnSelect   = closeOnSelect;

            _onUpdate        = onUpdate;
            _onSearch        = onSearch;
            _triggerRefresh  = triggerRefresh;
            _stateHasChanged = stateHasChanged;

            InstanceID = $"FlareSelect_{Guid.NewGuid().ToString().Replace("-", "")}";

            if (CloseOnSelect == null)
                CloseOnSelect = !Multiple;

            UpdateSelected();
            _onUpdate?.Invoke(Selected);

            if (_triggerRefresh != null)
            {
                // Don't invoke _onUpdate here. When someone manually invokes
                // _triggerRefresh.Trigger() they already know the selection was
                // updated.
                _triggerRefresh.OnUpdate += UpdateSelected; 
            }

            Global.ElementClickHandler.OnOuterClick += targetID =>
            {
                if (targetID == InstanceID) Unfocus();
            };

            Global.ElementClickHandler.OnInnerClick += targetID =>
            {
                if (targetID == InstanceID) Focus();
            };
        }

        public  bool           Disabled        { get; }
        private Events.Options Options         { get; }
        private Events.Options OptionsFiltered { get; }
        private bool           Multiple        { get; }
        private bool?          CloseOnSelect   { get; }

        internal IEnumerable<Option> Filtered
        {
            get
            {
                // ReSharper disable once RedundantIfElseBlock
                // ReSharper disable once MergeConditionalExpressionWhenPossible
                if (_searchTerm == null)
                {
                    return OptionsFiltered == null
                        ? Options.Invoke(_searchTerm).ToList()
                        : OptionsFiltered.Invoke(_searchTerm).ToList();
                }
                else
                {
                    return OptionsFiltered == null
                        ? Options.Invoke(_searchTerm).Where(v => Match(v.DropdownValue.ToString(), _searchTerm))
                        : OptionsFiltered.Invoke(_searchTerm);
                }
            }
        }

        private void UpdateSelected()
        {
            Selected = !Multiple
                ? Options.Invoke(_searchTerm).Where(v => v.Selected).Take(1).ToList()
                : Options.Invoke(_searchTerm).Where(v => v.Selected).ToList();
        }

        internal void Select(Option option)
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
                _focused = false;
                _stateHasChanged.Invoke();
            }

            _onUpdate?.Invoke(Selected);
        }

        internal bool IsSelected(Option option)
        {
            return !option.Placeholder && Selected.Any(v => v.ID.Equals(option.ID));
        }

        internal void Deselect(Option option)
        {
            if (option.Placeholder) return;

            if (!Multiple)
                Selected.Clear();
            else
                Selected.RemoveAll(v => v.ID.Equals(option.ID));

            _onUpdate?.Invoke(Selected);
        }

        internal void Unfocus()
        {
            if (!_focused) return;

            _focused = false;
            _stateHasChanged.Invoke();
        }

        internal void Focus()
        {
            if (_focused) return;

            _focused = true;
            _stateHasChanged.Invoke();
        }

        internal void Search(ChangeEventArgs args)
        {
            Console.WriteLine($"Search: {args.Value}");
            _focused = true;

            var val = (string) args.Value;

            _searchTerm = string.IsNullOrEmpty(val) ? null : val;
            _onSearch?.Invoke(_searchTerm);
        }

        private static bool Match(string str, string term)
        {
            return str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal string ContainerClasses(string containerName)
        {
            var result = "";

            result += $"FlareSelect_{containerName} ";
            result += $"FlareSelect_{containerName}--{(!_focused ? "Unfocused" : "Focused")} ";
            result += $"FlareSelect_{containerName}--{(!Multiple ? "Single" : "Multiple")} ";
            result += $"FlareSelect_{containerName}--{(!Disabled ? "Enabled" : "Disabled")}";

            return result;
        }
    }
}