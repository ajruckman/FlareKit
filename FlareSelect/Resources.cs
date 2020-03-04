using Superset.Web.Resources;

namespace FlareSelect
{
    public static class Resources
    {
        public static ResourceManifest FocusScript = new ResourceManifest(
            nameof(FlareSelect),
            scripts: new[] {"FocusScript.js"}
        );
    }
}