using System;
using System.Collections.Generic;
using Superset.Common;

namespace FlareSelect
{
    
    // TODO: Keyboard control
    // TODO: Close (OnOuterClick) anywhere on page
    public static class Events
    {
        public delegate void OnUpdate(List<Option> selected);

        public delegate void OnSearch(string searchTerm);
        
        public delegate IEnumerable<Option> Options(string searchTerm);
        // public delegate IEnumerable<Option> OptionsFiltered(string searchTerm);
    }
}