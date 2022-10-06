using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.ContentPack
{
    public sealed class ModManifest
    {
        public ModManifest(ContentRootID contentRootID, string id, Version version, List<ModDependency> dependencies)
        {
            ContentRootID = contentRootID;
            ID = id;
            Version = version;
            Dependencies = dependencies;
        }

        public ContentRootID ContentRootID { get; }
        public string ID { get; }
        public Version Version { get; }
        public IReadOnlyList<ModDependency> Dependencies { get; private set; }
    }

    public sealed class ModDependency
    {
        public ModDependency(string id)
        {
            ID = id;
        }

        public string ID { get; }
    }
}
