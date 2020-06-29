#nullable enable

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace FlareTables
{
    public sealed class Column
    {
        [JsonIgnore]                  internal Regex?        CompiledFilterValue;
        [JsonIgnore]                  public   string        DisplayName;
        [JsonProperty("FilterValue")] internal string        FilterValue;
        [JsonIgnore]                  internal bool          FilterValueValid;
        [JsonIgnore]                  public   string        ID;
        [JsonIgnore]                  internal string?       Key;
        [JsonIgnore]                  internal PropertyInfo? Property;
        [JsonProperty]                public   bool          Shown;
        [JsonIgnore]                  public   string        Width;
        [JsonIgnore]                  public   bool          Monospace;
        [JsonIgnore]                  internal bool          Filterable;
        [JsonIgnore]                  internal bool          Sortable;

        [JsonProperty] internal SortDirections SortDirection;
        [JsonProperty] internal int            SortIndex;

        internal Column? Default;

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
            bool           filterable,
            bool           sortable,
            PropertyInfo?  property
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
            Filterable    = filterable;
            Sortable      = sortable;
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

        internal Column Clone()
        {
            return new Column
            {
                DisplayName   = DisplayName,
                FilterValue   = FilterValue,
                ID            = ID,
                Property      = Property,
                Shown         = Shown,
                Width         = Width,
                Monospace     = Monospace,
                Filterable    = Filterable,
                Sortable      = Sortable,
                SortDirection = SortDirection,
                SortIndex     = SortIndex,
                Default       = Default
            };
        }
    }
}