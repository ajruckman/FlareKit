using System.Text;

namespace FlareSelect
{
    public static class FocusScript
    {
        public static string Script
        {
            get
            {
                StringBuilder res = new StringBuilder();

                res.AppendLine("<script>");
                res.AppendLine("    window.FlareSelect = {");
                res.AppendLine("        focusElement : function (id) {");
                res.AppendLine("            setTimeout(function() {");
                res.AppendLine("                document.getElementById(id).focus();");
                res.AppendLine("            }, 10);");
                res.AppendLine("        }");
                res.AppendLine("    }");
                res.AppendLine("</script>");

                return res.ToString();
            }
        }

        // https://stackoverflow.com/a/3028037/9911189
        public static string ClickScript =>
            @"
window.provision = function(dotnetHelper, id) {
    console.log('provision ' + id);
    
    window.hideOnClickOutside(dotnetHelper, document.getElementById(id))
}

window.hideOnClickOutside = function(dotnetHelper, element) {
    console.log(element)
    
    const outsideClickListener = event => {
        if (!element.contains(event.target) && window.isVisible(element)) { // or use: event.target.closest(selector) === null
            dotnetHelper.invokeMethodAsync('OuterClick');
            //element.style.display = 'none'
            //removeClickListener()
        }
    }

    const removeClickListener = () => {
        document.removeEventListener('click', outsideClickListener)
    }

    document.addEventListener('click', outsideClickListener)
}

window.isVisible = elem => !!elem && !!( elem.offsetWidth || elem.offsetHeight || elem.getClientRects().length ) // source (2018-03-11): https://github.com/jquery/jquery/blob/master/src/css/hiddenVisibleSelectors.js";
    }
}