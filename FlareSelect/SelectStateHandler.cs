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
        private readonly  string       _uid;
        internal readonly string       InputUID;
        internal readonly List<Option> Selected;
        private           string       _searchTerm;
        internal          bool         Focused;

        internal SelectStateHandler(
            IEnumerable<Option>               options,
            bool                              multiple,
            bool?                             closeOnSelect,
            ElementClickHandler               elementClickHandler,
            FlareLib.FlareLib.StateHasChanged stateUpdater,
            IJSRuntime                        jsRuntime
        )
        {
            Options             = options;
            Multiple            = multiple;
            CloseOnSelect       = closeOnSelect;
            ElementClickHandler = elementClickHandler;

            _uid     = Guid.NewGuid().ToString();
            InputUID = _uid + "_Filter";

            Selected = !Multiple
                ? Options.Where(v => v.Selected).Take(1).ToList()
                : Options.Where(v => v.Selected).ToList();

            if (CloseOnSelect == null)
                CloseOnSelect = !Multiple;

            ElementClickHandler.OnBodyClick += () => Focused = false;

            ElementClickHandler.OnElementClick += source =>
            {
                if (source != _uid) return;

                Focused = true;
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
            Console.WriteLine("Select");
            if (CloseOnSelect == false)
                ElementClickHandler.BlockOne();

            if (!Multiple)
            {
                Selected.Clear();
                Selected.Add(option);
            }
            else if (Selected.Contains(option))
            {
                Selected.Remove(option);
            }
            else
            {
                Selected.Add(option);
            }
        }

        internal bool IsSelected(Option option) => Selected.Contains(option);

        internal void Deselect(Option option)
        {
            if (!Multiple)
                Selected.Clear();
            else
                Selected.Remove(option);

            ElementClickHandler.BlockOne();
            ElementClickHandler.NoninteractiveClicked(_uid);
        }

        internal void Focus()
        {
            if (!Focused)
            {
                ElementClickHandler.BodyClicked();
                ElementClickHandler.ElementClicked(_uid);
            }

            ElementClickHandler.BlockOne();
        }

        internal void Search(UIChangeEventArgs args)
        {
            Focused = true;

            var val = (string) args.Value;
            _searchTerm = string.IsNullOrEmpty(val) ? null : val;
        }

        internal void SearchClick()
        {
            ElementClickHandler.BlockOne();
        }

        private static bool Match(string str, string term)
        {
            return str?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}