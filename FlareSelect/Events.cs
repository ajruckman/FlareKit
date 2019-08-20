using System;
using System.Collections.Generic;

namespace FlareSelect
{
    public static class Events
    {
        public delegate void OnUpdate(List<Option> selected);
    }
}