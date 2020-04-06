#nullable enable

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Superset.Common;
using Superset.Web.Listeners;
using Superset.Web.State;
using Superset.Web.Utilities;

#pragma warning disable 649

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace FS3
{
    public partial class __FlareSelector<T> where T : IEquatable<T>
    {
        private readonly UpdateTrigger _onToggle                 = new UpdateTrigger();
        private readonly UpdateTrigger _onFilterValueValidChange = new UpdateTrigger();

        private bool _justFocused;
        private bool _shown;

        private ElementReference _selectedRef;
        private ClickListener    _selectedClickListener;
        private KeyListener      _selectedKeyListener;

        private ElementReference _optionsRef;
        private ClickListener    _optionsClickListener;
        private KeyListener      _optionsKeyListener;

        private ElementReference _inputRef;
        private ClickListener    _inputClickListener;
        private KeyListener      _inputKeyListener;

        [Parameter]
        public FlareSelector<T> FlareSelector { get; set; }

        protected override void OnInitialized()
        {
            _validFilterValueLength = IsFilterValueValid();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender) return;

            // Selected - clicks
            _selectedClickListener              =  new ClickListener(_selectedRef);
            _selectedClickListener.OnClick      += OnSelectedClick;
            _selectedClickListener.OnInnerClick += OnSelectedInnerClick;
            _selectedClickListener.Execute(JSRuntime);

            // Selected - keys
            _selectedKeyListener         =  new KeyListener(_selectedRef);
            _selectedKeyListener.OnKeyUp += OnSelectedKeyUp;
            _selectedKeyListener.Execute(JSRuntime);

            // Options - clicks
            _optionsClickListener              =  new ClickListener(_optionsRef);
            _optionsClickListener.OnOuterClick += OnOptionsOuterClick;
            _optionsClickListener.OnInnerClick += OnOptionsInnerClick;
            _optionsClickListener.Execute(JSRuntime);

            // Options - keys
            _optionsKeyListener              =  new KeyListener(_optionsRef);
            _optionsKeyListener.OnInnerKeyUp += OnOptionsInnerKeyUp;
            _optionsKeyListener.Execute(JSRuntime);

            // Input - clicks
            _inputClickListener         =  new ClickListener(_inputRef);
            _inputClickListener.OnClick += OnInputClick;
            _inputClickListener.Execute(JSRuntime);

            // Input - keys
            if (FlareSelector.Multiple)
            {
                _inputKeyListener         =  new KeyListener(_inputRef);
                _inputKeyListener.OnKeyUp += OnInputKeyUp;
                _inputKeyListener.Execute(JSRuntime);
            }
        }

        private void OnSelectedClick(ClickListener.ClickArgs args)
        {
            if (args.Button != 0) return;

            _shown       = !_shown;
            _justFocused = true;
            _onToggle.Trigger();

            if (FlareSelector.Multiple)
                Utilities.FocusElement(JSRuntime, _inputRef);
        }

        private void OnSelectedInnerClick(ClickListener.ClickArgs args)
        {
            if (args.Button != 0) return;
            
            _justFocused = true;

            if (!FlareSelector.AnySelected() || !FlareSelector.Multiple)
            {
                _shown       = !_shown;
                _justFocused = true;
                _onToggle.Trigger();

                Utilities.FocusElement(JSRuntime, _inputRef);
            }
        }

        private void OnSelectedKeyUp(KeyListener.KeyArgs args)
        {
            if (args.Key == "Escape" && _shown)
            {
                Console.WriteLine("escape OnSelectedKeyUp");
                _shown = false;
                _onToggle.Trigger();
            }
            else if (args.Key == "Enter")
            {
                _shown = !_shown;
                _onToggle.Trigger();
            }
        }

        private void OnOptionsOuterClick(ClickListener.ClickArgs args)
        {
            if (args.Button != 0) return;

            if (_justFocused)
            {
                _justFocused = false;
                return;
            }

            _shown = false;
            _onToggle.Trigger();
        }

        private void OnOptionsInnerClick(ClickListener.ClickArgs args)
        {
            if (args.Button != 0 || args.TargetID == "") return;

            string[] ids = args.TargetID.Remove(0, 5).Split('_');

            Select(int.Parse(ids[0]), int.Parse(ids[1]));
        }


        private void OnOptionsInnerKeyUp(KeyListener.KeyArgs args)
        {
            if (args.Key == "Escape" && _shown)
            {
                Console.WriteLine("escape OnOptionsInnerKeyUp");
                _shown = false;
                _onToggle.Trigger();
            }
            else if (args.Key == "Enter")
            {
                string[] ids = args.TargetID.Remove(0, 5).Split('_');
                Select(int.Parse(ids[0]), int.Parse(ids[1]));
            }
        }

        private void OnInputClick(ClickListener.ClickArgs args)
        {
            if (_shown) return;
            _shown = true;
            _onToggle.Trigger();
        }

        private void OnInputKeyUp(KeyListener.KeyArgs args)
        {
            if (args.Key == "Escape" && _shown)
            {
                _shown = false;
                _onToggle.Trigger();
            }
        }

        private void Select(int batchID, int optionIndex)
        {
            FlareSelector.Select(batchID, optionIndex);
            if (FlareSelector.CloseOnSelect)
            {
                _shown = false;
                _onToggle.Trigger();
            }
            else if (FlareSelector.Multiple)
            {
                Utilities.FocusElement(JSRuntime, _inputRef);
            }
        }

        private bool _validFilterValueLength;

        private void UpdateFilterValue(ChangeEventArgs obj)
        {
            FlareSelector.UpdateFilterValue(obj.Value.ToString() ?? "");

            bool validNow = IsFilterValueValid();
            if (_validFilterValueLength != validNow)
            {
                _validFilterValueLength = validNow;
                _onFilterValueValidChange.Trigger();
            }

            _shown = true;
            _onToggle.Trigger();
        }

        private bool IsFilterValueValid()
        {
            return FlareSelector.MinFilterValueLength == null ||
                   FlareSelector.FilterValue.Length   >= FlareSelector.MinFilterValueLength.Value;
        }

        private void OnOptionsKeypress(KeyboardEventArgs args) { }

        private string OptionClasses(IOption<T> option)
        {
            if (FlareSelector.IsOptionSelected(option))
                return "FlareSelect_Option--Selected";
            if (option.Placeholder)
                return "FlareSelect_Option--Placeholder";
            if (option.Disabled)
                return "FlareSelect_Option--Disabled";

            return "";
        }

        private string OptionID(IOption<T> option, int batchID, int optionIndex)
        {
            if (option.Placeholder || option.Disabled)
                return "";

            return $"FS_O_{batchID}_{optionIndex}";
        }
    }
}