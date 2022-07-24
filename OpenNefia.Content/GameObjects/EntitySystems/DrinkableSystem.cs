using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Effects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Locale;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Effects;

namespace OpenNefia.Content.GameObjects
{
    public class DrinkableSystem : EntitySystem
    {
        public const string VerbIDDrink = "Elona.Drink";

        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<DrinkableComponent, GetVerbsEventArgs>(HandleGetVerbs);
            SubscribeComponent<DrinkableComponent, ThrownEntityImpactedOtherEvent>(HandleImpactOther);
            SubscribeComponent<DrinkableComponent, ThrownEntityImpactedGroundEvent>(HandleImpactGround);
            SubscribeComponent<PotionPuddleComponent, EntitySteppedOnEvent>(HandlePotionPuddleSteppedOn);
        }

        private void HandleGetVerbs(EntityUid potion, DrinkableComponent drinkableComp, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(VerbIDDrink, "Drink Item", () => Drink(args.Source, args.Target)));
        }

        private TurnResult Drink(EntityUid drinker, EntityUid target,
            DrinkableComponent? drinkable = null)
        {
            if (!Resolve(target, ref drinkable))
                return TurnResult.Failed;

            if (!EntityManager.TryGetComponent(drinker, out SpatialComponent sourceSpatial))
                return TurnResult.Failed;

            if (!_stackSystem.TrySplit(target, 1, out var split))
                return TurnResult.Failed;

            _mes.Display(Loc.GetString("Elona.Drinkable.Drinks", ("entity", drinker), ("item", split)));

            _sounds.Play(Protos.Sound.Drink1, sourceSpatial.MapPosition);

            var result = drinkable.Effect?.Apply(drinker, sourceSpatial.MapPosition, drinker, drinkable.Args)
                ?? EffectResult.Succeeded;

            EntityManager.DeleteEntity(split);

            return result.ToTurnResult();
        }

        private void HandleImpactOther(EntityUid thrown, DrinkableComponent potionComp, ThrownEntityImpactedOtherEvent args)
        {
            _mes.Display(Loc.GetString("Elona.Throwable.Hits", ("entity", args.ImpactedWith)));
            _sounds.Play(Protos.Sound.Crush2, args.Coords);

            potionComp.Effect?.Apply(args.Thrower, args.Coords, args.ImpactedWith, potionComp.Args);

            EntityManager.DeleteEntity(thrown);
        }

        private void HandleImpactGround(EntityUid thrown, DrinkableComponent potionComp, ThrownEntityImpactedGroundEvent args)
        {
            _mes.Display(Loc.GetString("Elona.Drinkable.Thrown.Shatters"));
            _sounds.Play(Protos.Sound.Crush2, args.Coords);

            var puddle = _entityGen.SpawnEntity(Protos.Mef.Potion, args.Coords);

            if (puddle == null)
                return;

            if (EntityManager.TryGetComponent(puddle.Value, out ChipComponent chipCompPuddle)
                && EntityManager.TryGetComponent(thrown, out ChipComponent chipCompPotion))
            {
                chipCompPuddle.Color = chipCompPotion.Color;
            }
            if (EntityManager.TryGetComponent(puddle.Value, out PotionPuddleComponent puddleComp))
            {
                puddleComp.Effect = potionComp.Effect;
                puddleComp.Args = potionComp.Args;
            }
        
            EntityManager.DeleteEntity(thrown);
        }

        private void HandlePotionPuddleSteppedOn(EntityUid source, PotionPuddleComponent potionComp, EntitySteppedOnEvent args)
        {
            _sounds.Play(Protos.Sound.Water, args.Coords);

            potionComp.Effect?.Apply(source, args.Coords, args.Stepper, potionComp.Args);

            EntityManager.DeleteEntity(source);
        }
    }
}
