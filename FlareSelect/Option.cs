#nullable enable
namespace FlareSelect
{
    public sealed class Option
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
    }
}