using Superset.Web.Resources;

namespace FlareSelect
{
    public static class ResourceManifests
    {
        public static readonly ResourceManifest FlareSelect =
            new ResourceManifest(nameof(FlareSelect), stylesheets: new[] {"FlareSelect.css"});
    }
}