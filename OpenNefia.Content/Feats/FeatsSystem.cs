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
        int Level(FeatsComponent feats, PrototypeId<FeatPrototype> id);
        bool TryGetKnown(FeatsComponent feats, PrototypeId<FeatPrototype> protoId, [NotNullWhen(true)] out FeatLevel? stat);
        void AddLevel(EntityUid ent, PrototypeId<FeatPrototype> protoId, int delta);
    }

    public sealed class FeatsSystem : EntitySystem, IFeatsSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public int Level(FeatsComponent feats, PrototypeId<FeatPrototype> id)
        {
            if (TryGetKnown(feats, id, out var feat))
                return feat.Level.Buffed;

            return 0;
        }

        public bool TryGetKnown(FeatsComponent feats, PrototypeId<FeatPrototype> protoId, [NotNullWhen(true)] out FeatLevel? level)
        {
            return feats.Feats.TryGetValue(protoId, out level);
        }

        public void AddLevel(EntityUid ent, PrototypeId<FeatPrototype> protoId, int delta)
        {
            if (delta == 0)
                return;

            if (!EntityManager.TryGetComponent(ent, out FeatsComponent feats))
                return;

            var featProto = _protos.Index(protoId);

            var level = feats.Feats.GetValueOrInsert(protoId, () => new FeatLevel(0));

            level.Level.Base = Math.Clamp(level.Level.Base + delta, featProto.LevelMin, featProto.LevelMax);
        }
    }
}
