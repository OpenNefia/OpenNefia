using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Feats
{
    /// <summary>
    /// Holds the current Feats of an entity as well as the current amount of learnable Feats.
    /// </summary>
    [RegisterComponent]
    public class FeatsComponent : Component, IComponentRefreshable
    {        
        /// <summary>
        /// Current amount of learnable Feats.
        /// </summary>
        [DataField]
        public int NumberOfFeatsAcquirable { get; set; } = 0;

        /// <summary>
        /// Current Feats and the respective Levels
        /// </summary>
        [DataField]
        public Dictionary<PrototypeId<FeatPrototype>, FeatLevel> Feats { get; } = new();

        public FeatLevel Ensure(PrototypeId<FeatPrototype> protoId)
        {
            if (Feats.TryGetValue(protoId, out var level))
                return level;

            return new FeatLevel()
            {
                Level = new(0)
            };
        }

        public bool TryGetKnown(FeatPrototype proto, [NotNullWhen(true)] out FeatLevel? level)
            => TryGetKnown(proto.GetStrongID(), out level);
        public bool TryGetKnown(PrototypeId<FeatPrototype> protoId, [NotNullWhen(true)] out FeatLevel? level)
        {
            if (!Feats.TryGetValue(protoId, out level))
            {
                return false;
            }

            if (level.Level.Base <= 0)
            {
                return false;
            }

            return true;
        }

        public bool HasFeat(FeatPrototype proto)
            => HasFeat(proto.GetStrongID());
        public bool HasFeat(PrototypeId<FeatPrototype> id)
        {
            return TryGetKnown(id, out _);
        }

        public int Level(FeatPrototype proto) => Level(proto.GetStrongID());
        public int Level(PrototypeId<FeatPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Level.Buffed;
        }

        public int BaseLevel(FeatPrototype proto) => BaseLevel(proto.GetStrongID());
        public int BaseLevel(PrototypeId<FeatPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Level.Base;
        }

        public void Refresh()
        {
            foreach (var level in Feats.Values)
            {
                level.Level.Reset();
            }
        }
    }
}
