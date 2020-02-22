using System.Timers;
using Microsoft.AspNetCore.Components;

namespace FT3.Component
{
    public partial class FlareTableFilter<T>
    {
        [CascadingParameter]
        public FlareTable<T> FlareTable { get; set; }

        [Parameter]
        public string ID { get; set; }

        private readonly object _debouncerLock = new object();
        private          Timer  _debouncer;
        private          string _filter;

        private void BeginDebounce(ChangeEventArgs args)
        {
            lock (_debouncerLock)
            {
                _filter = args.Value.ToString();

                if (_debouncer == null)
                {
                    _debouncer         =  new Timer(250) {AutoReset = false};
                    _debouncer.Elapsed += (_, __) => { FlareTable.SetColumnFilter(ID, _filter); };
                    _debouncer.Start();
                }
                else
                {
                    _debouncer.Enabled = false;
                    _debouncer.Enabled = true;
                }
            }
        }
    }
}