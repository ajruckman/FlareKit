#nullable enable

using System;
using System.Threading.Tasks;
using Blazored.SessionStorage;
using Superset.Common;

// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

namespace FT3
{
    public partial class FlareTable<T>
    {
        private readonly string? _identifier;

        internal event Action OnColumnFilterUpdate;     // +
        internal event Action OnColumnSortUpdate;       // +
        internal event Action OnColumnVisibilityUpdate; // +
        internal event Action OnRegexToggle;            // +
        internal event Action OnPageUpdate;             // +
        internal event Action OnPageSizeUpdate;         // +
        internal event Action OnReset;                  // +

        private bool _invalidateRowsPending;
        private bool _invalidateSortPending;
        private bool _invalidatePagePending;

        private void ExecutePending()
        {
            if (_loadingSessionValues)
                return;
            
            if (_invalidateRowsPending)
            {
                InvalidateRows();
                _invalidateRowsPending = false;
            }

            if (_invalidateSortPending)
            {
                InvalidateSort();
                _invalidateSortPending = false;
            }

            if (_invalidatePagePending)
            {
                InvalidatePage();
                _invalidatePagePending = false;
            }
        }

        internal readonly UpdateTrigger UpdateTableControls = new UpdateTrigger();
        internal readonly UpdateTrigger UpdateTableBody     = new UpdateTrigger();

        /// <summary>
        ///     Creates a FlareTable object without persistent values.
        /// </summary>
        public FlareTable(
            DataGetter      dataGetter,
            ValueGetter?    valueGetter    = null,
            bool            regexMode      = false,
            int             pageSize       = 25,
            bool            monospace      = false,
            bool            fixedLayout    = false,
            RowColorGetter? rowColorGetter = null
        )
        {
            _dataGetter  = dataGetter;
            _valueGetter = valueGetter ?? ReflectionValueGetter;
            RegexMode    = regexMode;

            CurrentPage      = 0;
            PageSize         = pageSize;
            _initialPageSize = pageSize;
            _monospace       = monospace;
            _fixedLayout     = fixedLayout;
            _rowColorGetter  = rowColorGetter;

            Events();
        }

        /// <summary>
        ///     Creates a FlareTable object that accesses session storage to
        ///     load and store persistent values.
        /// </summary>
        public FlareTable(
            DataGetter             dataGetter,
            ISessionStorageService sessionStorage,
            string                 identifier,
            ValueGetter?           valueGetter    = null,
            bool                   regexMode      = false,
            int                    pageSize       = 25,
            bool                   monospace      = false,
            bool                   fixedLayout    = false,
            RowColorGetter?        rowColorGetter = null
        )
        {
            _dataGetter     = dataGetter;
            _sessionStorage = sessionStorage;
            _identifier     = identifier;
            _valueGetter    = valueGetter ?? ReflectionValueGetter;

            RegexMode        = regexMode;
            CurrentPage      = 0;
            PageSize         = pageSize;
            _initialPageSize = pageSize;
            _monospace       = monospace;
            _fixedLayout     = fixedLayout;
            _rowColorGetter  = rowColorGetter;

            Events();
        }

        private void Events()
        {
            OnColumnFilterUpdate     += () => Console.WriteLine("OnColumnFilterUpdate ++");
            OnColumnSortUpdate       += () => Console.WriteLine("OnColumnSortUpdate ++");
            OnColumnVisibilityUpdate += () => Console.WriteLine("OnColumnVisibilityUpdate ++");
            OnRegexToggle            += () => Console.WriteLine("OnRegexToggle ++");
            OnPageUpdate             += () => Console.WriteLine("OnPageUpdate ++");
            OnPageSizeUpdate         += () => Console.WriteLine("OnPageSizeUpdate ++");
            OnReset                  += () => Console.WriteLine("OnReset ++");

            OnColumnFilterUpdate     += () => _invalidateRowsPending = true;
            OnColumnVisibilityUpdate += () => _invalidateRowsPending = true;
            OnRegexToggle            += () => _invalidateRowsPending = true;
            OnReset                  += () => _invalidateRowsPending = true;

            OnColumnSortUpdate += () => _invalidateSortPending = true;

            OnPageUpdate     += () => _invalidatePagePending = true;
            OnPageSizeUpdate += () => _invalidatePagePending = true;

            //

            UpdateTableControls.OnUpdate += () => Console.WriteLine("UpdateTableControls.Trigger()");
            UpdateTableBody.OnUpdate     += () => Console.WriteLine("UpdateTableBody.Trigger()");
        }

        public async Task Reset()
        {
            if (RegexMode)
                await ToggleRegexMode();

            foreach (Column? c in _columns.Values)
            {
                if (c == null) continue;

                c.Shown         = true;
                c.FilterValue   = "";
                c.SortDirection = SortDirections.Neutral;
                c.SortIndex     = 0;

                if (RegexMode)
                    c.TryCompileFilter();

                await StoreColumnConfig(c);
            }

            if (_initialPageSize != PageSize)
            {
                await UpdatePageSize(_initialPageSize);
            }

            await FirstPage();
        }
    }
}