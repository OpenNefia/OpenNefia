using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.GameObjects
{
    public class PotionSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<PotionComponent, ThrownEntityImpactedOtherEvent>(HandlePotionImpactOther);
            SubscribeLocalEvent<PotionComponent, ThrownEntityImpactedGroundEvent>(HandlePotionImpactGround);
            SubscribeLocalEvent<PotionPuddleComponent, EntitySteppedOnEvent>(HandlePotionPuddleSteppedOn);
        }

        private void HandlePotionImpactOther(EntityUid thrown, PotionComponent potionComp, ThrownEntityImpactedOtherEvent args)
        {
            Mes.Display($"{DisplayNameSystem.GetDisplayName(thrown)} hits {DisplayNameSystem.GetDisplayName(args.ImpactedWith)}!");
            Sounds.Play(SoundPrototypeOf.Crush2, args.Coords);

            potionComp.Effect?.Apply(args.Thrower, args.Coords, args.ImpactedWith, potionComp.Args);

            EntityManager.DeleteEntity(thrown);
        }

        private void HandlePotionImpactGround(EntityUid thrown, PotionComponent potionComp, ThrownEntityImpactedGroundEvent args)
        {
            Mes.Display($"{DisplayNameSystem.GetDisplayName(thrown)} shatters.");
            Sounds.Play(SoundPrototypeOf.Crush2, args.Coords);

            var puddle = EntityManager.SpawnEntity(new("PotionPuddle"), args.Coords);

            if (EntityManager.TryGetComponent(puddle.Uid, out ChipComponent chipCompPuddle)
                && EntityManager.TryGetComponent(thrown, out ChipComponent chipCompPotion))
            {
                chipCompPuddle.Color = chipCompPotion.Color;
            }
            if (EntityManager.TryGetComponent(puddle.Uid, out PotionPuddleComponent puddleComp))
            {
                puddleComp.Effect = potionComp.Effect;
                puddleComp.Args = potionComp.Args;
            }
        
            EntityManager.DeleteEntity(thrown);
        }

        private void HandlePotionPuddleSteppedOn(EntityUid source, PotionPuddleComponent potionComp, EntitySteppedOnEvent args)
        {
            Sounds.Play(SoundPrototypeOf.Water, args.Coords);

            potionComp.Effect?.Apply(source, args.Coords, args.Stepper, potionComp.Args);

            EntityManager.DeleteEntity(source);
        }
    }
}
