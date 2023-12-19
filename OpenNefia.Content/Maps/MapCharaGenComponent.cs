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

    /// <summary>
    /// Original max chara counts from 1.22.
    /// TODO: Should robably replace these at some point.
    /// </summary>
    public static class MapCharaGenConsts
    {
        public const int MaxAllyCount = 16;
        public const int MaxAdventurerCount = 40;
        public const int MaxSavedCharaCount = MaxAllyCount + MaxAdventurerCount;
        public const int MaxTotalCharaCount = 245;
        public const int MaxOtherCharaCount = MaxTotalCharaCount - MaxSavedCharaCount;
    }
}
