using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Maps;
using OpenNefia.Content.RandomGen;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapCharaGenComponent : Component
    {
        public override string Name => "MapCharaGen";

        /// <summary>
        /// The maximum number of entities with a <see cref="CharaComponent"/> that this map should
        /// contain. If this limit is reached, no new characters will be randomly generated.
        /// </summary>
        [DataField]
        public int MaxCharaCount { get; set; } = 100;

        /// <summary>
        /// Current number of characters in this map.
        /// </summary>
        [DataField]
        public int CurrentCharaCount { get; set; } = 0;

        /// <summary>
        /// Random character generation behavior.
        /// </summary>
        [DataField]
        public IMapCharaFilterGen? CharaFilterGen { get; set; }
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IMapCharaFilterGen
    {
        CharaFilter GenerateFilter(IMap map);
    }
}
