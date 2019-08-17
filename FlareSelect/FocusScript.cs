using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace FlareSelect
{
    public static class FocusScript
    {
        public static RenderFragment Content
        {
            get
            {
                void Fragment(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "script");
                    builder.AddMarkupContent(1,
                                             @"
window.FlareSelect = {
  focusElement : function (id) {
    setTimeout(function() {
      document.getElementById(id).focus();
    }, 10);
  }
}");
                    builder.CloseElement();
                }

                return Fragment;
            }
        }
    }
}