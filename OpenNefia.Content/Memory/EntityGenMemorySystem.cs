using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Memory
{
    public interface IEntityGenMemorySystem : IEntitySystem
    {
        void Memorize(PrototypeId<EntityPrototype> id, string kind);
        void Forget(PrototypeId<EntityPrototype> id, string kind);
        void Set(PrototypeId<EntityPrototype> id, string kind, int amount);
        int Get(PrototypeId<EntityPrototype> id, string kind);
    }

    public static class EntityMemoryKinds
    {
        /// <summary>
        /// How many of this entity have been generated.
        /// </summary>
        public const string Generated = nameof(Generated);
        
        /// <summary>
        /// How many of this entity have been killed.
        /// </summary>
        public const string Killed = nameof(Killed);

        /// <summary>
        /// If this item has been identified. > 0 means "true".
        /// </summary>
        public const string Identified = nameof(Identified);
    }

    public sealed class EntityGenMemorySystem : EntitySystem, IEntityGenMemorySystem
    {
        [RegisterSaveData($"Elona.EntityMemorySystem.Memory")]
        private Dictionary<string, Dictionary<PrototypeId<EntityPrototype>, int>> Memory { get; } = new();

        public void Memorize(PrototypeId<EntityPrototype> id, string kind)
        {
            var byId = Memory.GetValueOrInsert(kind);
            byId[id]++;
        }

        public void Forget(PrototypeId<EntityPrototype> id, string kind)
        {
            var byId = Memory.GetValueOrInsert(kind);
            byId[id] = Math.Max(byId[id] - 1, 0);
        }

        public void Set(PrototypeId<EntityPrototype> id, string kind, int amount)
        {
            var byId = Memory.GetValueOrInsert(kind);
            byId[id] = Math.Max(amount, 0);
        }

        public int Get(PrototypeId<EntityPrototype> id, string kind)
        {
            var byId = Memory.GetValueOrInsert(kind);
            return byId.GetValueOr(id, 0);
        }
    }

    public static class IEntityGenMemorySystemExtensions
    {
        public static int Generated(this IEntityGenMemorySystem mem, PrototypeId<EntityPrototype> id)
            => mem.Get(id, EntityMemoryKinds.Generated);

        public static int Killed(this IEntityGenMemorySystem mem, PrototypeId<EntityPrototype> id)
            => mem.Get(id, EntityMemoryKinds.Killed);

        public static bool IsIdentified(this IEntityGenMemorySystem mem, PrototypeId<EntityPrototype> id) 
            => mem.Get(id, EntityMemoryKinds.Identified) > 0;

        public static void SetIdentified(this IEntityGenMemorySystem mem, PrototypeId<EntityPrototype> id, bool identified)
            => mem.Set(id, EntityMemoryKinds.Identified, identified ? 1 : 0);
    }
}
