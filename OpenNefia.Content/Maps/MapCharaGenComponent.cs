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
        public IMapCharaFilter? CharaFilterGen { get; set; }
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IMapCharaFilter
    {
        CharaFilter GenerateFilter(IMap map);
    }

    public class DefaultCharaFilter : IMapCharaFilter
    {
        public CharaFilter GenerateFilter(IMap map)
        {
            return new();
        }
    }
}
