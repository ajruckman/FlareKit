using System;
using Superset.Common;

namespace FS3
{
    public class Option<T> : IOption<T> where T : IEquatable<T>
    {
        public T      ID           { get; set; }
        public string OptionText   { get; set; }
        public string SelectedText { get; set; }
        public bool   Selected     { get; set; }
        public bool   Disabled     { get; set; }
        public bool   Placeholder  { get; set; }
    }
}