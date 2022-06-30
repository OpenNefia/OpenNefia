﻿using OpenNefia.Content.Logic;
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

        public override void Initialize()
        {
            SubscribeComponent<MoveableComponent, CollideWithEventArgs>(HandleCollideWith, priority: EventPriorities.Low);
            SubscribeComponent<MoveableComponent, AfterMoveEventArgs>(CheckMovementIntoWater, priority: EventPriorities.Low);
        }

        private void HandleCollideWith(EntityUid uid, MoveableComponent _, CollideWithEventArgs args)
        {
            // >>>>>>>> shade2 / action.hsp:537        tc = cellChara...
            if (args.Handled)
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

            _mes.Display(Loc.GetString("Elona.Movement.Displace.Text", ("source", source), ("target", target)));
            if (_movement.SwapPlaces(source, target))
            {
                OnEntityDisplaced(source, target);
            }

            return true;
        }

        private void OnEntityDisplaced(EntityUid source, EntityUid target)
        {
            // >>>>>>>> shade2 / action.hsp:551            if cRowAct(tc) = rowActEat:if cActionPeriod(tc) > 0...
            if (_activities.TryGetActivity(target, out var activityComp))
            {
                var proto = _protos.Index(activityComp.PrototypeID);
                if (proto.InterruptOnDisplace)
                {
                    _mes.Display(Loc.GetString("Elona.Movement.Displace.InterruptActivity", ("source", source), ("target", target)));
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
    }
}