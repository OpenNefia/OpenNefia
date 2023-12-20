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
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.GameController;
using OpenNefia.Content.GameController;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Wishes;

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
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly IFieldLayer _fieldLayers = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IWishSystem _wishes = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectIdentifyComponent, ApplyEffectDamageEvent>(Apply_Identify);
            SubscribeComponent<EffectUncurseComponent, ApplyEffectDamageEvent>(Apply_Uncurse);
            SubscribeComponent<EffectOracleComponent, ApplyEffectDamageEvent>(Apply_Oracle);
            SubscribeComponent<EffectWallCreationComponent, ApplyEffectDamageEvent>(Apply_WallCreation);
            SubscribeComponent<EffectDoorCreationComponent, ApplyEffectDamageEvent>(Apply_DoorCreation);
            SubscribeComponent<EffectWizardsHarvestComponent, ApplyEffectDamageEvent>(Apply_WizardsHarvest);
            SubscribeComponent<EffectRestoreComponent, ApplyEffectDamageEvent>(Apply_Restore);
            SubscribeComponent<EffectWishComponent, ApplyEffectDamageEvent>(Apply_Wish);
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

            // >>>>>>>> elona122/shade2/proc.hsp:3257 			if homeMapMode=false{ ...
            _mes.Display(Loc.GetString("Elona.Effect.WallCreation.WallAppears"), combineDuplicates: true);
            _audio.Play(Protos.Sound.Offer1, args.TargetCoordsMap);

            map.SetTile(args.TargetCoordsMap, tileID.Value);
            map.MemorizeTile(args.TargetCoordsMap);
            // <<<<<<<< elona122/shade2/proc.hsp:3265 			map(x,y,2)=p ...

            args.Handle(TurnResult.Succeeded);
        }

        private void Apply_DoorCreation(EntityUid uid, EffectDoorCreationComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            var map = args.SourceMap;

            if (!map.IsInBounds(args.TargetCoordsMap))
            {
                // TODO combine nothing happens flags so message doesn't appear
                // more than once
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                args.Args.Ensure<EffectCommonArgs>().OutEffectWasObvious = false;
                args.Handle(TurnResult.Failed);
                return;
            }

            var tile = map.GetTilePrototype(args.TargetCoordsMap)!;

            if (tile.IsSolid)
            {
                if (_mapTilesets.TryGetTile(Protos.Tile.MapgenTunnel, map, out var newTile))
                {
                    map.SetTile(args.TargetCoordsMap, newTile.Value);
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                    args.Args.Ensure<EffectCommonArgs>().OutEffectWasObvious = false;
                    args.Handle(TurnResult.Failed);
                    return;
                }
            }

            // >>>>>>>> elona122/shade2/proc.hsp:3269 			snd seOffer ...
            _audio.Play(Protos.Sound.Offer1, args.TargetCoordsMap);
            if (tile.Kind == TileKind.HardWall || tile.Kind2 == TileKind.HardWall)
            {
                _mes.Display(Loc.GetString("Elona.Effect.DoorCreation.WallsResist"), combineDuplicates: true);
                args.Handle(TurnResult.Failed);
                return;
            }

            _mes.Display(Loc.GetString("Elona.Effect.DoorCreation.DoorAppears"), combineDuplicates: true);

            // TODO determine door theme.
            var door = _entityGen.SpawnEntity(Protos.MObj.DoorWooden, args.TargetCoordsMap);
            if (IsAlive(door))
                EnsureComp<DoorComponent>(door.Value).UnlockDifficulty = int.Max(args.OutDamage, 0);

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:3273 			if tAttb(map(x,y,0))&cantPass:map(x,y,0)=tile_t ...
        }

        private void Apply_WizardsHarvest(EntityUid uid, EffectWizardsHarvestComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:3445 	case spHarvest ...
            var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSparkle);
            _mapDrawables.Enqueue(anim, args.TargetCoordsMap);

            var itemCount = int.Clamp(4 + _rand.Next(args.OutDamage / 50 + 1), 1, 15);

            for (var i = 0; i < itemCount; i++)
            {
                _audio.Play(Protos.Sound.Pray1, args.TargetCoordsMap);

                var itemID = Protos.Item.GoldPiece;
                var amount = 400 + _rand.Next(args.OutDamage);

                if (_rand.OneIn(30))
                {
                    itemID = Protos.Item.PlatinumCoin;
                    amount = 1;
                }
                if (_rand.OneIn(80))
                {
                    itemID = Protos.Item.SmallMedal;
                    amount = 1;
                }
                if (_rand.OneIn(2000))
                {
                    itemID = Protos.Item.RodOfWishing;
                    amount = 1;
                }

                var filter = new ItemFilter()
                {
                    Amount = amount,
                    Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Good),
                    MinLevel = _randomGen.CalcObjectLevel(args.OutDamage / 10),
                    Id = itemID,
                    Args = EntityGenArgSet.Make(new EntityGenCommonArgs()
                    {
                        NoStack = true
                    })
                };
                var item = _itemGen.GenerateItem(args.TargetCoordsMap, filter);
                if (IsAlive(item))
                {
                    _mes.Display(Loc.GetString("Elona.Effect.WizardsHarvest.FallsDown", ("source", args.Source), ("item", item.Value)), entity: args.Source);
                }

                _gameController.Wait(100);
                _fieldLayers.RefreshScreen(); // TODO remove?
            }

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:3457 	loop ...
        }

        private void Apply_Restore(EntityUid uid, EffectRestoreComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.InnerTarget == null)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:2728 	if efId=spRestoreBody{ ...
            if (_curseStates.IsCursed(args.CommonArgs.CurseState))
            {
                _audio.Play(Protos.Sound.Curse3, args.TargetCoordsMap);
                _mes.Display(Loc.GetString(component.MessageKey.With("Cursed"), ("source", args.Source), ("target", args.InnerTarget)));
            }
            else
            {
                _mes.Display(Loc.GetString(component.MessageKey.With("Apply"), ("source", args.Source), ("target", args.InnerTarget)));
                var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSparkle);
                _mapDrawables.Enqueue(anim, args.TargetCoordsMap);

                if (_curseStates.IsBlessed(args.CommonArgs.CurseState))
                {
                    _mes.Display(Loc.GetString(component.MessageKey.With("Blessed"), ("source", args.Source), ("target", args.InnerTarget)));
                    anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSparkle);
                    _mapDrawables.Enqueue(anim, args.TargetCoordsMap);
                }
            }

            var targetQuality = CompOrNull<QualityComponent>(args.InnerTarget.Value)?.Quality?.Buffed ?? Quality.Normal;

            foreach (var skillID in component.SkillsToRestore)
            {
                var adj = _skills.LevelAdjustment(args.InnerTarget.Value, skillID);
                if (_curseStates.IsCursed(args.CommonArgs.CurseState))
                {
                    if (targetQuality <= Quality.Normal)
                    {
                        adj -= _rand.Next(_skills.BaseLevel(args.InnerTarget.Value, skillID)) / 5 + _rand.Next(5);
                    }
                }
                else
                {
                    adj = int.Max(adj, 0);
                    if (args.CommonArgs.CurseState == CurseState.Blessed)
                        adj = _skills.BaseLevel(args.InnerTarget.Value, skillID) / 10 + 5;
                }

                _skills.SetLevelAdjustment(args.InnerTarget.Value, skillID, adj);
            }

            _refreshes.Refresh(args.InnerTarget.Value);
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:2741 	} ...
        }

        private void Apply_Wish(EntityUid uid, EffectWishComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            _wishes.PromptForWish(_gameSession.Player);

            args.Handle(TurnResult.Succeeded);
        }
    }
}