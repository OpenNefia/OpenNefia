using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;

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
            SubscribeComponent<PortraitComponent, EntityBeingGeneratedEvent>(SetRandomPortrait, priority: EventPriorities.Highest);

            _protos.PrototypesReloaded += ev =>
            {
                if (ev.ByType.ContainsKey(typeof(PortraitPrototype)))
                    RegeneratePortraitCache();
            };

            RegeneratePortraitCache();
        }

        private readonly List<PrototypeId<PortraitPrototype>> MalePrototypes = new();
        private readonly List<PrototypeId<PortraitPrototype>> FemalePrototypes = new();

        private void RegeneratePortraitCache()
        {
            MalePrototypes.Clear();
            FemalePrototypes.Clear();

            foreach (var proto in _protos.EnumeratePrototypes<PortraitPrototype>())
            {
                if (proto.Gender == Gender.Male)
                    MalePrototypes.Add(proto.GetStrongID());
                else if (proto.Gender == Gender.Female)
                    FemalePrototypes.Add(proto.GetStrongID());
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
            if (TryComp<CharaComponent>(entity, out var chara) && chara.Gender == Gender.Male)
                return _rand.Pick(MalePrototypes);
            else
                return _rand.Pick(FemalePrototypes);
        }
    }
}