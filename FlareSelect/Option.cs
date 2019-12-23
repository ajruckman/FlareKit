namespace FlareSelect
{
    public sealed class Option
    {
        public object ID            { get; set; }
        public object DropdownValue { get; set; }
        public object SelectedValue { get; set; }
        public bool   Selected      { get; set; }
        public bool   Placeholder   { get; set; }

        public Option Clone() =>
            new Option
            {
                ID            = ID,
                DropdownValue = DropdownValue,
                SelectedValue = SelectedValue,
                Selected      = Selected
            };
    }
}