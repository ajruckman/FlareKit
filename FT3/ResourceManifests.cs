using Superset.Web.Resources;

namespace FT3
{
    public static class ResourceManifests
    {
        public static readonly ResourceManifest FlareTables = new ResourceManifest(
            nameof(FT3),
            stylesheets: new[] {"css/Build/Style.{{ThemeVariant}}.css"}
        );
    }
}