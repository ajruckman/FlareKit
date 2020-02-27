#nullable enable

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace FT3
{
    public sealed class Column
    {
        [JsonIgnore]   internal Regex?       CompiledFilterValue;
        [JsonIgnore]   public   string       DisplayName;
        [JsonProperty] internal string       FilterValue;
        [JsonIgnore]   internal bool         FilterValueValid;
        [JsonIgnore]   public   string       ID;
        [JsonIgnore]   internal string?      Key;
        [JsonIgnore]   internal PropertyInfo Property;
        [JsonProperty] public   bool         Shown;

        [JsonProperty] internal SortDirections SortDirection;
        [JsonProperty] internal int            SortIndex;

        public Column(
            string         id,
            string         displayName,
            bool           shown,
            SortDirections sortDirection,
            int            sortIndex,
            string         filterValue,
            PropertyInfo   property
        )
        {
            DisplayName   = displayName;
            FilterValue   = filterValue;
            ID            = id;
            Property      = property;
            Shown         = shown;
            SortDirection = sortDirection;
            SortIndex     = sortIndex;
        }

        internal void TryCompileFilter()
        {
            try
            {
                CompiledFilterValue = new Regex(FilterValue, RegexOptions.Compiled);
                FilterValueValid    = true;
            }
            catch (ArgumentException)
            {
                CompiledFilterValue = null;
                FilterValueValid    = false;
            }
        }
    }
}