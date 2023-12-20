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
using Microsoft.FileFormats;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Effects.New;
using OpenNefia.Content.Visibility;

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
        [Dependency] private readonly ICurseStateSystem _curseStates = default!;
        [Dependency] private readonly INewEffectSystem _newEffects = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;

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

            // TODO simpler visibilty check, which assumes player is the onlooker
            if (_vis.HasLineOfSight(_gameSession.Player, drinker))
            {
                _mes.Display(Loc.GetString("Elona.Potion.Drinks", ("entity", drinker), ("item", potion)));
                _sounds.Play(Protos.Sound.Drink1, drinker);
            }

            TurnResult result = TurnResult.Failed;
            var obvious = false;
            foreach (var spec in potionComp.Effects.EnumerateEffectSpecs())
            {
                var args = new EffectCommonArgs()
                {
                    EffectSource = EffectSources.PotionDrunk,
                    CurseState = _curseStates.GetCurseState(potion),
                    Power = spec.Power,
                    TileRange = spec.MaxRange,
                    SkillLevel = spec.SkillLevel,
                    SourceItem = potion
                };
                var newResult = _newEffects.Apply(drinker, drinker, Spatial(drinker).Coordinates, spec.ID, EffectArgSet.Make(args));
                result = result.Combine(newResult);
                obvious = obvious || args.OutEffectWasObvious;
            }

            _stackSystem.Use(potion, 1);
            ApplyPotionHungerEffects(drinker);

            if (_gameSession.IsPlayer(drinker))
            {
                if (obvious && IsAlive(potion))
                {
                    _identify.IdentifyItem(potion, IdentifyState.Name);
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

        private void HandleImpactOther(EntityUid thrownPotion, PotionComponent potionComp, ThrownEntityImpactedOtherEvent args)
        {
            if (args.Handled)
                return;
            
            args.Handled = true;

            _mes.Display(Loc.GetString("Elona.Throw.Hits", ("entity", args.ImpactedWith)));
            _sounds.Play(Protos.Sound.Crush2, args.ImpactedWith);
            _sounds.Play(Protos.Sound.Drink1, args.ImpactedWith);

            var curseState = _curseStates.GetCurseState(thrownPotion);

            foreach (var spec in potionComp.Effects.EnumerateEffectSpecs())
            {
                var effectArgs = new EffectCommonArgs()
                {
                    EffectSource = EffectSources.PotionThrown,
                    CurseState = curseState,
                    Power = spec.Power,
                    TileRange = spec.MaxRange,
                    SkillLevel = spec.SkillLevel,
                    SourceItem = potionComp.Owner
                };
                _newEffects.Apply(args.Thrower, args.ImpactedWith, null, spec.ID, EffectArgSet.Make(effectArgs));
            }

            _stackSystem.Use(thrownPotion, 1);
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
                puddleComp.Effects = potionComp.Effects;
            }
            EnsureComp<CurseStateComponent>(puddleComp.Owner).CurseState = _curseStates.GetCurseState(potionComp.Owner);
        
            EntityManager.DeleteEntity(thrown);
        }

        private void HandlePotionPuddleSteppedOn(EntityUid puddle, PotionPuddleComponent potionComp, EntitySteppedOnEvent args)
        {
            _sounds.Play(Protos.Sound.Water, puddle);
            _sounds.Play(Protos.Sound.Drink1, args.Stepper);

            // TODO set source to original thrower!
            var source = args.Stepper;
            var target = args.Stepper;

            var curseState = _curseStates.GetCurseState(puddle);

            foreach (var spec in potionComp.Effects.EnumerateEffectSpecs())
            {
                var effectArgs = new EffectCommonArgs()
                {
                    EffectSource = EffectSources.PotionThrown,
                    CurseState = curseState,
                    Power = spec.Power,
                    TileRange = spec.MaxRange,
                    SkillLevel = spec.SkillLevel,
                    SourceItem = potionComp.Owner
                };
                _newEffects.Apply(source, target, args.Coords, spec.ID, EffectArgSet.Make(effectArgs));
            }

            EntityManager.DeleteEntity(puddle);
        }
    }
}
