using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace FlareSelect
{
    public static class FocusScript
    {
        private static void Script(RenderTreeBuilder builder)
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

        public static RenderFragment Fragment => Script;
    }
}