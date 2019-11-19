using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Superset.Common;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace FlareSelect
{
    public class TriggerWrapper : ComponentBase
    {
        [Parameter] public RenderFragment  ChildContent { get; set; }
        [Parameter] public UpdateTrigger   Trigger      { get; set; }
        [Parameter] public UpdateTrigger[] Triggers     { get; set; }
        [Parameter] public bool            Protected    { get; set; }

        private bool _canRender;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, ChildContent);
        }

        protected override void OnInitialized()
        {
            if (Trigger != null)
                Trigger.OnUpdate += () =>
                {
                    _canRender = true;
                    InvokeAsync(StateHasChanged);
                    _canRender = false;
                };

            if (Triggers == null) return;

            foreach (UpdateTrigger trigger in Triggers)
                trigger.OnUpdate += () =>
                {
                    _canRender = true;
                    InvokeAsync(StateHasChanged);
                    _canRender = false;
                };
        }

        protected override bool ShouldRender()
        {
            return !Protected || _canRender;
        }
    }
}