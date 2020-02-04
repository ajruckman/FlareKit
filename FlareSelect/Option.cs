using System;

#nullable enable
namespace FlareSelect
{
    public sealed class Option : IEquatable<Option>
    {
        public object  ID           { get; set; }
        public string  Text         { get; set; }
        public string? SelectedText { get; set; }
        public bool    Selected     { get; set; }
        public bool    Disabled     { get; set; }
        public bool    Placeholder  { get; set; }

        public Option Clone() =>
            new Option
            {
                ID           = ID,
                Text         = Text,
                SelectedText = SelectedText,
                Selected     = Selected,
                Disabled     = Disabled,
                Placeholder  = Placeholder
            };

        public bool Equals(Option? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ID.Equals(other.ID);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Option other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}