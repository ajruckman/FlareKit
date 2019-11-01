using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FlareSelect
{
    internal sealed class SelectStateHandler
    {
        private readonly bool                              _disabled;
        private readonly Events.OnSearch                   _onSearch;
        private readonly Events.OnUpdate                   _onUpdate;
        private readonly FlareLib.FlareLib.StateHasChanged _stateHasChanged;

        internal readonly string       InstanceID;
        internal readonly List<Option> Selected;
        internal          bool         _focused;
        private           string       _searchTerm;

        internal SelectStateHandler(Events.Options               options,
                                    bool                              multiple,
                                    bool?                             closeOnSelect,
                                    bool                              disabled,
                                    Events.OnUpdate                   onUpdate,
                                    Events.OnSearch                   onSearch,
                                    FlareLib.FlareLib.StateHasChanged stateHasChanged)
        {
            _disabled        = disabled;
            Options          = options;
            Multiple         = multiple;
            CloseOnSelect    = closeOnSelect;
            _onUpdate        = onUpdate;
            _onSearch        = onSearch;
            _stateHasChanged = stateHasChanged;

            InstanceID = $"FlareSelect_{Guid.NewGuid().ToString().Replace("-", "")}";

            Selected = !Multiple
                ? Options.Invoke().Where(v => v.Selected).Take(1).ToList()
                : Options.Invoke().Where(v => v.Selected).ToList();

            if (CloseOnSelect == null)
                CloseOnSelect = !Multiple;

            _onUpdate?.Invoke(Selected);

            Global.ElementClickHandler.OnOuterClick += targetID =>
            {
                if (targetID == InstanceID) Unfocus();
            };

            Global.ElementClickHandler.OnInnerClick += targetID =>
            {
                if (targetID == InstanceID) Focus();
            };
        }

        private Events.Options Options { get; }

        private bool                 Multiple      { get; }
        private bool?                CloseOnSelect { get; }
//        public  Events.TriggerSearch TriggerSearch { get; }

        internal IEnumerable<Option> Filtered =>
            _searchTerm == null
                ? Options.Invoke().ToList()
                : Options.Invoke().Where(v => Match(v.DropdownValue.ToString(), _searchTerm)).ToList();

        internal void Select(Option option)
        {
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
            return Selected.Any(v => v.ID.Equals(option.ID));
        }

        internal void Deselect(Option option)
        {
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
//            Console.WriteLine($"Search: {args.Value}");
            _focused = true;

            var val = (string) args.Value;

            _searchTerm = string.IsNullOrEmpty(val) ? null : val;
            _onSearch?.Invoke(_searchTerm);
        }

        internal void SearchClick() { }

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
            result += $"FlareSelect_{containerName}--{(!_disabled ? "Enabled" : "Disabled")}";

            return result;
        }
    }
}