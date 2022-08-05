using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Directions;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.UI;
using OpenNefia.Content.Talk;

namespace OpenNefia.Content.VanillaAI
{
    public interface IVanillaAISystem : IEntitySystem
    {
        public EntityUid? GetTarget(EntityUid entity, VanillaAIComponent? ai = null);
        public void SetTarget(EntityUid entity, EntityUid? target, int aggro = 0, VanillaAIComponent? ai = null);
        public void ResetAI(EntityUid entity, VanillaAIComponent? ai = null);
    }

    /// <summary>
    /// Handles Elona's vanilla AI. The idea is that this can be replaced with whatever
    /// AI system you want by simply removing the <see cref="VanillaAIComponent"/> on the 
    /// entity prototype and writing another entity system that handles <see cref="NPCTurnStartedEvent"/>.
    /// </summary>
    public sealed partial class VanillaAISystem : EntitySystem, IVanillaAISystem
    {
        [Dependency] private readonly IMapRandom _mapRandom = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IMoveableSystem _movement = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IVisibilitySystem _vision = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IActionBashSystem _actionBash = default!;

        public override void Initialize()
        {
            SubscribeComponent<VanillaAIComponent, EntityTurnStartingEventArgs>(HandleTurnStarting);
            SubscribeComponent<VanillaAIComponent, NPCTurnStartedEvent>(HandleNPCTurnStarted, priority: EventPriorities.VeryLow);
        }

        private void HandleTurnStarting(EntityUid uid, VanillaAIComponent ai, EntityTurnStartingEventArgs args)
        {
            if (!EntityManager.IsAlive(ai.CurrentTarget))
            {
                SetTarget(uid, null, ai: ai);
            }
        }

        private void HandleNPCTurnStarted(EntityUid uid, VanillaAIComponent ai, ref NPCTurnStartedEvent args)
        {
            if (args.Handled)
                return;

            args.Handle(RunVanillaAI(uid, ai));
        }

        private bool HelpWithChoking(EntityUid entity, VanillaAIComponent ai, SpatialComponent spatial)
        {
            // >>>>>>>> elona122/shade2/ai.hsp:107 	if cRelation(cc)>=cNeutral{ ...
            if (_parties.TryGetLeader(entity, out var leader) && leader != entity && _effects.HasEffect(leader.Value, Protos.StatusEffect.Choking))
            {
                var leaderSpatial = Spatial(leader.Value);
                if (spatial.MapPosition.IsAdjacentTo(leaderSpatial.MapPosition))
                {
                    _actionBash.DoBash(entity, leaderSpatial.MapPosition);
                    return true;
                }
            }

            return false;
            // <<<<<<<< elona122/shade2/ai.hsp:109 	} ...
        }

        private bool TryToHeal(EntityUid entity, VanillaAIComponent ai, SpatialComponent spatial)
        {
            // TODO
            return false;
        }

        private bool TryToUseItem(EntityUid entity, VanillaAIComponent ai, SpatialComponent spatial)
        {
            // TODO
            return false;
        }

        private void DoAiTalk(EntityUid entity)
        {
            if (TryComp<TaughtWordsComponent>(entity, out var taughtWords))
            {
                if (_rand.OneIn(30))
                {
                    _mes.Display(_rand.Pick(taughtWords.TaughtWords), UiColors.MesSkyBlue);
                    return;
                }
            }

            if (TryComp<ToneComponent>(entity, out var tone) && !tone.IsTalkSilenced)
            {
                // TODO ai talk
            }
        }

        public TurnResult RunVanillaAI(EntityUid entity, VanillaAIComponent? ai = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entity, ref ai, ref spatial))
                return TurnResult.Failed;

            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return TurnResult.Failed;

            if (IsAlliedWithPlayer(entity))
                DecideAllyTarget(entity, ai, spatial);

            if (!EntityManager.IsAlive(ai.CurrentTarget))
                SetTarget(entity, GetDefaultTarget(entity), 0, ai);

            DoAiTalk(entity);

            if (HelpWithChoking(entity, ai, spatial))
                return TurnResult.Succeeded;

            if (TryToHeal(entity, ai, spatial))
                return TurnResult.Succeeded;

            if (TryToUseItem(entity, ai, spatial))
                return TurnResult.Succeeded;

            var target = ai.CurrentTarget;
            if (EntityManager.IsAlive(target)
                && (ai.Aggro > 0 || IsAlliedWithPlayer(entity)))
            {
                DoTargetedAction(entity, ai, spatial);
            }

            if (EntityManager.TryGetComponent(entity, out TurnOrderComponent turnOrder))
            {
                if (turnOrder.TotalTurnsTaken % 10 == 1)
                {
                    SearchForTarget(entity, map, ai, spatial);
                }
            }

