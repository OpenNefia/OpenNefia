using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.Targetable;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.Factions;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Effects.New.EffectTargets
{
    public sealed class VanillaEffectTargetsSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ITargetingSystem _targeting = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly ITargetableSystem _targetable = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly ISpatialSystem _spatials = default!;
        [Dependency] private readonly IPlayerQuery _playerQueries = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectTargetSelfComponent, GetEffectPlayerTargetEvent>(GetTarget_Self);
            SubscribeComponent<EffectTargetSelfOrNearbyComponent, GetEffectPlayerTargetEvent>(GetTarget_SelfOrNearby);
            SubscribeComponent<EffectTargetNearbyComponent, GetEffectPlayerTargetEvent>(GetTarget_Nearby);
            SubscribeComponent<EffectTargetOtherComponent, GetEffectPlayerTargetEvent>(GetTarget_Other);
            SubscribeComponent<EffectTargetDirectionComponent, GetEffectPlayerTargetEvent>(GetTarget_Direction);
            SubscribeComponent<EffectTargetPositionComponent, GetEffectPlayerTargetEvent>(GetTarget_Ground);

            SubscribeComponent<EffectTargetSummoningComponent, GetEffectPlayerTargetEvent>(GetPlayerTarget_Summoning);
            SubscribeComponent<EffectTargetSummoningComponent, GetEffectAITargetEvent>(GetAITarget_Summoning);
        }

        private void GetTarget_Self(EntityUid uid, EffectTargetSelfComponent component, GetEffectPlayerTargetEvent args)
        {
            if (args.Handled)
                return;

            args.Handle(args.Source, Spatial(args.Source).Coordinates);
        }

        private void GetTarget_SelfOrNearby(EntityUid uid, EffectTargetSelfOrNearbyComponent component, GetEffectPlayerTargetEvent args)
        {
            if (args.Handled)
                return;

            var effectCommon = args.Args.Ensure<EffectCommonArgs>();

            // TODO wands
            var wasCastedByWand = effectCommon.EffectSource == EffectSources.Wand;
            if (wasCastedByWand)
            {
                if (TryGetDirectionalTarget(args.Source, out var target))
                    args.Handle(target);
                else
                {
                    _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                    effectCommon.OutEffectWasObvious = false;
                    args.Handle(null);
                }
            }
            else
            {
                args.Handle(args.Source, Spatial(args.Source).Coordinates);
            }
        }

        private void GetTarget_Nearby(EntityUid uid, EffectTargetNearbyComponent component, GetEffectPlayerTargetEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:1570 		if cc=pc{ ...
            var effectCommon = args.Args.Ensure<EffectCommonArgs>();

            if (TryGetDirectionalTarget(args.Source, out var target))
            {
                args.Handle(target);
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                effectCommon.OutEffectWasObvious = false;
                args.Handle(null);
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1576 			}else{ ...
        }

        private void GetTarget_Other(EntityUid effect, EffectTargetOtherComponent component, GetEffectPlayerTargetEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:1586 				if fov_los(cX(cc),cY(cc),tgLocX,tgLocY)=false{ ...
            var effectCommon = args.Args.Ensure<EffectCommonArgs>();
            if (!TryComp<VanillaAIComponent>(args.Source, out var vai))
            {
                effectCommon.OutEffectWasObvious = false;
                args.Handle(null, null);
                return;
            }

            if (component.CanTargetGround && vai.CurrentTargetLocation != null)
            {
                var coords = vai.CurrentTargetLocation.Value;
                if (!TryMap(coords, out var map) || !_vis.HasLineOfSight(args.Source, coords))
                {
                    _mes.Display(Loc.GetString("Elona.Targeting.Prompt.CannotSeeLocation"));
                    effectCommon.OutEffectWasObvious = false;
                    args.Handle(null, null);
                }

                _targetable.TryGetTargetableEntity(coords, out var target);
                args.Handle(target?.Owner, coords);
                return;
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1597 				} ...

            // >>>>>>>> elona122/shade2/proc.hsp:1615 		if cc=pc{ ...
            var range = args.Args.TileRange;

            if (!TryPickTarget(args.Source, component.PromptIfFriendly, range, out var targetEnt))
            {
                effectCommon.OutEffectWasObvious = false;
                args.Handle(null, null);
                return;
            }

            args.Handle(targetEnt);
            // <<<<<<<< elona122/shade2/proc.hsp:1629 		return true ...
        }

        private void GetTarget_Direction(EntityUid uid, EffectTargetDirectionComponent component, GetEffectPlayerTargetEvent args)
        {
            if (args.Handled)
                return;

            var effectCommon = args.Args.Ensure<EffectCommonArgs>();

            var dir = _uiManager.Query<DirectionPrompt, DirectionPrompt.Args, DirectionPrompt.Result>(new(args.Source, Loc.GetString("Elona.Targeting.Prompt.WhichDirection")));

            if (!dir.HasValue)
            {
                _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                effectCommon.OutEffectWasObvious = false;
                args.Handle(null);
                return;
            }

            _targetable.TryGetTargetableEntity(dir.Value.Coords, out var target);
            args.Handle(target?.Owner, dir.Value.EntityCoords);
        }

        private void GetTarget_Ground(EntityUid uid, EffectTargetPositionComponent component, GetEffectPlayerTargetEvent args)
        {
            if (args.Handled)
                return;

            var effectCommon = args.Args.Ensure<EffectCommonArgs>();

            // >>>>>>>> elona122/shade2/proc.hsp:1591 				}else{ ...
            var coords = Spatial(args.Source).MapPosition;
            var promptArgs = new PositionPrompt.Args(coords, args.Source)
            {
                AlwaysShowLine = component.AlwaysShowLine
            };
            var posResult = _uiManager.Query<PositionPrompt, PositionPrompt.Args, PositionPrompt.Result>(promptArgs);

            if (!posResult.HasValue)
            {
                _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                effectCommon.OutEffectWasObvious = false;
                args.Handle(null);
                return;
            }

            if (!posResult.Value.CanSee)
            {
                _mes.Display(Loc.GetString("Elona.Targeting.Prompt.CannotSeeLocation"));
                effectCommon.OutEffectWasObvious = false;
                args.Handle(null);
                return;
            }

            _targetable.TryGetTargetableEntity(posResult.Value.Coords, out var target);
            args.Handle(target?.Owner, posResult.Value.EntityCoords);
            // <<<<<<<< elona122/shade2/proc.hsp:1597 				} ...
        }

        private bool TryPickTarget(EntityUid source, bool promptIfFriendly, int maxRangeTiled, [NotNullWhen(true)] out EntityUid? targetEnt)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1615 		if cc=pc{ ...
            if (!_targeting.TrySearchForTarget(source, out targetEnt))
            {
                targetEnt = null;
                return false;
            }

            if (promptIfFriendly
                && _factions.GetRelationTowards(source, targetEnt.Value) >= Relation.Neutral
                && !_targeting.PromptReallyAttack(source, targetEnt.Value))
            {
                targetEnt = null;
                return false;
            }

            if (!_spatials.TryMapDistanceTiled(source, targetEnt.Value, out var dist) || dist > maxRangeTiled)
            {
                _mes.Display(Loc.GetString("Elona.Targeting.Prompt.OutOfRange"), combineDuplicates: true);
                targetEnt = null;
                return false;
            }

            if (!_vis.HasLineOfSight(source, targetEnt.Value))
            {
                _mes.Display(Loc.GetString("Elona.Targeting.Prompt.CannotSeeLocation"));
                targetEnt = null;
                return false;
            }

            return true;
            // <<<<<<<< elona122/shade2/proc.hsp:1629 		return true ...
        }

        private bool TryGetDirectionalTarget(EntityUid source, [NotNullWhen(true)] out EntityUid? targetEnt)
        {
            var dir = _uiManager.Query<DirectionPrompt, DirectionPrompt.Args, DirectionPrompt.Result>(new(source, Loc.GetString("Elona.Targeting.Prompt.WhichDirection")));

            if (!dir.HasValue)
            {
                targetEnt = null;
                return false;
            }

            if (!_targetable.TryGetTargetableEntity(dir.Value.Coords, out var target))
            {
                targetEnt = null;
                return false;
            }

            targetEnt = target.Owner;
            return true;
        }

        /// <summary>
        /// Set the target as the player if they are casting a summoning skill.
        /// </summary>
        private void GetPlayerTarget_Summoning(EntityUid uid, EffectTargetSummoningComponent component, GetEffectPlayerTargetEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1567 	if skillType(efId)=skSummon : if cc=pc : tc=pc :  ...
            if (args.Handled)
                return;

            if (_gameSession.IsPlayer(args.Source))
            {
                args.Handle(args.Source);
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1567 	if skillType(efId)=skSummon : if cc=pc : tc=pc :  ...
        }

        /// <summary>
        /// Use directional targeting if the AI is casting a summoning spell.
        /// Also prevent the AI from spamming summoning skills in undesirable situations:
        /// - If they're an ally
        /// - If the map is the pet arena
        /// - If the casting frequency is too high
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        private void GetAITarget_Summoning(EntityUid uid, EffectTargetSummoningComponent component, GetEffectAITargetEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:1298 	if cc!pc:if skillType(efId)=skSummon{ ...
            // TODO pet arena
            var isPetArena = false;
            if (_factions.GetRelationToPlayer(args.Source) == Relation.Ally || isPetArena)
            {
                args.Handle(null);
                return;
            }

            // 40% chance to cast if an NPC
            if (TryComp<TurnOrderComponent>(args.Source, out var turnOrder) && turnOrder.TotalTurnsTaken % 10 > 4)
            {
                args.Handle(null);
                return;
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1302 		} ...

            // In vanilla, all summoning skills use tgEnemy for AI targeting.

            // >>>>>>>> elona122/shade2/init.hsp:2983 	skillSet spSummon	,rsMAG	,skSummon			,15	,tgEnemy ...
            var range = args.Args.TileRange;

            // TODO
            //if (!TryPickTarget(args.Source, false, range, out var targetEnt))
            //{
            //    args.Handle(null, null);
            //    return;
            //}

            //args.Handle(targetEnt);
            // <<<<<<<< elona122/shade2/init.hsp:2989 	skillSet actSummonSister,rsMAG	,skSummon			,15	,t ...
        }
    }
}