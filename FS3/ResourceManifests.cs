using Superset.Web.Resources;

namespace FS3
{
    public static class ResourceManifests
    {
        public static readonly ResourceManifest FlareSelect =
            new ResourceManifest(nameof(FS3), stylesheets: new[] {"FlareSelect.css"});
    }
}