            target = ai.CurrentTarget;
            if (EntityManager.IsAlive(target) && _gameSession.IsPlayer(target.Value))
            {
                var perceived = _vis.TryToPercieve(entity, target.Value);
                if (perceived && _factions.GetRelationTowards(entity, target.Value) <= Relation.Enemy)
                    SetTarget(entity, target, 30);
            }

            DoIdleAction(entity, ai);

            return TurnResult.Succeeded;
        }

        private bool SearchForTarget(EntityUid entity,
            IMap map,
            VanillaAIComponent ai, SpatialComponent spatial,
            int searchRadius = 5)
        {
            if (EntityManager.HasComponent<AINoTargetComponent>(entity))
                return true;

            for (int j = 0; j < searchRadius; j++)
            {
                var y = spatial.WorldPosition.Y - 2 + j;

                if (y >= 0 && y < map.Height)
                {
                    for (int i = 0; i < searchRadius; i++)
                    {
                        var x = spatial.WorldPosition.X - 2 + i;

                        if (x >= 0 && x < map.Width)
                        {
                            var onCellSpatial = _lookup.GetBlockingEntity(map.AtPos(x, y));

                            if (onCellSpatial != null && entity != onCellSpatial.Owner)
                            {
                                if (!EntityManager.HasComponent<AINoTargetComponent>(onCellSpatial.Owner)
                                    && _factions.GetRelationTowards(entity, onCellSpatial.Owner) <= Relation.Enemy)
                                {
                                    SetTarget(entity, onCellSpatial.Owner, 30, ai);
                                    _emoIcons.SetEmotionIcon(onCellSpatial.Owner, EmotionIcons.Angry, 2);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            // TODO stealth

            return false;
        }

        private EntityUid GetDefaultTarget(EntityUid entity)
        {
            if (_parties.TryGetLeader(entity, out var leader))
                return leader.Value;

            return _gameSession.Player;
        }

        private bool IsAlliedWithPlayer(EntityUid entity)
        {
            return _factions.GetRelationTowards(entity, _gameSession.Player) >= Relation.Ally;
        }

        private void DecrementAggro(VanillaAIComponent ai)
        {
            ai.Aggro = Math.Max(ai.Aggro - 1, 0);
        }

        public EntityUid? GetTarget(EntityUid entity, VanillaAIComponent? ai = null)
        {
            if (!Resolve(entity, ref ai))
                return null;

            return ai.CurrentTarget;
        }

        public void SetTarget(EntityUid entity, EntityUid? target, int aggro = 0, VanillaAIComponent? ai = null)
        {
            if (!Resolve(entity, ref ai))
                return;

            if (target == null)
                aggro = 0;

            ai.CurrentTarget = target;
            ai.Aggro = aggro;

            if (target != null && EntityManager.TryGetComponent(target.Value, out SpatialComponent targetSpatial))
            {
                ai.TurnsUntilMovement = 0;
                ai.DestinationCoords = targetSpatial.WorldPosition;
            }
        }

        private void DoTargetedAction(EntityUid entity, VanillaAIComponent ai, SpatialComponent spatial)
        {
            if (EntityManager.HasComponent<StatusBlindnessComponent>(entity))
            {
                if (_rand.Next(10) > 2)
                {
                    DoIdleAction(entity, ai);
                    return;
                }
            }
            if (EntityManager.HasComponent<StatusConfusionComponent>(entity))
            {
                if (_rand.Next(10) > 3)
                {
                    DoIdleAction(entity, ai);
                    return;
                }
            }

            if (IsAlliedWithPlayer(entity) && _gameSession.IsPlayer(ai.CurrentTarget!.Value))
            {
                DoAllyIdleAction(entity, ai);
                return;
            }

            if (EntityManager.HasComponent<StatusFearComponent>(entity))
            {
                MoveTowardsTarget(entity, ai, spatial, retreat: true);
                return;
            }

            if (EntityManager.HasComponent<StatusBlindnessComponent>(entity))
            {
                if (_rand.OneIn(3))
                {
                    DoIdleAction(entity, ai);
                    return;
                }
            }

            if (EntityManager.TryGetComponent(ai.CurrentTarget!.Value, out SpatialComponent targetSpatial))
            {
                if (spatial.MapPosition.TryDistanceTiled(targetSpatial.MapPosition, out var dist)
                    && dist != ai.TargetDistance
                    && _rand.Prob(ai.MoveChance))
                {
                    MoveTowardsTarget(entity, ai, spatial);
                    return;
                }
            }

            DoBasicAction(entity, ai);
        }

        private bool FollowPlayer(EntityUid entity, VanillaAIComponent ai)
        {
            if (ai.CalmAction != VanillaAICalmAction.Follow)
                return false;

            var player = _gameSession.Player;
            if (EntityManager.IsAlive(player))
            {
                ai.CurrentTarget = player;
                return MoveTowardsTarget(entity, ai);
            }
            return false;
        }

        private bool DoIdleAction(EntityUid entity, VanillaAIComponent ai)
        {
            if (FollowPlayer(entity, ai))
                return true;

            if (!_rand.OneIn(5))
                return true;

            if (DoAICalmAction(entity, ai))
                return true;

            Wander(entity, ai);
            return true;
        }

        private bool DoAICalmAction(EntityUid entity, VanillaAIComponent ai)
        {
            // TODO implement
            var ev = new OnAICalmActionEvent();
            return Raise(entity, ev);
        }

        private bool DoAllyIdleAction(EntityUid entity, VanillaAIComponent ai)
        {
            // >>>>>>>> shade2/ai.hsp:147 		if cRelation(cc)=cAlly:if tc=pc{ ...
            if (!IsAlive(ai.CurrentTarget))
                return false;

            // TODO shopkeeper movement
            var ev = new OnAllyCalmActionEvent();
            if (Raise(entity, ev))
                return true;

            if (Spatial(entity).MapPosition.TryDistanceTiled(
                Spatial(ai.CurrentTarget.Value).MapPosition,
                out var dist)
                && (dist > 2 || _rand.OneIn(3)))
            {
                return MoveTowardsTarget(entity, ai);
            }

            return DoIdleAction(entity, ai);
            // <<<<<<<< shade2/ai.hsp:161 			} ..
        }

        private void DecideAllyTarget(EntityUid ally, VanillaAIComponent ai, SpatialComponent spatial)
        {
            DecrementAggro(ai);

            var currentTarget = ai.CurrentTarget;

            bool updateTarget;
            if (!EntityManager.IsAlive(currentTarget))
            {
                updateTarget = true;
            }
            else
            {
                var relationToTarget = _factions.GetRelationTowards(ally, currentTarget.Value);
                var beingTargeted = GetTarget(currentTarget.Value) == ally;
                var targetNotImportant = currentTarget != null
                    && relationToTarget >= Relation.Hate
                    && !beingTargeted;

                updateTarget = !targetNotImportant || ai.Aggro <= 0;
            }

            if (!updateTarget)
                return;

            EntityUid? leader = _parties.GetLeaderOrNull(ally);

            if (leader == null && currentTarget != null
                && _factions.GetRelationTowards(ally, currentTarget.Value) >= Relation.Ally)
            {
                leader = currentTarget.Value;
            }

            if (leader != null && EntityManager.TryGetComponent(leader.Value, out VanillaAIComponent? leaderAi))
            {
                // If a party leader was attacked by something, make their allies target the attacker.
                var leaderAttacker = leaderAi.LastAttacker;

                if (EntityManager.IsAlive(leaderAttacker)
                    && _factions.GetRelationTowards(ally, leaderAttacker.Value) <= Relation.Enemy
                    && _vision.HasLineOfSight(ally, leaderAttacker.Value))
                {
                    SetTarget(ally, leaderAttacker.Value, 5, ai);
                }
                else
                {
                    leaderAi.LastAttacker = null;
                }

                // If the ally has no target at this point, make them target the same thing as the leader.
                if ((ai.CurrentTarget == null || ai.CurrentTarget == leader.Value)
                    && EntityManager.IsAlive(leader))
                {
                    var leaderTarget = leaderAi.CurrentTarget;
                    if (EntityManager.IsAlive(leaderTarget)
                        && _factions.GetRelationTowards(leader.Value, leaderTarget.Value) <= Relation.Enemy
                        && _vision.HasLineOfSight(ally, leaderTarget.Value))
                    {
                        SetTarget(ally, leaderTarget.Value, 5, ai);
                    }
                }
            }

            // The AI will occasionally lose sight of invisible targets.
            var target = ai.CurrentTarget;
            if (EntityManager.IsAlive(target) && !_vision.CanSeeEntity(ally, target.Value) && !_rand.OneIn(5))
            {
                ai.CurrentTarget = leader;
            }
        }

        public void ResetAI(EntityUid entity, VanillaAIComponent? ai = null)
        {
            if (!Resolve(entity, ref ai))
                return;

            SetTarget(entity, null, ai: ai);
            _factions.ClearAllPersonalRelations(entity);
        }
    }

    public sealed class OnAICalmActionEvent : HandledEntityEventArgs
    {
        public OnAICalmActionEvent()
        {
        }
    }

    public sealed class OnAllyCalmActionEvent : HandledEntityEventArgs
    {
        public OnAllyCalmActionEvent()
        {
        }
    }
}
