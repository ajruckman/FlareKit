#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Superset.Logging;

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FT3
{
    public partial class FlareTable<T>
    {
        internal readonly int[] PageSizes = {10, 25, 50, 100, 250, 500};
        private           int   _currentPage;
        internal          int   PageSize;

        public   bool CanPrev  => CurrentPage - 1 >= 0;
        public   bool CanNext  => CurrentPage + 1 < NumPages;
        private  int  NumPages => (int) Math.Ceiling(_rowCount / (decimal) PageSize);
        internal int  Skip     => PageSize * CurrentPage;

        public int CurrentPage
        {
            get => _currentPage;
            private set
            {
                _currentPage = value;
            }
        }

        internal string Info =>
            $"Showing {(_rowCount != 0 ? Skip + 1 : 0)} to {Math.Min(Skip + PageSize, _rowCount)} of {_rowCount:#,##0} | {NumPages} page" +
            (NumPages != 1 ? "s" : "");

        private int RowCount
        {
            set
            {
                Log.Update("[COMPONENT] RowCount.set");
                _rowCount = value;
#pragma warning disable 4014
                ResetCurrentPage();
#pragma warning restore 4014
            }
        }

        private async Task ResetCurrentPage()
        {
            Log.Update();
            if (PageSize == 0 || CurrentPage < NumPages || NumPages == 0) return;
            CurrentPage = NumPages - 1;
            
            OnPageUpdate.Invoke();
            ExecutePending();
            await SavePageNumber();
        }

        public async Task NextPage()
        {
            Log.Update();
            CurrentPage++;
            
            OnPageUpdate.Invoke();
            ExecutePending();
            await SavePageNumber();
        }

        public async Task PreviousPage()
        {
            Log.Update();
            CurrentPage--;
            
            OnPageUpdate.Invoke();
            ExecutePending();
            await SavePageNumber();
        }

        public async Task FirstPage()
        {
            Log.Update();
            CurrentPage = 0;
            
            OnPageUpdate.Invoke();
            ExecutePending();
            await SavePageNumber();
        }

        public async Task LastPage()
        {
            Log.Update();
            CurrentPage = NumPages - 1;
            
            OnPageUpdate.Invoke();
            ExecutePending();
            await SavePageNumber();
        }

        public async Task JumpToPage(int page)
        {
            Log.Update();
            CurrentPage = page > NumPages ? NumPages : page;
            
            OnPageUpdate.Invoke();
            ExecutePending();
            await SavePageNumber();
        }

        private async Task SavePageNumber()
        {
            if (_sessionStorage != null)
                await _sessionStorage.SetItemAsync($"FlareTable_{_identifier}_!PageNum", CurrentPage);
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
            else if (CurrentPage <= offset)
            {
                start = 0;
                end   = diameter - 1;

                pages.AddRange(Enumerable.Range(start, end - start - 1).ToList());

                pages.Add(-1);
                pages.Add(NumPages - 1);
            }
            else if (CurrentPage + offset >= NumPages)
            {
                start = NumPages - diameter;
                end   = NumPages - 1;

                pages.Add(0);
                pages.Add(-1);

                pages.AddRange(Enumerable.Range(start + 2, end - start - 1).ToList());
            }
            else
            {
                start = CurrentPage - radius + 2;
                end   = CurrentPage + radius - 2;

                pages.Add(0);
                pages.Add(-1);

                pages.AddRange(Enumerable.Range(start, end - start + 1).ToList());

                if (CurrentPage == NumPages - radius - 1)
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
            await ResetCurrentPage();

            if (_sessionStorage != null)
                await _sessionStorage.SetItemAsync($"FlareTable_{_identifier}_!PageSize", PageSize);
            
            OnPageSizeUpdate.Invoke();
            ExecutePending();
        }
    }
}