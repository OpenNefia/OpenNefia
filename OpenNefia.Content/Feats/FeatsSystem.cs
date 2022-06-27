using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Stats;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Feats
{
    public interface IFeatsSystem : IEntitySystem
    {
        bool TryGetKnown(EntityUid entity, PrototypeId<FeatPrototype> protoId, [NotNullWhen(true)] out FeatLevel? stat, FeatsComponent? feats = null);
        bool HasFeat(EntityUid entity, PrototypeId<FeatPrototype> id, FeatsComponent? feats = null);
        int Level(EntityUid entity, PrototypeId<FeatPrototype> id, FeatsComponent? feats = null);
        void AddLevel(EntityUid ent, PrototypeId<FeatPrototype> protoId, int delta, FeatsComponent? feats = null);
    }

    public sealed class FeatsSystem : EntitySystem, IFeatsSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public bool TryGetKnown(EntityUid entity, PrototypeId<FeatPrototype> protoId, [NotNullWhen(true)] out FeatLevel? level, FeatsComponent? feats = null)
        {
            if (!Resolve(entity, ref feats))
            {
                level = null;
                return false;
            }

            return feats.Feats.TryGetValue(protoId, out level);
        }

        public int Level(EntityUid entity, PrototypeId<FeatPrototype> protoId, FeatsComponent? feats = null)
        {
            if (!TryGetKnown(entity, protoId, out var feat))
                return 0;

            return feat.Level.Buffed;
        }
        
        public bool HasFeat(EntityUid entity, PrototypeId<FeatPrototype> protoId, FeatsComponent? feats = null)
        {
            if (!TryGetKnown(entity, protoId, out var feat))
                return false;

            return feat.Level != 0;
        }

        public void AddLevel(EntityUid entity, PrototypeId<FeatPrototype> protoId, int delta, FeatsComponent? feats = null)
        {
            if (delta == 0)
                return;
            
            if (!Resolve(entity, ref feats))
                return;

            var featProto = _protos.Index(protoId);
            var level = feats.Feats.GetValueOrInsert(protoId, () => new FeatLevel(0));
            level.Level.Base = Math.Clamp(level.Level.Base + delta, featProto.LevelMin, featProto.LevelMax);
        }
    }
}
