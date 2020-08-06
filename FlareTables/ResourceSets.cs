using Superset.Web.Resources;

namespace FlareTables
{
    public static class ResourceSets
    {
        public static readonly ResourceSet FlareTables =
            new ResourceSet(
                nameof(FlareTables),
                nameof(FlareTables),
                stylesheets: new[] {"FlareTables.css"},
                dependencies: new[]
                {
                    Integrant.Resources.ResourceSets.MaterialIcons,
                    Integrant.Element.ResourceSets.Overrides.Buttons,
                    Integrant.Element.ResourceSets.Overrides.Inputs,
                }
            );
    }
}