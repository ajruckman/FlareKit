#nullable enable

using System;
using System.Threading.Tasks;
using Blazored.SessionStorage;
using Superset.Web.State;
using Superset.Logging;

// ReSharper disable ClassCanBeSealed.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

namespace FT3
{
    public partial class FlareTable<T>
    {
        private readonly string? _identifier;

        internal bool Complete { get; private set; }

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

        private void ExecutePending(bool force = false)
        {
            if (_loadingSessionValues && !force)
            {
                Log.Update("ExecutePending(): exiting because we are loading session values");
                return;
            }

            Log.Update(
                $"ExecutePending(): Rows: {_invalidateRowsPending} | Sort: {_invalidateSortPending} | Page: {_invalidatePagePending}");

            if (_invalidateRowsPending || force)
            {
                InvalidateRows();
                _invalidateRowsPending = false;
            }

            if (_invalidateSortPending || force)
            {
                InvalidateSort();
                _invalidateSortPending = false;
            }

            if (_invalidatePagePending || force)
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
            RowColorGetter? rowColorGetter = null,
            bool            clickable      = false
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
            _clickable       = clickable;

            Complete = true;

            RegisterEvents();
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
            RowColorGetter?        rowColorGetter = null,
            bool                   clickable      = false
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
            _clickable       = clickable;

            Complete = true;

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            OnColumnFilterUpdate     += () => Log.Update("OnColumnFilterUpdate ++");
            OnColumnSortUpdate       += () => Log.Update("OnColumnSortUpdate ++");
            OnColumnVisibilityUpdate += () => Log.Update("OnColumnVisibilityUpdate ++");
            OnRegexToggle            += () => Log.Update("OnRegexToggle ++");
            OnPageUpdate             += () => Log.Update("OnPageUpdate ++");
            OnPageSizeUpdate         += () => Log.Update("OnPageSizeUpdate ++");
            OnReset                  += () => Log.Update("OnReset ++");

            OnColumnFilterUpdate     += () => _invalidateRowsPending = true;
            OnColumnVisibilityUpdate += () => _invalidateRowsPending = true;
            OnRegexToggle            += () => _invalidateRowsPending = true;
            OnReset                  += () => _invalidateRowsPending = true;

            OnColumnSortUpdate += () => _invalidateSortPending = true;

            OnPageUpdate     += () => _invalidatePagePending = true;
            OnPageSizeUpdate += () => _invalidatePagePending = true;

            //

            UpdateTableControls.OnUpdate += () => Log.Update("UpdateTableControls.Trigger()");
            UpdateTableBody.OnUpdate     += () => Log.Update("UpdateTableBody.Trigger()");
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