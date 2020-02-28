#nullable enable

namespace FT3
{
    // ReSharper disable once ClassCanBeSealed.Global
    public partial class FlareTable<T>
    {
        // ReSharper disable once MemberCanBeInternal
        public string GetColumnDisplayName(string id)
        {
            return ((Column) _columns[id]).DisplayName;
        }

        internal string ColumnSortButtonClass(string id)
        {
            return ((Column) _columns[id]).SortDirection switch
            {
                SortDirections.Neutral    => "FlareTableFilter_SortButton--Neutral",
                SortDirections.Ascending  => "FlareTableFilter_SortButton--Ascending",
                SortDirections.Descending => "FlareTableFilter_SortButton--Descending",
                _                         => ""
            };
        }

        internal string ColumnFilterValueValidClass(string id)
        {
            return !RegexMode
                ? "FlareTableFilter_Input--Valid"
                : ((Column) _columns[id]).FilterValueValid
                    ? "FlareTableFilter_Input--Valid"
                    : "FlareTableFilter_Input--Invalid";
        }

        internal string ColumnSortButtonContent(string id)
        {
            return ((Column) _columns[id]).SortDirection switch
            {
                SortDirections.Neutral    => "↕",
                SortDirections.Ascending  => "↑",
                SortDirections.Descending => "↓",
                _                         => ""
            };
        }

        // ReSharper disable once MemberCanBeInternal
        public bool ColumnShown(string id)
        {
            return ((Column) _columns[id])?.Shown ?? false;
        }

        // ReSharper disable once MemberCanBeInternal
        public string GetColumnFilter(string id)
        {
            return ((Column) _columns[id])?.FilterValue ?? "";
        }
    }
}