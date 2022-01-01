using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.TurnOrder;
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

namespace OpenNefia.Content.VanillaAI
{
    /// <summary>
    /// Handles Elona's vanilla AI. The idea is that this can be replaced with whatever
    /// AI system you want by simply removing the <see cref="VanillaAIComponent"/> on the 
    /// entity prototype and writing another entity system that handles <see cref="NPCTurnStartedEvent"/>.
    /// </summary>
    public sealed partial class VanillaAISystem : EntitySystem
    {
        [Dependency] private readonly IMapRandom _mapRandom = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly MoveableSystem _movement = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IVisibilitySystem _vision = default!;
        [Dependency] private readonly IRandom _random = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<VanillaAIComponent, NPCTurnStartedEvent>(HandleNPCTurnStarted, nameof(HandleNPCTurnStarted));
            SubscribeAIActions();
        }

        private void HandleNPCTurnStarted(EntityUid uid, VanillaAIComponent ai, ref NPCTurnStartedEvent args)
        {
            if (args.Handled)
                return;

            args.Handle(RunVanillaAI(uid, ai));
        }

        public TurnResult RunVanillaAI(EntityUid entity, VanillaAIComponent? ai = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entity, ref ai, ref spatial))
                return TurnResult.Failed;

            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return TurnResult.Failed;

            if (IsAlliedWithPlayer(entity))
            {
                DecideAllyTarget(entity, ai, spatial);
            }

            if (!EntityManager.IsAlive(ai.CurrentTarget))
            {
                SetTarget(entity, GetDefaultTarget(entity), 0, ai);
            }

            // TODO choking
            // TODO healing
            // TODO items

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

            // TODO try to perceive

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
                            var onCellSpatial = GetBlockingEntity(map, new Vector2i(x, y));

                            if (onCellSpatial != null)
                            {
                                var onCellUid = onCellSpatial.OwnerUid;

                                if (!EntityManager.HasComponent<AINoTargetComponent>(onCellUid)
                                    && _factions.GetRelationTowards(entity, onCellUid) <= Relation.Enemy)
                                {
                                    SetTarget(entity, onCellUid, 30, ai);
                                    // TODO emotion icon
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
            return _parties.GetSupremeCommander(entity)?.OwnerUid ?? _gameSession.Player!.Uid;
        }

        private bool IsAlliedWithPlayer(EntityUid entity)
        {
            return _factions.GetRelationTowards(entity, _gameSession.Player!.Uid) >= Relation.Ally;
        }

        private void DecrementAggro(VanillaAIComponent ai)
        {
            ai.Aggro = Math.Max(ai.Aggro - 1, 0);
        }

        private EntityUid? GetTarget(EntityUid entity, VanillaAIComponent? ai = null)
        {
            if (!Resolve(entity, ref ai))
                return null;

            return ai.CurrentTarget;
        }

        private void SetTarget(EntityUid entity, EntityUid? target, int aggro, VanillaAIComponent? ai = null)
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
            if (EntityManager.HasComponent<StatusBlindComponent>(entity))
            {
                if (_random.Next(10) > 2)
                {
                    DoIdleAction(entity, ai);
                    return;
                }
            }
            if (EntityManager.HasComponent<StatusConfusionComponent>(entity))
            {
                if (_random.Next(10) > 3)
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

            if (EntityManager.HasComponent<StatusBlindComponent>(entity))
            {
                if (_random.OneIn(3))
                {
                    DoIdleAction(entity, ai);
                    return;
                }
            }

            if (EntityManager.TryGetComponent(ai.CurrentTarget!.Value, out SpatialComponent targetSpatial))
            {
                if (spatial.MapPosition.TryDistanceTiled(targetSpatial.MapPosition, out var dist)
                    && dist != ai.TargetDistance
                    && _random.Prob(ai.MoveChance))
                {
                    MoveTowardsTarget(entity, ai, spatial);
                    return;
                }
            }

            DoBasicAction(entity, ai);
        }

        private void DoIdleAction(EntityUid entity, VanillaAIComponent ai)
        {
            if (FollowPlayer(entity, ai))
                return;

            if (DoAICalmAction(entity, ai))
                return;

            if (GoToPresetAnchor(entity, ai))
                return;

            Wander(entity, ai);
        }

        private bool FollowPlayer(EntityUid entity, VanillaAIComponent ai)
        {
            if (ai.CalmAction != VanillaAICalmAction.FollowPlayer)
                return false;

            var player = _gameSession.Player?.Uid;
            if (EntityManager.IsAlive(player))
            {
                ai.CurrentTarget = player;
                return MoveTowardsTarget(entity, ai);
            }
            return false;
        }

        private bool DoAICalmAction(EntityUid entity, VanillaAIComponent ai)
        {
            if (_random.OneIn(5))
                return true;

            // TODO

            return false;
        }

        private void DoAllyIdleAction(EntityUid entity, VanillaAIComponent ai)
        {
            // TODO
        }

        private void DoBasicAction(EntityUid entity, VanillaAIComponent ai)
        {
            if (ai.CurrentTarget == null)
            {
                return;
            }

            var target = _parties.GetSupremeCommander(ai.CurrentTarget.Value)?.OwnerUid ?? ai.CurrentTarget.Value;

            RunMeleeAction(entity, target);
        }

        private void RunMeleeAction(EntityUid entity, EntityUid target)
        {
            var action = EntityManager.SpawnEntity(null, new EntityCoordinates(target, Vector2i.Zero));
            action.AddComponent<AIActionMeleeComponent>();

            RunAIAction(entity, action.Uid);

            EntityManager.DeleteEntity(action);
        }

        private TurnResult RunAIAction(EntityUid entity, EntityUid action)
        {
            var ev = new RunAIActionEvent(entity);
            RaiseLocalEvent(action, ev);
            return ev.TurnResult;
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

            var leader = _parties.GetSupremeCommander(ally);
            
            if (leader == null && currentTarget != null
                && _factions.GetRelationTowards(ally, currentTarget.Value) >= Relation.Ally)
            {
                leader = EntityManager.EnsureComponent<PartyComponent>(currentTarget.Value);
            }

            var map = _mapManager.GetMap(spatial.MapID);

            if (leader != null && EntityManager.TryGetComponent(leader.OwnerUid, out VanillaAIComponent? leaderAi))
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
                if ((ai.CurrentTarget == null || ai.CurrentTarget == leader.OwnerUid)
                    && EntityManager.IsAlive(leader.OwnerUid))
                {
                    var leaderTarget = leaderAi.CurrentTarget;
                    if (EntityManager.IsAlive(leaderTarget)
                        && _factions.GetRelationTowards(leader.OwnerUid, leaderTarget.Value) <= Relation.Enemy
                        && _vision.HasLineOfSight(ally, leaderTarget.Value))
                    {
                        SetTarget(ally, leaderTarget.Value, 5, ai);
                    }
                }
            }

            // The AI will occasionally lose sight of invisible targets.
            var target = ai.CurrentTarget;
            if (EntityManager.IsAlive(target) && !_vision.CanSeeEntity(ally, target.Value) && !_random.OneIn(5))
            {
                ai.CurrentTarget = leader?.OwnerUid;
            }
        }
    }
}
