using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FlareLib;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FlareSelect
{
    internal sealed class SelectStateHandler
    {
        private readonly Events.OnUpdate _onUpdate;
        private readonly bool            _disabled;

//        private readonly string _uid;

//        internal readonly string          InputUID;
        internal readonly List<Option> Selected;
        private           string       _searchTerm;
        internal          bool         _focused;
        private           IJSRuntime   _jsRuntime;

        internal readonly string InstanceID;

        internal SelectStateHandler(IEnumerable<Option> options,
                                    bool                multiple,
                                    bool?               closeOnSelect,
                                    bool                disabled,
                                    Events.OnUpdate     onUpdate,
//                                    ElementClickHandler elementClickHandler,
                                    IJSRuntime          jsRuntime)
        {
            _disabled           = disabled;
            Options             = options;
            Multiple            = multiple;
            CloseOnSelect       = closeOnSelect;
            _onUpdate           = onUpdate;
//            ElementClickHandler = elementClickHandler;
            _jsRuntime          = jsRuntime;

            InstanceID = $"FlareSelect_{Guid.NewGuid().ToString().Replace("-", "")}";

            Selected = !Multiple
                ? Options.Where(v => v.Selected).Take(1).ToList()
                : Options.Where(v => v.Selected).ToList();

            if (CloseOnSelect == null)
                CloseOnSelect = !Multiple;

            Global.ElementClickHandler.OnOuterClick += () =>
            {
                _focused = false;
                Global.ElementClickHandler.OuterClickHandled(InstanceID);
            };

//            ElementClickHandler.OnOuterClick += async () =>
//            {
//                Console.WriteLine($"111 {InstanceID} {_focused}");
//                if (!_focused) return;
//                Console.WriteLine("SelectStateHandler -> false");
//                _focused = false;
//                ElementClickHandler.OuterClickHandled(InstanceID);
//            };

            _onUpdate?.Invoke(Selected);
        }

        private IEnumerable<Option> Options { get; }

//        private ElementClickHandler ElementClickHandler { get; }
        private bool                Multiple            { get; }
        private bool?               CloseOnSelect       { get; }

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
                Console.WriteLine("CloseOnSelect -> false");
                _focused = false;
            }

//            // Block OnOuterClick if CloseOnSelect is null or false
//            if (CloseOnSelect == false)
//                ElementClickHandler.BlockOne();

            _onUpdate?.Invoke(Selected);
        }

        internal bool IsSelected(Option option)
        {
            return Selected.Any(v => v.ID == option.ID);
        }

//        private bool _wasDeselectedWhileFocused;
//        private bool _wasDeselectedWhileUnfocused;
//        private bool _wasSearchClickedWhileFocused;

        internal void Deselect(Option option)
        {
//            if (_focused)
//                _wasDeselectedWhileFocused = true;
//            else
//                _wasDeselectedWhileUnfocused = true;

            if (!Multiple)
                Selected.Clear();
            else
                Selected.RemoveAll(v => v.ID == option.ID);

            _onUpdate?.Invoke(Selected);
        }

        internal void Focus()
        {
            if (_focused) return;
//                if (_wasDeselectedWhileUnfocused)
//                {
//                    _wasDeselectedWhileUnfocused = false;
//                    return;
//                }

//            ElementClickHandler.OuterClick(); // Must come before _focused = true

            Console.WriteLine("Focus -> true");
            _focused = true;
        }

        internal void Search(ChangeEventArgs args)
        {
            _focused = true;

            var val = (string) args.Value;
            _searchTerm = string.IsNullOrEmpty(val) ? null : val;
        }

        internal void SearchClick()
        {
//            _wasSearchClickedWhileFocused = true;
//            // Don't close dialog when search input is clicked
//            ElementClickHandler.BlockOne();
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
            result += $"FlareSelect_{containerName}--{(!_disabled ? "Enabled" : "Disabled")}";

            return result;
        }
    }
}