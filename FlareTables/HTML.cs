using System;
using Microsoft.AspNetCore.Components.Web;

#nullable enable

namespace FlareTables
{
    // ReSharper disable once ClassCanBeSealed.Global
    public partial class FlareTable<T>
    {
        private readonly bool            _monospace;
        private readonly bool            _fixedLayout;
        private readonly RowColorGetter? _rowColorGetter;
        private readonly bool            _clickable;

        public event Action<T> OnRowClick;

        /// <summary>
        /// Arg 1: The row clicked on.
        /// Arg 2: Whether the Ctrl key was pressed.
        /// Arg 3: Whether the Shift key was pressed.
        /// Arg 4: Whether the middle mouse button was used. 
        /// </summary>
        public event Action<T, bool, bool, bool> OnRowClickDetail;

        public string TableContainerClasses()
        {
            return "FlareTableContainer" + (_fixedLayout ? " FlareTableContainer--Fixed" : "");
        }

        public delegate RowColor RowColorGetter(T row);

        internal string RowClasses(T row)
        {
            if (row == null)
            {
                if (typeof(T).GetConstructor(Type.EmptyTypes) != null)
                    row = Activator.CreateInstance<T>();
                else
                    throw new ArgumentNullException(nameof(row),
                                                    "Row passed to RowClasses() was null and does not have a parameterless constructor; ensure you are adding the 'Value' parameter to FlareTableBodyRow components.");
            }

            // row ??= new T();

            // return "FlareTableBodyRow";
            // throw new ArgumentNullException(nameof(row), "Row passed to RowClasses() was null; ensure you are adding the 'Value' parameter to FlareTableBodyRow components.");

            var result = "FlareTableBodyRow";

            if (_rowColorGetter != null)
                result += _rowColorGetter.Invoke(row) switch
                {
                    RowColor.Undefined => "",
                    RowColor.Red       => " FlareTableBodyRow--Red",
                    RowColor.Green     => " FlareTableBodyRow--Green",
                    _                  => ""
                };

            if (_clickable)
                result += " FlareTableBodyRow--Clickable";

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

        public bool ColumnShown(string      id) => ((Column) _columns[id])?.Shown      ?? false;
        public bool ColumnFilterable(string id) => ((Column) _columns[id])?.Filterable ?? false;
        public bool ColumnSortable(string   id) => ((Column) _columns[id])?.Sortable   ?? false;

        // ReSharper disable once MemberCanBeInternal
        public string GetColumnFilter(string id)
        {
            return ((Column) _columns[id])?.FilterValue ?? "";
        }

        internal void RowClick(MouseEventArgs args, T row)
        {
            if (_clickable)
            {
                OnRowClick?.Invoke(row);
                OnRowClickDetail?.Invoke(row, args.CtrlKey, args.ShiftKey, false);
            }
        }

        internal void RowMouseUp(MouseEventArgs args, T row)
        {
            if (args.Button != 1) return;

            if (_clickable)
            {
                OnRowClick?.Invoke(row);
                OnRowClickDetail?.Invoke(row, args.CtrlKey, args.ShiftKey, true);
            }
        }
    }

    public enum RowColor
    {
        Undefined,
        Red,
        Green
    }
}