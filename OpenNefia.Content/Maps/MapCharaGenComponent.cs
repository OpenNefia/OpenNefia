using OpenNefia.Core.GameObjects;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
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
        /// Random character generation behavior.
        /// </summary>
        [DataField]
        public IMapCharaFilter? CharaFilter { get; set; }
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IMapCharaFilter
    {
        // TODO
    }

    public class CharaFilterPalmia : IMapCharaFilter
    {
    }
}
