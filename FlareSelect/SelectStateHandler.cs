using System;
using System.Collections.Generic;
using System.Linq;
using FlareLib;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FlareSelect
{
    internal sealed class SelectStateHandler
    {
        private readonly  Events.OnUpdate                   _onUpdate;
        private readonly  bool                              _disabled;
        private readonly  string                            _uid;
        internal readonly string                            InputUID;
        internal readonly List<Option>                      Selected;
        private           string                            _searchTerm;
        private           bool                              _focused;

        internal SelectStateHandler(IEnumerable<Option>               options,
                                    bool                              multiple,
                                    bool?                             closeOnSelect,
                                    bool                              disabled,
                                    Events.OnUpdate                   onUpdate,
                                    ElementClickHandler               elementClickHandler,
                                    IJSRuntime                        jsRuntime)
        {
            _disabled           = disabled;
            Options             = options;
            Multiple            = multiple;
            CloseOnSelect       = closeOnSelect;
            _onUpdate           = onUpdate;
            ElementClickHandler = elementClickHandler;

            _uid     = Guid.NewGuid().ToString();
            InputUID = _uid + "_Filter";

            Selected = !Multiple
                ? Options.Where(v => v.Selected).Take(1).ToList()
                : Options.Where(v => v.Selected).ToList();

            if (CloseOnSelect == null)
                CloseOnSelect = !Multiple;

            ElementClickHandler.OnOuterClick += () => _focused = false;

            ElementClickHandler.OnReactiveClick += source =>
            {
                if (source != _uid) return;

                _focused = true;

                // Focus search input when dialog is opened
                jsRuntime.InvokeAsync<object>("FlareSelect.focusElement", InputUID);
            };
        }

        private IEnumerable<Option> Options { get; }

        private ElementClickHandler ElementClickHandler { get; }
        private bool                Multiple            { get; }
        private bool?               CloseOnSelect       { get; }

        internal IEnumerable<Option> Filtered =>
            _searchTerm == null
                ? Options.ToList()
                : Options.Where(v => Match(v.DropdownValue.ToString(), _searchTerm)).ToList();

        internal void Select(Option option)
        {
            Console.WriteLine("Select ---");

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

            // Block OnOuterClick if CloseOnSelect is null or false
            if (CloseOnSelect == false)
                ElementClickHandler.BlockOne();

            _onUpdate?.Invoke(Selected);
        }

        internal bool IsSelected(Option option)
        {
            return Selected.Any(v => v.ID == option.ID);
        }

        internal void Deselect(Option option)
        {
            Console.WriteLine("Deselect ---");

            // If focused, don't block subsequent Focus() calls
            if (!_focused)
            {
                // Never open dialog on deselect
                ElementClickHandler.BlockOne();
                ElementClickHandler.BlockOne();
            }

            if (!Multiple)
                Selected.Clear();
            else
                Selected.RemoveAll(v => v.ID == option.ID);

            _onUpdate?.Invoke(Selected);
        }

        internal void Focus()
        {
            Console.WriteLine("Focus ---");

            if (!_focused)
            {
                // Close others
                ElementClickHandler.OuterClick();

                // Should open dialog
                ElementClickHandler.ReactiveClick(_uid);
            }

            // Block OnOuterClick
            ElementClickHandler.BlockOne();
        }

        internal void Search(UIChangeEventArgs args)
        {
            _focused = true;

            var val = (string) args.Value;
            _searchTerm = string.IsNullOrEmpty(val) ? null : val;
        }

        internal void SearchClick()
        {
            Console.WriteLine("SearchClick ---");

            // Don't close dialog when search input is clicked
            ElementClickHandler.BlockOne();
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