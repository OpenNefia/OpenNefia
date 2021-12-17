using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Logic;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.GameObjects
{
    public class DrinkableSystem : EntitySystem
    {
        public const string VerbIDDrink = "Elona.Drink";

        public override void Initialize()
        {
            SubscribeLocalEvent<DrinkableComponent, GetVerbsEventArgs>(HandleGetVerbs, nameof(HandleGetVerbs));
            SubscribeLocalEvent<ExecuteVerbEventArgs>(HandleExecuteVerb, nameof(HandleExecuteVerb));
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
            switch (args.Verb.ID)
            {
                case VerbIDDrink:
                    ExecuteVerbDrink(args.Target, args);
                    break;
            }
        }

        private void ExecuteVerbDrink(EntityUid potion, ExecuteVerbEventArgs args, 
            DrinkableComponent? drinkableComp = null)
        {
            Mes.Display($"{DisplayNameSystem.GetDisplayName(args.Source)} drinks {DisplayNameSystem.GetDisplayName(potion)}.");

            if (!Resolve(potion, ref drinkableComp))
                return;

            if (!EntityManager.TryGetComponent(args.Source, out SpatialComponent sourceSpatial))
                return;

            Sounds.Play(Protos.Sound.Drink1, sourceSpatial.MapPosition);

            drinkableComp.Effect?.Apply(args.Source, sourceSpatial.MapPosition, args.Source, drinkableComp.Args);

            // TODO stacking
            EntityManager.DeleteEntity(potion);
        }

        private void HandleImpactOther(EntityUid thrown, DrinkableComponent potionComp, ThrownEntityImpactedOtherEvent args)
        {
            Mes.Display($"{DisplayNameSystem.GetDisplayName(thrown)} hits {DisplayNameSystem.GetDisplayName(args.ImpactedWith)}!");
            Sounds.Play(Protos.Sound.Crush2, args.Coords);

            potionComp.Effect?.Apply(args.Thrower, args.Coords, args.ImpactedWith, potionComp.Args);

            EntityManager.DeleteEntity(thrown);
        }

        private void HandleImpactGround(EntityUid thrown, DrinkableComponent potionComp, ThrownEntityImpactedGroundEvent args)
        {
            Mes.Display($"{DisplayNameSystem.GetDisplayName(thrown)} shatters.");
            Sounds.Play(Protos.Sound.Crush2, args.Coords);

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
            Sounds.Play(Protos.Sound.Water, args.Coords);

            potionComp.Effect?.Apply(source, args.Coords, args.Stepper, potionComp.Args);

            EntityManager.DeleteEntity(source);
        }
    }
}
