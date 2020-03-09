#nullable enable

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
        internal readonly UpdateTrigger OnColumnToggle     = new UpdateTrigger();
        internal readonly UpdateTrigger OnColumnFilter     = new UpdateTrigger();
        internal readonly UpdateTrigger OnPagination       = new UpdateTrigger();
        internal readonly UpdateTrigger OnDataChange       = new UpdateTrigger();
        internal readonly UpdateTrigger OnViewChange       = new UpdateTrigger();
        internal readonly UpdateTrigger OnPaginationResize = new UpdateTrigger();

        private readonly string? _identifier;

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

            Current          = 0;
            PageSize         = pageSize;
            _initialPageSize = pageSize;
            _monospace       = monospace;
            _fixedLayout     = fixedLayout;
            _rowColorGetter  = rowColorGetter;
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
            Current          = 0;
            PageSize         = pageSize;
            _initialPageSize = pageSize;
            _monospace       = monospace;
            _fixedLayout     = fixedLayout;
            _rowColorGetter  = rowColorGetter;
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

            await First();

            OnColumnToggle.Trigger();
        }
    }
}