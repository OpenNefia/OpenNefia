using OpenNefia.Content.TurnOrder;
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
using static CSharpRepl.Services.Completion.OpenAI.ChatCompletionApi.ChatCompletionResponse;

namespace OpenNefia.Content.Feats
{
    public interface IFeatsSystem : IEntitySystem
    {
        bool TryGetKnown(EntityUid entity, PrototypeId<FeatPrototype> protoId, [NotNullWhen(true)] out FeatLevel? stat, FeatsComponent? feats = null);
        bool TryGetKnown(EntityUid entity, FeatPrototype protoId, [NotNullWhen(true)] out FeatLevel? stat, FeatsComponent? feats = null);
        bool HasFeat(EntityUid entity, PrototypeId<FeatPrototype> id, FeatsComponent? feats = null);
        bool HasFeat(EntityUid entity, FeatPrototype id, FeatsComponent? feats = null);
        int Level(EntityUid entity, PrototypeId<FeatPrototype> id, FeatsComponent? feats = null);
        int Level(EntityUid entity, FeatPrototype id, FeatsComponent? feats = null);
        void ModifyLevel(EntityUid ent, PrototypeId<FeatPrototype> id, int delta, FeatsComponent? feats = null);
        void ModifyLevel(EntityUid ent, FeatPrototype protoId, int delta, FeatsComponent? feats = null);
        void SetLevel(EntityUid ent, PrototypeId<FeatPrototype> id, int level, FeatsComponent? feats = null);
        void SetLevel(EntityUid ent, FeatPrototype protoId, int level, FeatsComponent? feats = null);
    }

    public sealed class FeatsSystem : EntitySystem, IFeatsSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<FeatsComponent, MapBeforeTurnBeginEventArgs>(ProcFeatTurnBeginEvents);
        }

        private void ProcFeatTurnBeginEvents(EntityUid uid, FeatsComponent component, MapBeforeTurnBeginEventArgs args)
        {
            if (args.Handled)
                return;

            var ev = new P_FeatBeforeTurnBeginEvent(uid);

            foreach (var proto in _protos.EnumeratePrototypes<FeatPrototype>())
            {
                var id = proto.GetStrongID();
                if (TryGetKnown(uid, id, out var feat, component))
                {
                    _protos.EventBus.RaiseEvent(id, ev);
                }
            }
        }

        public bool TryGetKnown(EntityUid entity, PrototypeId<FeatPrototype> protoId, [NotNullWhen(true)] out FeatLevel? level, FeatsComponent? feats = null)
        {
            if (!Resolve(entity, ref feats))
            {
                level = null;
                return false;
            }

            return feats.Feats.TryGetValue(protoId, out level);
        }

        public bool TryGetKnown(EntityUid entity, FeatPrototype proto, [NotNullWhen(true)] out FeatLevel? level, FeatsComponent? feats = null)
            => TryGetKnown(entity, proto.GetStrongID(), out level, feats);

        public int Level(EntityUid entity, PrototypeId<FeatPrototype> protoId, FeatsComponent? feats = null)
        {
            if (!TryGetKnown(entity, protoId, out var feat))
                return 0;

            return feat.Level.Buffed;
        }

        public int Level(EntityUid entity, FeatPrototype proto, FeatsComponent? feats = null)
            => Level(entity, proto.GetStrongID(), feats);

        public bool HasFeat(EntityUid entity, PrototypeId<FeatPrototype> protoId, FeatsComponent? feats = null)
        {
            if (!TryGetKnown(entity, protoId, out var feat))
                return false;

            return feat.Level != 0;
        }

        public bool HasFeat(EntityUid entity, FeatPrototype proto, FeatsComponent? feats = null)
            => HasFeat(entity, proto.GetStrongID(), feats);

        public void ModifyLevel(EntityUid entity, PrototypeId<FeatPrototype> protoId, int delta, FeatsComponent? feats = null)
        {
            if (delta == 0)
                return;

            if (!Resolve(entity, ref feats))
                return;

            var featProto = _protos.Index(protoId);
            var level = feats.Feats.GetValueOrInsert(protoId, () => new FeatLevel(0));
            level.Level.Base = Math.Clamp(level.Level.Base + delta, featProto.LevelMin, featProto.LevelMax);

            if (level.Level.Base == 0)
                feats.Feats.Remove(protoId);
        }

        public void ModifyLevel(EntityUid entity, FeatPrototype proto, int delta, FeatsComponent? feats = null)
            => ModifyLevel(entity, proto.GetStrongID(), delta, feats);

        public void SetLevel(EntityUid ent, PrototypeId<FeatPrototype> protoId, int level, FeatsComponent? feats = null)
        {
            if (!Resolve(ent, ref feats))
                return;

            var featProto = _protos.Index(protoId);

            if (level == 0)
            {
                feats.Feats.Remove(protoId);
            }
            else
            {
                var featLevel = feats.Feats.GetValueOrInsert(protoId, () => new FeatLevel(level));
                featLevel.Level.Base = level;
            }
        }

        public void SetLevel(EntityUid ent, FeatPrototype protoId, int level, FeatsComponent? feats = null)
            => SetLevel(ent, protoId.GetStrongID(), level, feats);
    }

    [PrototypeEvent(typeof(FeatPrototype))]
    public sealed class P_FeatBeforeTurnBeginEvent : PrototypeEventArgs
    {
        public EntityUid Entity { get; }

        public P_FeatBeforeTurnBeginEvent(EntityUid entity)
        {
            Entity = entity;
        }
    }
}
