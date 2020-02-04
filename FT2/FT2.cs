using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

// ReSharper disable InconsistentNaming

namespace FT2
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class FT2<T> : ComponentBase
    where T : IEquatable<T>
    {
        public delegate IEnumerable<TData> TypedData<out TData>();

        [Parameter]
        public TypedData<T> Data { get; set; }

        public string SearchTerm { get; set; }

        // Content parameters

        [Parameter]
        public RenderFragment Header { get; set; }

        [Parameter]
        public RenderFragment SearchBar { get; set; }

        [Parameter]
        public RenderFragment<T> Row { get; set; }

        [Parameter]
        public RenderFragment Footer { get; set; }

        // --

        public FT2(
            TypedData<T>      data,
            RenderFragment<T> row,
            RenderFragment    header    = null,
            RenderFragment    searchBar = null,
            RenderFragment    footer    = null
        )
        {
            Data      = data;
            Header    = header;
            SearchBar = searchBar;
            Row       = row;
            Footer    = footer;
        }

        public FT2() { }

        private void Fragment(RenderTreeBuilder builder)
        {
            int seq = -1;

            ++seq;
            if (Header != null)
                builder.AddContent(seq, Header);

            ++seq;
            if (SearchBar != null)
                builder.AddContent(seq, SearchBar);

            foreach (T data in Data.Invoke())
                builder.AddContent(++seq, Row(data));

            ++seq;
            if (Footer != null)
                builder.AddContent(seq, Footer);
        }

        public RenderFragment Render()
        {
            return Fragment;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            Fragment(builder);
        }
    }
}