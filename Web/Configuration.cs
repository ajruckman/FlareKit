using System.Collections.Generic;
using Superset.Web.Resources;

namespace Web
{
    public sealed class Configuration
    {
        public List<ResourceManifest> ResourceManifests = new List<ResourceManifest>
        {
            Superset.Web.Utilities.Utilities.ResourceManifest
        };
    }
}