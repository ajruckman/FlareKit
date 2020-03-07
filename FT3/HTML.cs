using System;

#nullable enable

namespace FT3
{
    // ReSharper disable once ClassCanBeSealed.Global
    public partial class FlareTable<T>
    {
        private readonly bool            _monospace;
        private readonly bool            _fixedLayout;
        private readonly RowColorGetter? _rowColorGetter;

        public string TableContainerClasses()
        {
            return "FlareTableContainer" + (_fixedLayout ? " FlareTableContainer--Fixed" : "");
        }

        public delegate RowColor RowColorGetter(T row);

        internal string RowClasses(T row)
        {
            var result = "FlareTableBodyRow";

            if (_rowColorGetter != null)
                result += _rowColorGetter.Invoke(row) switch
                {
                    RowColor.Undefined => "",
                    RowColor.Red       => " FlareTableBodyRow--Red",
                    RowColor.Green     => " FlareTableBodyRow--Green",
                    _                  => ""
                };

            return result;
        }

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

        internal string ColumnWidth(string id)
        {
            return ((Column) _columns[id]).Width;
        }

        internal string CellClasses(string id)
        {
            return "FlareTableCell" + (((Column) _columns[id]).Monospace || _monospace ? " FlareTableCell--Mono" : "");
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

    public enum RowColor
    {
        Undefined,
        Red,
        Green
    }
}