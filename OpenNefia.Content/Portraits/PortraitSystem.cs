using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Portraits
{
    public interface IPortraitSystem : IEntitySystem
    {
        PrototypeId<PortraitPrototype> PickRandomPortraitID(EntityUid entity);
    }

    public sealed class PortraitSystem : EntitySystem, IPortraitSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<PortraitComponent, EntityBeingGeneratedEvent>(SetRandomPortrait);

            _protos.PrototypesReloaded += ev =>
            {
                if (ev.ByType.ContainsKey(typeof(PortraitPrototype)))
                    RegeneratePortraitCache();
            };

            RegeneratePortraitCache();
        }

        private readonly Dictionary<Gender, List<PrototypeId<PortraitPrototype>>> RandomPortraits = new();

        private void RegeneratePortraitCache()
        {
            RandomPortraits.Clear();

            foreach (var proto in _protos.EnumeratePrototypes<PortraitPrototype>().Where(p => p.RandomlyGenerate))
            {
                if (proto.Gender != null)
                    RandomPortraits.GetOrInsertNew(proto.Gender.Value).Add(proto.GetStrongID());
            }
        }

        private void SetRandomPortrait(EntityUid uid, PortraitComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (component.HasRandomPortrait)
            {
                component.PortraitID = PickRandomPortraitID(uid);
            }
        }

        public PrototypeId<PortraitPrototype> PickRandomPortraitID(EntityUid entity)
        {
            if (TryComp<CharaComponent>(entity, out var chara)
                && RandomPortraits.TryGetValue(chara.Gender, out var portraits)
                && portraits.Count > 0)
                return _rand.Pick(portraits);
            else
                return Protos.Portrait.Default;
        }
    }
}