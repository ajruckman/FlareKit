using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT3
{
    public partial class FlareTable<T>
    {
        internal static readonly int[] PageSizes = {10, 25, 50, 100, 250, 500};

        private int _current;

        private  int  _paginationRange;
        internal int  PageSize { get; set; }
        public   bool CanNext  => Current + 1 < NumPages;
        private  int  NumPages => (int) Math.Ceiling(_rowCount / (decimal) PageSize);
        public   bool CanPrev  => Current - 1 >= 0;
        internal int  Skip     => PageSize * Current;

        public int Current
        {
            get => _current;
            private set
            {
                _current = value;
                UpdateTableBody.Trigger();
            }
        }

        public string Info =>
            $"Showing {(_rowCount != 0 ? Skip + 1 : 0)} to {Math.Min(Skip + PageSize, _rowCount)} of {_rowCount:#,##0} | {NumPages} page" +
            (NumPages != 1 ? "s" : "");

        internal int RowCount
        {
            set
            {
                _rowCount = value;
                ResetCurrentPage();
            }
        }

        private int PaginationRange
        {
            get => _paginationRange;
            set
            {
                _paginationRange = value;
                ResetCurrentPage();
            }
        }

        private async Task ResetCurrentPage()
        {
            if (PageSize == 0 || Current < NumPages || NumPages == 0) return;
            Current = NumPages - 1;
            await SavePageNumber();
        }

        public async Task Next()
        {
            Current++;
            await SavePageNumber();
        }

        public async Task Previous()
        {
            Current--;
            await SavePageNumber();
        }

        public async Task First()
        {
            Current = 0;
            await SavePageNumber();
        }

        public async Task Last()
        {
            Current = NumPages - 1;
            await SavePageNumber();
        }

        public async Task Jump(int page)
        {
            Current = page > NumPages ? NumPages : page;
            await SavePageNumber();
        }

        private async Task SavePageNumber()
        {
            if (_sessionConfig)
                await _sessionStorage.SetItemAsync($"FlareTable_{_identifier}_!PageNum", Current);
        }

        public IEnumerable<int> Pages()
        {
            const int radius   = 3;
            const int diameter = 2 * radius + 1;
            const int offset   = (int) (diameter / 2.0);

            List<int> pages = new List<int>();

            int start, end;

            if (NumPages <= diameter)
            {
                start = 0;
                end   = Math.Max(NumPages - 3, NumPages);

                pages.AddRange(Enumerable.Range(start, end - start).ToList());
            }
            else if (Current <= offset)
            {
                start = 0;
                end   = diameter - 1;

                pages.AddRange(Enumerable.Range(start, end - start - 1).ToList());

                pages.Add(-1);
                pages.Add(NumPages - 1);
            }
            else if (Current + offset >= NumPages)
            {
                start = NumPages - diameter;
                end   = NumPages - 1;

                pages.Add(0);
                pages.Add(-1);

                pages.AddRange(Enumerable.Range(start + 2, end - start - 1).ToList());
            }
            else
            {
                start = Current - radius + 2;
                end   = Current + radius - 2;

                pages.Add(0);
                pages.Add(-1);

                pages.AddRange(Enumerable.Range(start, end - start + 1).ToList());

                if (Current == NumPages - radius - 1)
                    pages.Add(NumPages - 2);
                else
                    pages.Add(-1);

                pages.Add(NumPages - 1);
            }

            return pages;
        }

        public async Task UpdatePageSize(int size)
        {
            PageSize = size;
            UpdateTableHead.Trigger();
            UpdateTableBody.Trigger();

            if (_sessionConfig)
                await _sessionStorage.SetItemAsync($"FlareTable_{_identifier}_!PageSize", PageSize);
        }
    }
}