using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FlareSelect
{
    internal sealed class SelectStateHandler
    {
        private readonly  Events.OnUpdate _onUpdate;
        private readonly  bool            _disabled;
        internal readonly List<Option>    Selected;
        private           string          _searchTerm;
        internal          bool            _focused;
        private           IJSRuntime      _jsRuntime;
        private readonly FlareLib.FlareLib.StateHasChanged _stateHasChanged;

        internal readonly string InstanceID;

        internal SelectStateHandler(
            IEnumerable<Option>               options,
            bool                              multiple,
            bool?                             closeOnSelect,
            bool                              disabled,
            Events.OnUpdate                   onUpdate,
            IJSRuntime                        jsRuntime,
            FlareLib.FlareLib.StateHasChanged stateHasChanged
        )
        {
            _disabled     = disabled;
            Options       = options;
            Multiple      = multiple;
            CloseOnSelect = closeOnSelect;
            _onUpdate     = onUpdate;
            _jsRuntime    = jsRuntime;
            _stateHasChanged = stateHasChanged;

            InstanceID = $"FlareSelect_{Guid.NewGuid().ToString().Replace("-", "")}";

            Selected = !Multiple
                ? Options.Where(v => v.Selected).Take(1).ToList()
                : Options.Where(v => v.Selected).ToList();

            if (CloseOnSelect == null)
                CloseOnSelect = !Multiple;

            _onUpdate?.Invoke(Selected);

            Global.ElementClickHandler.OnOuterClick += (targetID) =>
            {
                if (targetID == InstanceID)
                {
                    Unfocus();
                }
            };
            
            Global.ElementClickHandler.OnInnerClick += (targetID) =>
            {
                if (targetID == InstanceID)
                {
                    Focus();
                }
            };
        }

        private IEnumerable<Option> Options { get; }

        private bool  Multiple      { get; }
        private bool? CloseOnSelect { get; }

        internal IEnumerable<Option> Filtered =>
            _searchTerm == null
                ? Options.ToList()
                : Options.Where(v => Match(v.DropdownValue.ToString(), _searchTerm)).ToList();

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
                    Selected.RemoveAll(v => v.ID == option.ID);
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
            return Selected.Any(v => v.ID == option.ID);
        }

        internal void Deselect(Option option)
        {
            if (!Multiple)
                Selected.Clear();
            else
                Selected.RemoveAll(v => v.ID == option.ID);

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
            _focused = true;

            var val = (string) args.Value;
            _searchTerm = string.IsNullOrEmpty(val) ? null : val;
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