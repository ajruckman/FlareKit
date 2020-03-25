using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Superset.Common;
using Superset.Web.Listeners;
using Superset.Web.State;

namespace FS3
{
    // ReSharper disable once InconsistentNaming
    public partial class __FlareSelector<T> where T : IEquatable<T>
    {
        private readonly UpdateTrigger _onToggle = new UpdateTrigger();

        private string _id;
        private bool   _justFocused;
        private ClickListener _optionsClickListener;
        private KeyListener   _optionsKeyListener;
        private ElementReference _optionsRef;

        private ClickListener _selectedClickListener;
        private KeyListener   _selectedKeyListener;

        private ElementReference _selectedRef;
        private bool   _shown = true;

        [Parameter]
        public FlareSelector<T> FlareSelector { get; set; }

        protected override void OnInitialized()
        {
            _id = Guid.NewGuid().ToString().Replace("-", "");


            Console.WriteLine("Initialized");
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                _selectedClickListener = new ClickListener(_selectedRef);
                _optionsClickListener  = new ClickListener(_optionsRef);
                _selectedKeyListener   = new KeyListener(_selectedRef);
                _optionsKeyListener    = new KeyListener(_optionsRef);

                _selectedClickListener.OnClick      += OnSelectedClick;
                _selectedClickListener.OnInnerClick += OnSelectedInnerClick;
                _optionsClickListener.OnOuterClick  += OnOptionsOuterClick;
                _optionsClickListener.OnInnerClick  += OnOptionsInnerClick;

                _selectedKeyListener.OnKeyUp     += OnSelectedKeyUp;
                _optionsKeyListener.OnInnerKeyUp += OnOptionsInnerKeyUp;

                _selectedClickListener.Initialize(JSRuntime);
                _optionsClickListener.Initialize(JSRuntime);
                _selectedKeyListener.Initialize(JSRuntime);
                _optionsKeyListener.Initialize(JSRuntime);
            }
        }

        private void OnSelectedClick(ClickListener.ClickArgs args)
        {
            if (args.Button != 0) return;

            _justFocused = true;
            Console.WriteLine("OnSelectedClick");
            _shown = !_shown;
            _onToggle.Trigger();
        }

        private void OnSelectedInnerClick(ClickListener.ClickArgs args)
        {
            if (args.Button != 0) return;
            Console.WriteLine("OnSelectedInnerClick");

            if (FlareSelector.Multiple)
                _justFocused = true;
            else
            {
                _shown = !_shown;
                _onToggle.Trigger();
            }
        }

        private void OnOptionsOuterClick(ClickListener.ClickArgs args)
        {
            if (args.Button != 0) return;

            Console.WriteLine("OnOptionsOuterClick -> " + _justFocused);
            // if (_justFocused)
            // {
            //     _justFocused = false;
            //     return;
            // }
            //
            // _shown = false;
            // _onToggle.Trigger();
        }

        private void OnOptionsInnerClick(ClickListener.ClickArgs args)
        {
            if (args.Button != 0 || args.TargetID == "") return;

            Console.WriteLine("OnOptionsInnerClick -> " + args.TargetID);

            string[] ids = args.TargetID.Remove(0, 5).Split('_');
            Console.WriteLine("Option: " + ids[0] + "." + ids[1]);

            // int i = int.Parse(args.TargetID.Split("FS_O_")[1]);
            // Console.WriteLine(i);

            FlareSelector.Select(int.Parse(ids[0]), int.Parse(ids[1]));

            if (FlareSelector.CloseOnSelect)
            {
                _shown = false;
                _onToggle.Trigger();
            }
        }

        private void OnSelectedKeyUp(KeyListener.KeyArgs args)
        {
            Console.WriteLine("OnSelectedKeyUp -> " + args.Key);

            if (args.Key == "Enter")
            {
                _shown = !_shown;
                _onToggle.Trigger();
            }
            else if (args.Key == "Escape")
            {
                _shown = false;
                _onToggle.Trigger();
            }
        }

        private void OnOptionsInnerKeyUp(KeyListener.KeyArgs args)
        {
            Console.WriteLine("OnOptionsInnerKeyUp -> " + args.Key);

            if (args.Key == "Enter")
            {
                string[] ids = args.TargetID.Remove(0, 5).Split('_');
                Console.WriteLine(ids[0] + "." + ids[1]);

                FlareSelector.Select(int.Parse(ids[0]), int.Parse(ids[1]));
            }
            else if (args.Key == "Escape")
            {
                _shown = false;
                _onToggle.Trigger();
            }
        }

        private void UpdateFilterValue(ChangeEventArgs obj)
        {
            FlareSelector.UpdateFilterValue(obj.Value.ToString() ?? "");
            _shown = true;
            _onToggle.Trigger();
        }

        private void OnOptionsKeypress(KeyboardEventArgs args)
        {
            Console.WriteLine(args.Key);
        }

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