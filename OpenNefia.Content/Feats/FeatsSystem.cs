using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Stats;
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
        Stat<int> Ensure(FeatsComponent feats, PrototypeId<FeatPrototype> id);
        bool TryGetKnown(FeatsComponent feats, PrototypeId<FeatPrototype> protoId, [NotNullWhen(true)] out Stat<int>? stat);
    }
    public sealed partial class FeatsSystem : EntitySystem, IFeatsSystem
    {
        public Stat<int> Ensure(FeatsComponent feats, PrototypeId<FeatPrototype> id)
        {
            if (TryGetKnown(feats, id, out var stat))
                return stat;
            return new(0);
        }

        public int Level(FeatsComponent feats, PrototypeId<FeatPrototype> id)
        {
            if (TryGetKnown(feats, id, out var stat))
                return stat.Buffed;
            return 0;
        }

        public bool TryGetKnown(FeatsComponent feats, PrototypeId<FeatPrototype> protoId, [NotNullWhen(true)] out Stat<int>? stat)
        {
            if (feats.Feats.TryGetValue(protoId, out stat))
                return true;
            return false;
        }
    }
}
