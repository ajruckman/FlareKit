using System.Collections.Generic;
using Superset.Web.Resources;

namespace Web
{
    public sealed class Configuration
    {
        public readonly List<ResourceManifest> ResourceManifests = new List<ResourceManifest>
        {
            Superset.Web.Utilities.Utilities.SaveAsFileManifest,
            FontSet.ResourceManifests.Inter,
            FontSet.ResourceManifests.JetBrainsMono,
            FT3.ResourceManifests.FlareTables
        };
    }
}