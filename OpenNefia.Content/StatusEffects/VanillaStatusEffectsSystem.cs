using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Maps;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Random;
using OpenNefia.Content.Logic;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.UI;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Game;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Hunger;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Directions;

namespace OpenNefia.Content.StatusEffects
{
    public sealed class VanillaStatusEffectsSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IHungerSystem _hunger = default!;


        public override void Initialize()
        {
            SubscribeComponent<StatusDrunkComponent, EntityPassTurnEventArgs>(HandlePassTurnDrunk);
            SubscribeComponent<StatusEffectsComponent, BeforeMoveEventArgs>(ProcRandomMovement);
        }

        public const int DrunkLevelHeavy = 45;

        private bool CanPickFightWith(SpatialComponent drunkard, SpatialComponent opponent)
        {
            return EntityManager.IsAlive(opponent.Owner)
                && drunkard.Owner != opponent.Owner
                && drunkard.MapPosition.TryDistanceTiled(opponent.MapPosition, out var dist)
                && dist <= 5
                && _vis.HasLineOfSight(drunkard.Owner, opponent.Owner)
                && _rand.OneIn(3);
        }

        private void HandlePassTurnDrunk(EntityUid drunkard, StatusDrunkComponent component, EntityPassTurnEventArgs args)
        {
            if (args.Handled)
                return;

            if (_rand.OneIn(200) && !_gameSession.IsPlayer(drunkard))
                TryToPickDrunkardFight(drunkard);

            if (_statusEffects.GetTurns(drunkard, Protos.StatusEffect.Drunk) >= DrunkLevelHeavy
                || (TryComp<HungerComponent>(drunkard, out var hunger) && hunger.Nutrition >= HungerLevels.Vomit))
            {
                if (_rand.OneIn(60))
                {
                    _hunger.Vomit(drunkard);
                    args.Handle(TurnResult.Failed);
                    return;
                }
            }
        }

        private void TryToPickDrunkardFight(EntityUid drunkard)
        {
            if (!TryMap(drunkard, out var map) || HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
                return;

            var drunkardSpatial = Spatial(drunkard);

            var opponentPair = _lookup.EntityQueryInMap<SpatialComponent, VanillaAIComponent>(map)
                .Where(pair => CanPickFightWith(drunkardSpatial, pair.Item1))
                .FirstOrDefault();

            if (opponentPair == default)
                return;

            var (opponentSpatial, opponentAI) = opponentPair;
            var opponent = opponentSpatial.Owner;

            var isInFov = _vis.IsInWindowFov(drunkard) || _vis.IsInWindowFov(opponent);

            if (isInFov)
            {
                _mes.Display(Loc.GetString("Elona.StatusEffects.Drunk.GetsTheWorse", ("chara", drunkard), ("target", opponent)), UiColors.MesSkyBlue);
                _mes.Display(Loc.GetString("Elona.StatusEffects.Drunk.Dialog"));
            }

            if (_rand.OneIn(4) && !_gameSession.IsPlayer(opponent) && isInFov)
            {
                _mes.Display(Loc.GetString("Elona.StatusEffects.Drunk.Annoyed.Text"), UiColors.MesSkyBlue);
                _mes.Display(Loc.GetString("Elona.StatusEffects.Drunk.Annoyed.Dialog"));

                // XXX: This may not be correct.
                if (TryComp<FactionComponent>(opponent, out var faction))
                {
                    _factions.SetPersonalRelationTowards(opponent, drunkard, Relation.Enemy);

                    _vanillaAI.SetTarget(opponent, drunkard, 20);
                    _emoIcons.SetEmotionIcon(opponent, "Elona.Angry", 2);
                }
            }
        }

        private void ProcRandomMovement(EntityUid uid, StatusEffectsComponent statusEffects, BeforeMoveEventArgs args)
        {
            var stumble = false;

            if (_statusEffects.HasEffect(uid, Protos.StatusEffect.Dimming, statusEffects)
                && _statusEffects.GetTurns(uid, Protos.StatusEffect.Dimming, statusEffects) + 10 > _rand.Next(60))
            {
                stumble = true;
            }

            if (_statusEffects.HasEffect(uid, Protos.StatusEffect.Drunk, statusEffects) && _rand.OneIn(5))
            {
                _mes.Display(Loc.GetString("Elona.StatusEffects.Drunk.Move"), UiColors.MesSkyBlue);
                stumble = true;
            }

            if (stumble)
            {
                args.OutNewPosition = new(args.DesiredPosition.MapId, 
                    Spatial(uid).WorldPosition + DirectionUtility.RandomDirections().First().ToIntVec());
            }
        }
    }
}
