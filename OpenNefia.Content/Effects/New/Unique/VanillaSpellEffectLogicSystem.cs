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
using OpenNefia.Content.Maps;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Parties;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Oracle;
using OpenNefia.Core.Game;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Materials;
using Love;
using OpenNefia.Content.Effects.New;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.BaseAnim;

namespace OpenNefia.Content.Effects.New.Unique
{
    public sealed class VanillaSpellEffectLogicSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMapTilesetSystem _mapTilesets = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly ICurseStateSystem _curseStates = default!;
        [Dependency] private readonly IOracleSystem _oracle = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IIdentifySystem _identifies = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly INewEffectSystem _newEffects = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly IRefreshSystem _refreshes = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectIdentifyComponent, ApplyEffectDamageEvent>(Apply_Identify);
            SubscribeComponent<EffectUncurseComponent, ApplyEffectDamageEvent>(Apply_Uncurse);
            SubscribeComponent<EffectOracleComponent, ApplyEffectDamageEvent>(Apply_Oracle);
            SubscribeComponent<EffectWallCreationComponent, ApplyEffectDamageEvent>(Apply_WallCreation);
        }

        private void Apply_Identify(EntityUid uid, EffectIdentifyComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:2427 	if cc!pc:txtNothingHappen:obvious=false:swbreak ...
            if (args.AffectedTileIndex > 0)
                return;

            EntityUid? targetItem = args.CommonArgs.TargetItem;

            if (!IsAlive(targetItem))
            {
                if (_gameSession.IsPlayer(args.Source))
                {
                    var invContext = new InventoryContext(args.Source, args.Source, new IdentifyInventoryBehavior());
                    var invResult = _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(invContext);

                    if (!invResult.HasValue || !IsAlive(invResult.Value.SelectedItem))
                    {
                        // XXX: Maybe the player can cancel here to not expend the item?
                        args.Handle(TurnResult.Failed);
                        return;
                    }
                    targetItem = invResult.Value.SelectedItem.Value;
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                    args.Args.Ensure<EffectCommonArgs>().OutEffectWasObvious = false;
                    args.Handle(TurnResult.Failed);
                    return;
                }
            }
            // <<<<<<<< elona122/shade2/proc.hsp:2429 	invCtrl=13:snd seInv:gosub *com_inventory ...

            // >>>>>>>> elona122/shade2/command.hsp:3845 			screenupdate=-1:gosub *screen_draw ...
            TurnResult turnResult;
            var identifyResult = _identifies.TryToIdentify(targetItem.Value, args.OutDamage);
            if (identifyResult.ChangedState)
            {
                if (identifyResult.NewState == IdentifyState.Full)
                {
                    _mes.Display(Loc.GetString("Elona.Identify.Result.Fully", ("source", args.Source), ("item", targetItem.Value), ("prevItemName", identifyResult.PreviousItemName)));
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Identify.Result.Partially", ("source", args.Source), ("item", targetItem.Value), ("prevItemName", identifyResult.PreviousItemName)));
                }
                turnResult = TurnResult.Succeeded;
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.Identify.Result.NeedMorePower", ("source", args.Source), ("item", targetItem.Value)));
                turnResult = TurnResult.Failed;
            }

            _stacks.TryStackAtSamePos(targetItem.Value);
            args.Handle(turnResult);
            // <<<<<<<< elona122/shade2/command.hsp:3849 			invSubRoutine=false:return true ...
        }

        private void Apply_Uncurse(EntityUid uid, EffectUncurseComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.InnerTarget == null)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:2459 	case spUncurse ...
            switch (args.CommonArgs.CurseState)
            {
                case CurseState.Normal:
                    _mes.Display(Loc.GetString("Elona.Effect.Uncurse.Power.Normal", ("source", args.Source), ("target", args.InnerTarget)));
                    break;
                case CurseState.Blessed:
                    _mes.Display(Loc.GetString("Elona.Effect.Uncurse.Power.Blessed", ("source", args.Source), ("target", args.InnerTarget)));
                    break;
                case CurseState.Cursed:
                case CurseState.Doomed:
                    _mes.Display(Loc.GetString("Elona.Effect.Common.ItIsCursed"));
                    var result = _newEffects.Apply(args.Source, args.InnerTarget, null, Protos.Effect.ActionCurse, args.Args);
                    args.Handle(result);
                    return;
            }

            bool CanUncurse(CurseStateComponent curseState)
            {
                if (curseState.CurseState == CurseState.Blessed || curseState.CurseState == CurseState.Normal)
                    return false;

                // Only uncurse items if the scroll is blessed.
                // Else, it only affects equipment.
                if (args.CommonArgs.CurseState != CurseState.Blessed)
                {
                    if (!_equipSlots.IsEquippedOnAnySlot(curseState.Owner))
                        return false;
                }

                return true;
            }

            var totalUncursed = 0;
            var totalResisted = 0;

            foreach (var curseState in _lookup.EntityQueryDirectlyIn<CurseStateComponent>(args.InnerTarget.Value).Where(CanUncurse))
            {
                var minPowerNeeded = 0;
                if (curseState.CurseState == CurseState.Cursed)
                    minPowerNeeded = _rand.Next(200) + 1;
                else if (curseState.CurseState == CurseState.Doomed)
                    minPowerNeeded = _rand.Next(1000) + 1;

                if (args.CommonArgs.CurseState == CurseState.Blessed)
                {
                    minPowerNeeded = minPowerNeeded / 2 + 1;
                }

                if (minPowerNeeded > 0 && args.OutDamage >= minPowerNeeded)
                {
                    totalUncursed++;
                    curseState.CurseState = CurseState.Normal;
                    _stacks.TryStackAtSamePos(curseState.Owner);
                }
                else
                {
                    totalResisted++;
                }
            }

            if (totalUncursed > 0)
            {
                if (args.CommonArgs.CurseState == CurseState.Blessed)
                {
                    _mes.Display(Loc.GetString("Elona.Effect.Uncurse.Apply.Item", ("source", args.Source), ("target", args.InnerTarget.Value)));
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Effect.Uncurse.Apply.Equipment", ("source", args.Source), ("target", args.InnerTarget.Value)));
                }
            }

            if (totalResisted > 0)
            {
                _mes.Display(Loc.GetString("Elona.Effect.Uncurse.Apply.Resisted", ("source", args.Source), ("target", args.InnerTarget.Value)));
            }

            var obvious = true;

            if (totalUncursed == 0 && totalResisted == 0)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                obvious = false;
            }
            else
            {
                var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSparkle);
                _mapDrawables.Enqueue(anim, args.InnerTarget.Value);
            }

            _refreshes.Refresh(args.InnerTarget.Value);
            args.CommonArgs.OutEffectWasObvious = obvious;
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:2497 	call *charaRefresh,(r1=tc)  ...
        }

        private void Apply_Oracle(EntityUid uid, EffectOracleComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:2501 	if tc>=maxFollower:txtNothingHappen:swbreak ...
            if (args.Handled)
                return;

            if (!_parties.IsInPlayerParty(args.Source))
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.Args.Ensure<EffectCommonArgs>().OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

            if (_curseStates.IsCursed(args.CommonArgs.CurseState))
            {
                _mes.Display(Loc.GetString("Elona.Effect.Oracle.Cursed"));
                args.Handle(TurnResult.Failed);
            }

            if (_oracle.ArtifactLocations.Count == 0)
            {
                _mes.Display(Loc.GetString("Elona.Effect.Oracle.NoArtifactsYet"));
            }
            else
            {
                foreach (var location in _oracle.ArtifactLocations)
                {
                    _mes.Display(_oracle.FormatArtifactLocation(location));
                }
            }

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:2516  ...
        }

        private void Apply_WallCreation(EntityUid uid, EffectWallCreationComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            var map = args.SourceMap;

            if (!_mapTilesets.TryGetTile(Protos.Tile.MapgenWall, map, out var tileID)
                || !map.IsInBounds(args.TargetCoordsMap)
                || !map.CanAccess(args.TargetCoordsMap)
                || map.GetTileID(args.TargetCoordsMap) == tileID)
            {
                // TODO combine nothing happens flags so message doesn't appear
                // more than once
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.Args.Ensure<EffectCommonArgs>().OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

            _mes.Display(Loc.GetString("Elona.Effect.Spell.WallCreation.WallAppears"), combineDuplicates: true);
            _audio.Play(Protos.Sound.Offer1, args.TargetCoordsMap);

            map.SetTile(args.TargetCoordsMap, tileID.Value);
            map.MemorizeTile(args.TargetCoordsMap);

            args.Handle(TurnResult.Succeeded);
        }
    }
}