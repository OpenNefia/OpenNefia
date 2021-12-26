using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Effects;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.GameObjects
{
    public class DrinkableSystem : EntitySystem
    {
        public const string VerbIDDrink = "Elona.Drink";

        [Dependency] private readonly IAudioSystem _sounds = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<DrinkableComponent, GetVerbsEventArgs>(HandleGetVerbs, nameof(HandleGetVerbs));
            SubscribeLocalEvent<ExecuteVerbEventArgs>(HandleExecuteVerb, nameof(HandleExecuteVerb));
            SubscribeLocalEvent<DrinkableComponent, DoDrinkEventArgs>(HandleDoDrink, nameof(HandleDoDrink));
            SubscribeLocalEvent<DrinkableComponent, ThrownEntityImpactedOtherEvent>(HandleImpactOther, nameof(HandleImpactOther));
            SubscribeLocalEvent<DrinkableComponent, ThrownEntityImpactedGroundEvent>(HandleImpactGround, nameof(HandleImpactGround));
            SubscribeLocalEvent<PotionPuddleComponent, EntitySteppedOnEvent>(HandlePotionPuddleSteppedOn, nameof(HandlePotionPuddleSteppedOn));
        }

        private void HandleGetVerbs(EntityUid potion, DrinkableComponent drinkableComp, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(VerbIDDrink));
        }

        private void HandleExecuteVerb(ExecuteVerbEventArgs args)
        {
            if (args.Handled)
                return;

            switch (args.Verb.ID)
            {
                case VerbIDDrink:
                    Raise(args.Target, new DoDrinkEventArgs(args.Source), args);
                    break;
            }
        }

        private void HandleDoDrink(EntityUid target, DrinkableComponent drinkable, DoDrinkEventArgs args)
        {
            args.Handle(Drink(target, args.Drinker, drinkable));
        }

        private TurnResult Drink(EntityUid target, EntityUid drinker,
            DrinkableComponent? drinkable = null)
        {
            if (!Resolve(target, ref drinkable))
                return TurnResult.Failed;

            if (!EntityManager.TryGetComponent(drinker, out SpatialComponent sourceSpatial))
                return TurnResult.Failed;

            if (!_stackSystem.TrySplit(target, 1, out var split))
                return TurnResult.Failed;

            Mes.Display($"{DisplayNameSystem.GetDisplayName(drinker)} drinks {DisplayNameSystem.GetDisplayName(split)}.");

            _sounds.Play(Protos.Sound.Drink1, sourceSpatial.MapPosition);

            var result = drinkable.Effect?.Apply(drinker, sourceSpatial.MapPosition, drinker, drinkable.Args)
                ?? EffectResult.Succeeded;

            EntityManager.DeleteEntity(split);

            return result.ToTurnResult();
        }

        private void HandleImpactOther(EntityUid thrown, DrinkableComponent potionComp, ThrownEntityImpactedOtherEvent args)
        {
            Mes.Display($"{DisplayNameSystem.GetDisplayName(thrown)} hits {DisplayNameSystem.GetDisplayName(args.ImpactedWith)}!");
            _sounds.Play(Protos.Sound.Crush2, args.Coords);

            potionComp.Effect?.Apply(args.Thrower, args.Coords, args.ImpactedWith, potionComp.Args);

            EntityManager.DeleteEntity(thrown);
        }

        private void HandleImpactGround(EntityUid thrown, DrinkableComponent potionComp, ThrownEntityImpactedGroundEvent args)
        {
            Mes.Display($"{DisplayNameSystem.GetDisplayName(thrown)} shatters.");
            _sounds.Play(Protos.Sound.Crush2, args.Coords);

            var puddle = EntityManager.SpawnEntity(Protos.Mef.Potion, args.Coords);

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
            _sounds.Play(Protos.Sound.Water, args.Coords);

            potionComp.Effect?.Apply(source, args.Coords, args.Stepper, potionComp.Args);

            EntityManager.DeleteEntity(source);
        }
    }

    public class DoDrinkEventArgs : TurnResultEntityEventArgs
    {
        public readonly EntityUid Drinker;

        public DoDrinkEventArgs(EntityUid drinker)
        {
            Drinker = drinker;
        }
    }
}
