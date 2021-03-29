using System.Collections.Generic;
using Superset.Web.Resources;

namespace Web
{
    public sealed class Configuration
    {
        public readonly ResourceSet ResourceSet = new ResourceSet(
            nameof(Web),
            nameof(ResourceSet),
            dependencies: new[]
            {
                Superset.Web.ResourceSets.SaveAsFile,
                // FontSet.ResourceSets.Inter,
                // FontSet.ResourceSets.JetBrainsMono,
                FlareTables.ResourceSets.FlareTables,
                FlareSelect.ResourceSets.FlareSelect,
                // ColorSet.ResourceSets.Globals,
                
                // Integrant.Resources.ResourceSets.MaterialIcons,
                Integrant.Resources.ResourceSets.Fonts.SansSerif.Inter,
                
                // Integrant.Element.ResourceSets.Bits,
                // Integrant.Element.ResourceSets.Overrides.Buttons,
                // Integrant.Element.ResourceSets.Overrides.Inputs,
                Integrant.Element.ResourceSets.Overrides.VariantLoader,
            }
        );
    }
}