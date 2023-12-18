using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Effects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Locale;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Identify;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Game;
using System.Security.Principal;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Parties;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Potion
{
    public sealed class PotionSystem : EntitySystem
    {
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEffectSystem _effects = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IHungerSystem _hunger = default!;

        public override void Initialize()
        {
            SubscribeComponent<PotionComponent, GetVerbsEventArgs>(HandleGetVerbs);
            SubscribeComponent<PotionComponent, ThrownEntityImpactedOtherEvent>(HandleImpactOther);
            SubscribeComponent<PotionComponent, ThrownEntityImpactedGroundEvent>(HandleImpactGround);
            SubscribeComponent<PotionPuddleComponent, EntitySteppedOnEvent>(HandlePotionPuddleSteppedOn);
        }

        private void HandleGetVerbs(EntityUid potion, PotionComponent drinkableComp, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(DrinkInventoryBehavior.VerbTypeDrink, "Drink Potion", () => Drink(args.Source, args.Target)));
        }

        private TurnResult Drink(EntityUid drinker, EntityUid potion,
            PotionComponent? potionComp = null)
        {
            if (!Resolve(potion, ref potionComp))
                return TurnResult.Failed;

            if (!EntityManager.TryGetComponent(drinker, out SpatialComponent sourceSpatial))
                return TurnResult.Failed;

            _mes.Display(Loc.GetString("Elona.Potion.Drinks", ("entity", drinker), ("item", potion)));
            _sounds.Play(Protos.Sound.Drink1, drinker);

            var effectArgs = EffectArgSet.FromImmutable(potionComp.EffectArgs);
            var result = _effects.Apply(potionComp.Effect, drinker, drinker, sourceSpatial.Coordinates, potion, effectArgs);

            _stackSystem.Use(potion, 1);
            ApplyPotionHungerEffects(drinker);

            if (_gameSession.IsPlayer(drinker))
            {
                if ((!effectArgs.TryGet<EffectCommonArgs>(out var commonArgs) || commonArgs.OutEffectWasObvious) && IsAlive(potion))
                {
                    _identify.Identify(potion, IdentifyState.Name);
                }
            }

            return result;
        }

        private void ApplyPotionHungerEffects(EntityUid drinker)
        {
            if (TryComp<HungerComponent>(drinker, out var hunger))
            {
                hunger.Nutrition += 150;
                
                if (_parties.IsInPlayerParty(drinker) && hunger.Nutrition > HungerLevels.Bloated && _rand.OneIn(5))
                    _hunger.Vomit(drinker, hunger);
            }

        }

        private void HandleImpactOther(EntityUid thrown, PotionComponent potionComp, ThrownEntityImpactedOtherEvent args)
        {
            if (args.Handled)
                return;
            
            args.Handled = true;

            _mes.Display(Loc.GetString("Elona.Throw.Hits", ("entity", args.ImpactedWith)));
            _sounds.Play(Protos.Sound.Crush2, args.ImpactedWith);
            _sounds.Play(Protos.Sound.Drink1, args.ImpactedWith);

            var effectArgs = EffectArgSet.FromImmutable(potionComp.EffectArgs);
            _effects.Apply(potionComp.Effect, args.Thrower, args.ImpactedWith, args.Coords, thrown, effectArgs);

            _stackSystem.Use(thrown, 1);
            ApplyPotionHungerEffects(args.ImpactedWith);
        }

        private void HandleImpactGround(EntityUid thrown, PotionComponent potionComp, ThrownEntityImpactedGroundEvent args)
        {
            if (args.Handled)
                return;
            
            args.Handled = true;

            _mes.Display(Loc.GetString("Elona.Potion.Thrown.Shatters"));
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
                puddleComp.EffectArgs = potionComp.EffectArgs;
            }
        
            EntityManager.DeleteEntity(thrown);
        }

        private void HandlePotionPuddleSteppedOn(EntityUid source, PotionPuddleComponent potionComp, EntitySteppedOnEvent args)
        {
            _sounds.Play(Protos.Sound.Water, source);
            _sounds.Play(Protos.Sound.Drink1, args.Stepper);

            var effectArgs = EffectArgSet.FromImmutable(potionComp.EffectArgs);
            _effects.Apply(potionComp.Effect, source, args.Stepper, args.Coords, potionComp.Owner, effectArgs);

            EntityManager.DeleteEntity(source);
        }
    }
}
