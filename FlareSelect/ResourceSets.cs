using Superset.Web.Resources;

namespace FlareSelect
{
    public static class ResourceSets
    {
        public static readonly ResourceSet FlareSelect =
            new ResourceSet(
                nameof(FlareSelect), 
                nameof(FlareSelect),
                stylesheets: new[] {"FlareSelect.css"},
                dependencies: new[]
                {
                    Superset.Web.ResourceSets.FocusElement, 
                    Superset.Web.ResourceSets.Listeners,
                    // ShapeSet.ResourceSets.ShapeSet, 
                }
                );
    }
}