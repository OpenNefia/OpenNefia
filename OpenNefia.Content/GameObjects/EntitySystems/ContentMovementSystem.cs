using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Input;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Activity;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Sanity;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Effects;
using OpenNefia.Core.Game;
using OpenNefia.Content.EffectMap;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using System.Collections.Immutable;
using OpenNefia.Content.Encounters;
using OpenNefia.Content.Combat;

namespace OpenNefia.Content.GameObjects
{
    public sealed class ContentMovementSystem : EntitySystem
    {
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IInputSystem _input = default!;
        [Dependency] private readonly ICombatSystem _combat = default!;
        [Dependency] private readonly IMoveableSystem _movement = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly ISanitySystem _sanity = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly CommonEffectsSystem _commonEffects = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IEffectMapSystem _effectMaps = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEncounterSystem _encounters = default!;

        public override void Initialize()
        {
            SubscribeComponent<MoveableComponent, CollideWithEventArgs>(HandleCollideWith, priority: EventPriorities.Low);
            SubscribeComponent<MoveableComponent, AfterMoveEventArgs>(CheckMovementIntoWater, priority: EventPriorities.Low);
            SubscribeComponent<PlayerComponent, AfterMoveEventArgs>(LeaveFootsteps, priority: EventPriorities.Low);
            SubscribeComponent<PlayerComponent, AfterMoveEventArgs>(ProcRandomEncounters, priority: EventPriorities.VeryLow);
        }

        private void HandleCollideWith(EntityUid uid, MoveableComponent _, CollideWithEventArgs args)
        {
            // >>>>>>>> shade2 / action.hsp:537        tc = cellChara...
            if (args.Handled)
                return;

            if (!_gameSession.IsPlayer(uid))
                return;

            if (!EntityManager.HasComponent<MoveableComponent>(args.Target))
                return;

            var relation = _factions.GetRelationTowards(uid, args.Target);

            if (ShouldDisplace(relation))
            {
                if (TryDisplace(uid, args.Target))
                {
                    args.Handle(TurnResult.Succeeded);
                    return;
                }
            }

            if (relation <= Relation.Dislike)
            {
                var turnResult = _combat.MeleeAttack(uid, args.Target);
                if (turnResult != null)
                {
                    args.Handle(turnResult.Value);
                    return;
                }
            }
            // <<<<<<<< shade2/action.hsp:563 		goto *turn_end ..
        }

        private bool ShouldDisplace(Relation relation)
        {
            return relation >= Relation.Ally
                || relation == Relation.Dislike && (!_config.GetCVar(CCVars.GameAttackNeutralNPCs) || _input.PlayerIsRunning());
        }

        private bool TryDisplace(EntityUid source, EntityUid target)
        {
            // TODO sandbag

            var sourceSpatial = Spatial(source);
            var oldPosition = sourceSpatial.MapPosition;

            _mes.Display(Loc.GetString("Elona.Movement.Displace.Text", ("source", source), ("target", target)), entity: source);
            if (_movement.SwapPlaces(source, target))
            {
                OnEntityDisplaced(source, target);
            }

            var ev = new AfterMoveEventArgs(oldPosition, sourceSpatial.MapPosition);
            RaiseEvent(source, ev);

            return true;
        }

        private void OnEntityDisplaced(EntityUid source, EntityUid target)
        {
            // >>>>>>>> shade2 / action.hsp:551            if cRowAct(tc) = rowActEat:if cActionPeriod(tc) > 0...
            if (_activities.TryGetActivity(target, out var activityComp))
            {
                if (activityComp.InterruptOnDisplace)
                {
                    _mes.Display(Loc.GetString("Elona.Movement.Displace.InterruptActivity", ("source", source), ("target", target)), entity: target);
                    _activities.RemoveActivity(target);
                }
            }

            // <<<<<<<< shade2 / action.hsp:551            if cRowAct(tc) = rowActEat:if cActionPeriod(tc) > 0..
        }

        private void CheckMovementIntoWater(EntityUid uid, MoveableComponent component, AfterMoveEventArgs args)
        {
            var spatial = Spatial(uid);

            if (!TryMap(uid, out var map))
                return;

            var tile = map.GetTilePrototype(spatial.WorldPosition);
            if (tile != null && tile.Kind == TileKind.Water)
            {
                if (tile.Kind2 == TileKind.MountainWater)
                {
                    _sanity.HealInsanity(uid, 1);
                }

                _effectMaps.AddEffectMap(Protos.Asset.EffectMapRipple, spatial.MapPosition);

                if (!_effects.HasEffect(uid, Protos.StatusEffect.Wet))
                {
                    _commonEffects.GetWet(uid, 20);
                }

                if (_gameSession.IsPlayer(uid))
                {
                    _audio.Play(Protos.Sound.Water2, spatial.MapPosition);
                }
            }
        }

        public static readonly PrototypeId<SoundPrototype>[] FootstepSounds = new[]
        {
            Protos.Sound.Foot1a,
            Protos.Sound.Foot1b,
        };

        public static readonly PrototypeId<SoundPrototype>[] SnowFootstepSounds = new[]
        {
            Protos.Sound.Foot2a,
            Protos.Sound.Foot2b,
            Protos.Sound.Foot2c,
        };

        public static readonly PrototypeId<AssetPrototype>[] SnowEffectMaps = new[]
        {
            Protos.Asset.EffectMapSnow1,
            Protos.Asset.EffectMapSnow2,
        };

        private int _footstep = 0;

        private void LeaveFootsteps(EntityUid uid, PlayerComponent component, AfterMoveEventArgs args)
        {
            if (!_gameSession.IsPlayer(uid) || !TryMap(uid, out var map, _mapMan))
                return;

            if (args.OldPosition != args.NewPosition)
            {
                var angle = Spatial(uid).Direction.ToAngle();
                var tileProto = map.GetTilePrototype(args.NewPosition.Position);

                if (tileProto != null)
                {
                    if (tileProto.Kind == TileKind.Snow)
                    {
                        _effectMaps.AddEffectMap(_rand.Pick(SnowEffectMaps), args.NewPosition, 10, angle, EffectMapType.Fade);
                        _audio.Play(SnowFootstepSounds[_footstep % SnowFootstepSounds.Length], args.NewPosition);
                        _footstep += _rand.Next(2);
                    }
                    else if (HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
                    {
                        _effectMaps.AddEffectMap(Protos.Asset.EffectMapFoot, args.NewPosition, 10, angle, EffectMapType.Fade);
                        _audio.Play(FootstepSounds[_footstep % FootstepSounds.Length], args.NewPosition);
                        _footstep++;
                    }
                }
            }
        }

        private void ProcRandomEncounters(EntityUid uid, PlayerComponent component, AfterMoveEventArgs args)
        {
            if (args.Handled)
                return;

            if (!_gameSession.IsPlayer(uid) || !TryMap(uid, out var map, _mapMan))
                return;

            var spatial = Spatial(uid);

            if (!HasComp<MapTypeWorldMapComponent>(map.MapEntityUid) || _lookup.QueryLiveEntitiesAtCoords<MObjComponent>(spatial.MapPosition).Any())
                return;

            var id = _encounters.PickRandomEncounterID();
            if (id != null)
            {
                _encounters.StartEncounter(id);
                args.Handled = true;
            }
        }
    }
}