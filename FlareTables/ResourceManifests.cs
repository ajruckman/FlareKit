using Superset.Web.Resources;

namespace FlareTables
{
    public static class ResourceManifests
    {
        public static readonly ResourceManifest FlareTables =
            new ResourceManifest(nameof(FlareTables), stylesheets: new[] {"FlareTables.css"});
    }
}