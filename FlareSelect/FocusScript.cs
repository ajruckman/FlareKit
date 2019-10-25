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
    }
}