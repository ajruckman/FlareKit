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
        private readonly string? _identifier;

        internal readonly UpdateTrigger ResetFilterValues  = new UpdateTrigger();
        internal readonly UpdateTrigger UpdateFilterValues = new UpdateTrigger();
        internal readonly UpdateTrigger UpdatePageState    = new UpdateTrigger();
        internal readonly UpdateTrigger UpdateTableBody    = new UpdateTrigger();
        internal readonly UpdateTrigger UpdateTableHead    = new UpdateTrigger();

        /// <summary>
        ///     Creates a FlareTable object without persistent values.
        /// </summary>
        public FlareTable(
            DataGetter   dataGetter,
            ValueGetter? valueGetter = null,
            bool         regexMode   = false,
            int          pageSize    = 25
        )
        {
            _dataGetter  = dataGetter;
            _valueGetter = valueGetter ?? ReflectionValueGetter;
            RegexMode    = regexMode;

            Current          = 0;
            PaginationRange  = 3;
            PageSize         = pageSize;
            _initialPageSize = pageSize;
        }

        /// <summary>
        ///     Creates a FlareTable object that accesses session storage to
        ///     load and store persistent values.
        /// </summary>
        public FlareTable(
            DataGetter             dataGetter,
            ISessionStorageService sessionStorage,
            string                 identifier,
            ValueGetter?           valueGetter = null,
            bool                   regexMode   = false,
            int                    pageSize    = 25
        )
        {
            _dataGetter     = dataGetter;
            _sessionStorage = sessionStorage;
            _identifier     = identifier;
            _valueGetter    = valueGetter ?? ReflectionValueGetter;

            RegexMode        = regexMode;
            Current          = 0;
            PaginationRange  = 3;
            PageSize         = pageSize;
            _initialPageSize = pageSize;
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

                // if (_sessionStorage != null)
                await StoreColumnConfig(c);
            }

            if (_initialPageSize != PageSize)
                await UpdatePageSize(_initialPageSize);

            await First();

            UpdateTableBody.Trigger();
            UpdateTableHead.Trigger();
            ResetFilterValues.Trigger();
        }
    }
}