#nullable enable

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace FT3
{
    public sealed class Column
    {
        [JsonIgnore]   internal          Regex?       CompiledFilterValue;
        [JsonIgnore]   public readonly   string       DisplayName;
        [JsonProperty] internal          string       FilterValue;
        [JsonIgnore]   internal          bool         FilterValueValid;
        [JsonIgnore]   public readonly   string       ID;
        [JsonIgnore]   internal          string?      Key;
        [JsonIgnore]   internal readonly PropertyInfo Property;
        [JsonProperty] public            bool         Shown;
        [JsonIgnore]   public            string       Width;
        [JsonIgnore]   public readonly   bool         Monospace;

        [JsonProperty] internal SortDirections SortDirection;
        [JsonProperty] internal int            SortIndex;

        private readonly Regex _matchSize =
            new Regex(
                @"^(0|auto|unset)$|^[+-]?[0-9]+(?:\.?([0-9]+))?(em|ex|%|px|cm|mm|in|pt|pc|ch|rem|vh|vw|vmin|vmax)$",
                RegexOptions.Compiled);

        public Column() { }

        public Column(
            string         id,
            string         displayName,
            bool           shown,
            string         width,
            bool           monospace,
            SortDirections sortDirection,
            int            sortIndex,
            string         filterValue,
            PropertyInfo   property
        )
        {
            if (!_matchSize.IsMatch(width))
                throw new ArgumentException($"Size '{width}' is not a valid CSS element size.", nameof(width));

            DisplayName   = displayName;
            FilterValue   = filterValue;
            ID            = id;
            Property      = property;
            Shown         = shown;
            Width         = width;
            Monospace     = monospace;
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