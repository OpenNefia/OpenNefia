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
using OpenNefia.Core.Formulae;
using OpenNefia.Content.Enchantments;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Feats;
using OpenNefia.Core.Maths;
using OpenNefia.Content.UI;
using Color = OpenNefia.Core.Maths.Color;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Items.Impl;
using OpenNefia.Content.ChooseNPC;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.Fame;
using NetVips;
using OpenNefia.Content.Spells;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Equipment;
using OpenNefia.Core.SaveGames;
using System.Threading.Tasks.Sources;
using System.Runtime.InteropServices;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.Return;
using OpenNefia.Content.Nefia;
using OpenNefia.Content.Dungeons;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.Effects.New.Unique
{
    public sealed class VanillaSpellEffectsSystem : EntitySystem
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
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IWishSystem _wishes = default!;
        [Dependency] private readonly IFormulaEngine _formulaEngine = default!;
        [Dependency] private readonly IEnchantmentSystem _encs = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly ICommonEffectsSystem _commonEffects = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;
        [Dependency] private readonly IMapPlacement _mapPlacements = default!;
        [Dependency] private readonly IEmotionIconSystem _emotionIcons = default!;
        [Dependency] private readonly IDialogSystem _dialogs = default!;
        [Dependency] private readonly IKarmaSystem _karmas = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly ISpatialSystem _spatials = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IEquipmentSystem _equipment = default!;
        [Dependency] private readonly IPlayerQuery _playerQueries = default!;
        [Dependency] private readonly IReturnSystem _returning = default!;
        [Dependency] private readonly IMapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IAreaNefiaSystem _areaNefias = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectMutationComponent, ApplyEffectDamageEvent>(Apply_Mutation);
            SubscribeComponent<EffectCureMutationComponent, ApplyEffectDamageEvent>(Apply_CureMutation);
            SubscribeComponent<EffectDominateComponent, ApplyEffectDamageEvent>(Apply_Dominate);
            SubscribeComponent<EffectIdentifyComponent, ApplyEffectDamageEvent>(Apply_Identify);
            SubscribeComponent<EffectUncurseComponent, ApplyEffectDamageEvent>(Apply_Uncurse);
            SubscribeComponent<EffectOracleComponent, ApplyEffectDamageEvent>(Apply_Oracle);
            SubscribeComponent<EffectWallCreationComponent, ApplyEffectDamageEvent>(Apply_WallCreation);
            SubscribeComponent<EffectDoorCreationComponent, ApplyEffectDamageEvent>(Apply_DoorCreation);
            SubscribeComponent<EffectResurrectionComponent, ApplyEffectDamageEvent>(Apply_Resurrection);
            SubscribeComponent<EffectWizardsHarvestComponent, ApplyEffectDamageEvent>(Apply_WizardsHarvest);
            SubscribeComponent<EffectRestoreComponent, ApplyEffectDamageEvent>(Apply_Restore);
            SubscribeComponent<EffectWishComponent, ApplyEffectDamageEvent>(Apply_Wish);
            SubscribeComponent<EffectDamageTeleportComponent, ApplyEffectDamageEvent>(Apply_Teleport);
            SubscribeComponent<EffectGravityComponent, ApplyEffectDamageEvent>(Apply_Gravity);
            SubscribeComponent<EffectCurseComponent, ApplyEffectDamageEvent>(Apply_Curse);
            SubscribeComponent<EffectReturnComponent, ApplyEffectDamageEvent>(Apply_Return);
            SubscribeComponent<EffectEscapeComponent, ApplyEffectDamageEvent>(Apply_Escape);
            SubscribeEntity<AfterPlayerCastsReturnMagicEvent>(ReturnMagicCommon, priority: EventPriorities.VeryLow);
        }

        private void Apply_Mutation(EntityUid uid, EffectMutationComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.InnerTarget == null)
                return;

            var mutationTimes = args.OutDamage;
            var target = args.InnerTarget.Value;

            // >>>>>>>> elona122/shade2/proc.hsp:2365 	if encFind(tc,encResMutation)!falseM :if rnd(5):  ...
            if (_encs.HasEnchantmentEquipped<EncResistMutationComponent>(target) && !_rand.OneIn(5))
            {
                _mes.Display(Loc.GetString("Elona.Effect.Mutation.Resists", ("source", args.Source), ("target", target)), entity: target);
                args.Handle(TurnResult.Failed);
                return;
            }

            List<FeatPrototype> feats = GetMutationFeatsList();
            var didSomething = false;

            for (var i = 0; i < mutationTimes; i++)
            {
                for (var j = 0; j < 100; j++)
                {
                    var feat = _rand.Pick(feats);

                    var delta = 1;
                    if (_rand.OneIn(2))
                        delta = -1;

                    if (_feats.Level(target, feat) >= feat.LevelMax)
                        delta = -1;
                    if (_feats.Level(target, feat) <= feat.LevelMin)
                        delta = 1;

                    var proceed = true;

                    if (_curseStates.IsCursed(args.CommonArgs.CurseState))
                    {
                        if (delta > 0)
                            proceed = false;
                    }
                    else
                    {
                        if (delta < 0)
                        {
                            if ((args.CommonArgs.CurseState == CurseState.Blessed && _rand.OneIn(3)) || component.NoNegativeMutations)
                                proceed = false;
                        }
                    }

                    if (proceed)
                    {
                        _mes.Display(Loc.GetString("Elona.Effect.Mutation.Apply", ("source", args.Source), ("target", target), ("featID", (string)feat.GetStrongID())));
                        _feats.ModifyLevel(target, feat, delta);
                        var cb = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSmoke);
                        _mapDrawables.Enqueue(cb, target);
                        didSomething = true;
                        break;
                    }
                }
            }


            if (!didSomething)
            {
                return;
            }

            _refreshes.Refresh(target);
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:2395 	swbreak ...
        }

        private List<FeatPrototype> GetMutationFeatsList()
        {
            return _protos.EnumeratePrototypes<FeatPrototype>().Where(f => f.FeatType == FeatType.Mutation).ToList();
        }

        private void Apply_CureMutation(EntityUid uid, EffectCureMutationComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.InnerTarget == null)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:2398 	if tc!pc:txtNothingHappen:swbreak ...
            var times = args.OutDamage;
            if (args.CommonArgs.CurseState == CurseState.Normal)
                times += 1;
            else if (args.CommonArgs.CurseState == CurseState.Blessed)
                times += 2;

            var target = args.InnerTarget.Value;
            List<FeatPrototype> candidates = GetMutationFeatsList();
            var didSomething = false;

            for (var i = 0; i < times; i++)
            {
                for (var j = 0; j < 100; j++)
                {
                    var feat = _rand.Pick(candidates);
                    var level = _feats.Level(target, feat);

                    if (level != 0)
                    {
                        string? message;
                        Color color;
                        if (level < 0)
                        {
                            Loc.TryGetPrototypeString(feat, "OnGainLevel", out message, ("entity", target));
                            color = UiColors.MesGreen;
                        }
                        else
                        {

                            Loc.TryGetPrototypeString(feat, "OnLoseLevel", out message, ("entity", target));
                            color = UiColors.MesRed;
                        }

                        _feats.SetLevel(target, feat, 0);

                        _mes.Display(Loc.GetString("Elona.Effect.CureMutation.Message", ("source", args.Source), ("target", target), ("featID", (string)feat.GetStrongID())), entity: target);

                        if (message != null)
                            _mes.Display(message, color: color, entity: target); // TODO move messages into IFeatSystem

                        didSomething = true;
                        break;
                    }
                }

                if (!didSomething)
                {
                    return;
                }

                _refreshes.Refresh(target);
                args.Handle(TurnResult.Succeeded);
            }
            // <<<<<<<< elona122/shade2/proc.hsp:2423  ...
        }

        private void Apply_Dominate(EntityUid uid, EffectDominateComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:2915 	case spCharm ...
            if (args.Handled || args.InnerTarget is not EntityUid target)
                return;

            if (_parties.IsInSameParty(args.Source, target)
                || _parties.IsUnderlingOfSomeParty(args.Source)
                || _factions.GetRelationTowards(target, args.Source) >= Relation.Ally)
            {
                return;
            }

            if (!_commonEffects.CanCaptureMonstersIn(args.TargetMap))
            {
                _mes.Display(Loc.GetString("Elona.Effect.Domination.DoesNotWorkHere", ("source", args.Source)), entity: args.Source);
                args.Handle(TurnResult.Failed);
                return;
            }

            if (!_commonEffects.CanCaptureMonster(target))
            {
                _mes.Display(Loc.GetString("Elona.Effect.Domination.CannotBeCharmed", ("source", args.Source), ("target", target)), entity: args.Source);
                args.Handle(TurnResult.Failed);
                return;
            }

            // XXX: Can this be its own component?
            var hasMonsterHeart = _inv.EntityQueryInInventory<MonsterHeartComponent>(args.Source).Any();

            var power = args.OutDamage;
            if (hasMonsterHeart)
                power = (int)(power * 1.5);

            var success = power >= _levels.GetLevel(target);

            if (success)
            {
                _parties.TryRecruitAsAlly(args.Source, target);
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.Effect.Common.Resists", ("source", args.Source), ("target", target)), entity: args.Source);
            }

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:2931 	swbreak ...
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
                    var result = _newEffects.Apply(args.Source, args.InnerTarget, null, Protos.Effect.Curse, args.Args);
                    args.Handle(result);
                    return;
            }

            bool CanUncurse(CurseStateComponent curseState)
            {
                if (curseState.CurseState == CurseState.Blessed || curseState.CurseState == CurseState.Normal)
                    return false;

                // Only uncurse items in the inventory if the scroll is blessed.
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

            if (totalUncursed == 0 && totalResisted == 0)
            {
                return;
            }

            var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSparkle);
            _mapDrawables.Enqueue(anim, args.InnerTarget.Value);
            _refreshes.Refresh(args.InnerTarget.Value);
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:2497 	call *charaRefresh,(r1=tc)  ...
        }

        private void Apply_Oracle(EntityUid uid, EffectOracleComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:2501 	if tc>=maxFollower:txtNothingHappen:swbreak ...
            if (args.Handled)
                return;

            if (!_parties.IsInPlayerParty(args.Source))
                return;

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

        private void Apply_Resurrection(EntityUid uid, EffectResurrectionComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.AffectedTileIndex > 0)
                return;

            if (HasComp<MapTypeWorldMapComponent>(args.SourceMap.MapEntityUid))
                return;

            if (_curseStates.IsCursed(args.CommonArgs.CurseState))
            {
                _mes.Display(Loc.GetString("Elona.Effect.Resurrection.Cursed"));

                var undeadCount = 4 + _rand.Next(4);
                for (var i = 0; i < undeadCount; i++)
                {
                    var filter = new CharaFilter()
                    {
                        MinLevel = _randomGen.CalcObjectLevel(_levels.GetLevel(_gameSession.Player)),
                        Quality = _randomGen.CalcObjectQuality(Quality.Good),
                        Tags = new[] { Protos.Tag.CharaUndead }
                    };
                    _charaGen.GenerateChara(args.Source, filter);
                }

                args.Handle(TurnResult.Failed);
                args.OutEffectWasObvious = false;
                return;
            }

            bool IsCharaDead(EntityUid e)
            {
                return TryComp<CharaComponent>(e, out var c)
                    && (c.Liveness == CharaLivenessState.PetDead
                     || c.Liveness == CharaLivenessState.VillagerDead
                     || c.Liveness == CharaLivenessState.Dead);
            }

            EntityUid? target = args.InnerTarget;

            if (target == args.Source || target == null)
            {
                target = null;

                var candidates = _parties.EnumerateMembers(args.Source)
                    .Where(IsCharaDead)
                    .ToList();

                if (candidates.Count == 0)
                    return;

                if (_gameSession.IsPlayer(args.Source))
                {
                    var uiArgs = new ChooseNPCMenu.Args(candidates)
                    {
                        Prompt = Loc.GetString("Elona.Effect.Resurrection.Prompt"),
                    };
                    var result = _uiManager.Query<ChooseNPCMenu, ChooseNPCMenu.Args, ChooseNPCMenu.Result>(uiArgs);
                    if (result.HasValue)
                        target = result.Value.Selected;
                }
                else
                {
                    target = _rand.Pick(candidates);
                }
            }

            if (target == null || !IsCharaDead(target.Value))
                return;

            if (args.OutDamage < _rand.Next(100))
            {
                _mes.Display(Loc.GetString("Elona.Effect.Resurrection.Fail", ("source", args.Source), ("target", target.Value)));
                args.Handle(TurnResult.Failed);
            }

            _charas.Revive(target.Value);
            if (!_mapPlacements.TryPlaceChara(target.Value, args.SourceCoordsMap))
                return;

            _mes.Display(Loc.GetString("Elona.Effect.Resurrection.Apply", ("source", args.Source), ("target", target.Value)), color: UiColors.MesYellow);
            _mes.Display(Loc.GetString("Elona.Effect.Resurrection.Dialog", ("source", args.Source), ("target", target.Value)), color: UiColors.MesTalk, entity: target.Value);

            var targetPos = Spatial(target.Value).MapPosition;
            var positions = new List<MapCoordinates>() { targetPos };
            var anim = new MiracleMapDrawable(positions, Protos.Sound.Heal1, Protos.Sound.Pray2);
            _mapDrawables.Enqueue(anim, targetPos);

            _emotionIcons.SetEmotionIcon(target.Value, EmotionIcons.Heart, 3);

            if (_gameSession.IsPlayer(args.Source))
            {
                _dialogs.ModifyImpression(target.Value, 15);
                if (!_parties.IsInPlayerParty(target.Value))
                {
                    _karmas.ModifyKarma(args.Source, 2);
                }
            }

            args.Handle(TurnResult.Succeeded);
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

                _gameController.WaitSecs(0.1f);
                _field.RefreshScreen(); // TODO remove?
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

        private void Apply_Teleport(EntityUid uid, EffectDamageTeleportComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            var ent = component.Subject == TeleportSubject.Source ? args.Source : args.InnerTarget;

            EntityCoordinates coords;
            switch (component.Origin)
            {
                case TeleportOrigin.Source:
                default:
                    coords = args.SourceCoords;
                    break;
                case TeleportOrigin.Target:
                    coords = args.InnerTarget != null
                        ? Spatial(args.InnerTarget.Value).Coordinates
                        : args.TargetCoords;
                    break;
                case TeleportOrigin.TargetCoordinates:
                    coords = args.TargetCoords;
                    break;
            }

            if (!IsAlive(ent) || !_mapManager.TryGetMapOfEntity(ent.Value, out var map))
                return;

            var subject = ent.Value;

            if (!component.IgnoresPreventTeleport)
            {
                if (_encs.HasEnchantmentEquipped<EncPreventTeleportComponent>(subject))
                {
                    _mes.Display(Loc.GetString("Elona.Effect.Teleport.Prevented"));
                    args.Handle(TurnResult.Failed);
                    return;
                }

                if (EntityManager.TryGetComponent<MapCommonComponent>(map.MapEntityUid, out var mapCommon)
                    && mapCommon.PreventsTeleport)
                {
                    _mes.Display(Loc.GetString("Elona.Effect.Teleport.Prevented"));
                    args.Handle(TurnResult.Failed);
                    return;
                }
            }

            var spatial = EntityManager.GetComponent<SpatialComponent>(subject);

            EntitySystem.InjectDependencies(component.Position);

            for (var attempt = 0; attempt < component.MaxAttempts; attempt++)
            {
                var newCoords = component.Position.GetCoordinates(subject, args.Source, args.InnerTarget, map, coords, attempt);

                if (map.CanAccess(newCoords))
                {
                    _activities.RemoveActivity(subject);
                    spatial.Coordinates = newCoords;

                    if (_gameSession.IsPlayer(subject))
                    {
                        _field.RefreshScreen(); // ensure camera has correct position for spatial audio
                        _audio.Play(Protos.Sound.Teleport1, newCoords);
                        _mes.Display(Loc.GetString(component.MessageKey, ("chara", ent), ("source", args.Source), ("target", args.InnerTarget)));
                    }
                    else if (_vis.IsInWindowFov(coords))
                    {
                        _audio.Play(Protos.Sound.Teleport1, coords);
                        _mes.Display(Loc.GetString(component.MessageKey, ("chara", ent), ("source", args.Source), ("target", args.InnerTarget)));
                    }

                    args.Handle(TurnResult.Succeeded);
                    return;
                }
            }

            // "Nothing happens..." message appears after fallthrough.
        }

        private void Apply_Gravity(EntityUid uid, EffectGravityComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            bool CanBeAffected(CharaComponent chara)
            {
                var e = chara.Owner;
                var isImmune = TryComp<CommonProtectionsComponent>(e, out var prot) && prot.IsImmuneToMines.Buffed;
                var inRange = _spatials.TryMapDistanceTiled(args.Source, e, out var dist) && dist <= args.CommonArgs.TileRange * 2;
                return !isImmune && inRange;
            }

            var didSomething = false;
            foreach (var chara in _lookup.EntityQueryInMap<CharaComponent>(args.SourceMap).Where(CanBeAffected))
            {
                _mes.Display(Loc.GetString("Elona.Effect.Gravity.Apply", ("source", args.Source), ("target", chara.Owner)), entity: chara.Owner);
                _statusEffects.Apply(chara.Owner, Protos.StatusEffect.Gravity, 100 + _rand.Next(100));
                didSomething = true;
            }

            if (!didSomething)
                return;

            args.Handle(TurnResult.Failed);
        }

        private void Apply_Curse(EntityUid uid, EffectCurseComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:2832 	case actCurse ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            var target = args.InnerTarget.Value;

            var chance = args.OutDamage;
            if (_curseStates.IsCursed(args.CommonArgs.CurseState))
                chance *= 100;

            var resistance = 75 + _skills.Level(target, Protos.Skill.AttrLuck);
            var encPower = _encs.GetTotalEquippedEnchantmentPower<EncResistCurseComponent>(target);
            if (encPower > 0)
                resistance += (encPower / 2);

            if (_rand.Next(resistance) > chance)
                return;

            if (_feats.HasFeat(target, Protos.Feat.ResCurse) && _rand.OneIn(3))
            {
                _mes.Display(Loc.GetString("Elona.Effect.Curse.NoEffect", ("source", args.Source), ("target", target)), entity: target);
                args.Handle(TurnResult.Failed);
                return;
            }

            bool CanCurse(EntityUid item)
            {
                if (_curseStates.IsBlessed(item) && _rand.OneIn(10))
                    return false;

                return true;
            }

            var candidates = _equipSlots.EnumerateEquippedEntities(target)
                .Where(CanCurse)
                .ToList();

            if (candidates.Count == 0)
            {
                candidates = _inv.EnumerateInventory(target).Where(CanCurse).ToList();
            }

            if (candidates.Count == 0)
            {
                return;
            }

            var item = _rand.Pick(candidates);

            _mes.Display(Loc.GetString("Elona.Effect.Curse.Apply", ("source", args.Source), ("target", target), ("item", item)), entity: target);

            var curseState = EnsureComp<CurseStateComponent>(item);
            if (curseState.CurseState == CurseState.Cursed)
                curseState.CurseState = CurseState.Doomed;
            else
                curseState.CurseState = CurseState.Cursed;

            _refreshes.Refresh(target);
            _audio.Play(Protos.Sound.Curse3, target);

            var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimCurse);
            _mapDrawables.Enqueue(anim, target);
            _stacks.TryStackAtSamePos(item);

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:2888 	swbreak ...
        }


        private void Apply_Return(EntityUid uid, EffectReturnComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || !_gameSession.IsPlayer(args.Source))
                return;

            if (_returning.ReturnState != null)
            {
                _mes.Display(Loc.GetString("Elona.Return.Cancel", ("source", args.Source)));
                _returning.CancelReturn();
                args.Handle(TurnResult.Succeeded);
                return;
            }

            if (!_returning.CheckReturnCapability(out var message))
            {
                if (message != null)
                    _mes.Display(message);

                args.Handle(TurnResult.Failed);
                return;
            }

            if (!_returning.TryPromptReturnLocation(args.SourceMap, out var entrance))
            {
                args.Handle(TurnResult.Failed);
                return;
            }

            // >>>>>>>> elona122/shade2/command.hsp:4416 		txt lang("周囲の大気がざわめきだした。","The air around you be ...
            var turnsUntilCast = args.OutDamage;
            _returning.StartReturn(entrance, turnsUntilCast);
            _mes.Display(Loc.GetString("Elona.Return.Begin", ("source", args.Source)));
            // <<<<<<<< elona122/shade2/command.hsp:4419 		gReturn=15+rnd(15) ...

            var ev = new AfterPlayerCastsReturnMagicEvent(args.SourceMap, args.Args);
            RaiseEvent(args.Source, ev);

            args.Handle(TurnResult.Succeeded);
        }

        private void Apply_Escape(EntityUid uid, EffectEscapeComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || !_gameSession.IsPlayer(args.Source))
                return;

            if (_returning.ReturnState != null)
            {
                _mes.Display(Loc.GetString("Elona.Return.Cancel", ("source", args.Source)));
                _returning.CancelReturn();
                args.Handle(TurnResult.Succeeded);
                return;
            }

            if (!_returning.CheckReturnCapability(out var message))
            {
                if (message != null)
                {
                    _mes.Display(message);
                    args.Handle(TurnResult.Failed);
                }
                return;
            }

            if (!_returning.TryGetEscapeLocation(args.SourceMap, out var coords))
            {
                Logger.ErrorS("return", $"Could not find escape location for map {args.SourceMap!}");
                return;
            }

            // >>>>>>>> elona122/shade2/proc.hsp:2771 		txt lang("周囲の大気がざわめきだした。","The air around you be ...
            var entrance = MapEntrance.FromMapCoordinates(coords.Value);
            var turnsUntilCast = args.OutDamage;
            _returning.StartReturn(entrance, turnsUntilCast);
            _mes.Display(Loc.GetString("Elona.Return.Begin", ("source", args.Source)));
            // <<<<<<<< elona122/shade2/proc.hsp:2775 		gReturn=5+rnd(10) ...

            var ev = new AfterPlayerCastsReturnMagicEvent(args.SourceMap, args.Args);
            RaiseEvent(args.Source, ev);

            args.Handle(TurnResult.Succeeded);
        }

        private void ReturnMagicCommon(EntityUid caster, AfterPlayerCastsReturnMagicEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:2772 		if areaId(gArea)=areaRandDungeon : if gLevel=are ...
            if (_areaNefias.IsNefiaBossActive(args.SourceMap))
            {
                _mes.Display(Loc.GetString("Elona.Nefia.BossMayDisappear"));
            }
            // <<<<<<<< elona122/shade2/proc.hsp:2772 		if areaId(gArea)=areaRandDungeon : if gLevel=are ...

            // >>>>>>>> elona122/shade2/proc.hsp:2786 		if efStatus<=stCursed:if rnd(3)=0:gTeleportArea= ...
            if (_curseStates.IsCursed(args.CommonArgs.CurseState) && _rand.OneIn(3))
            {
                _mes.Display("TODO jail", UiColors.MesYellow);
            }
            // <<<<<<<< elona122/shade2/proc.hsp:2786 		if efStatus<=stCursed:if rnd(3)=0:gTeleportArea= ...

            // >>>>>>>> elona122/shade2/main.hsp:737 			if (mType=mTypeQuest)or(gArea=areaShelter)or(gA ...
            if (HasComp<MapTypeQuestComponent>(args.SourceMap.MapEntityUid)
                || HasComp<MapTypeShelterComponent>(args.SourceMap.MapEntityUid))
            // TODO jail
            {
                _mes.Display(Loc.GetString("Elona.Return.Prevented", ("source", caster)));
                _returning.CancelReturn();
            }
            // <<<<<<<< elona122/shade2/main.hsp:740 				} ...
        }
    }

    public enum PreventReturnSeverity
    {
        /// <summary>
        /// The player will cast return without being prompted.
        /// </summary>
        Proceed = 0,

        /// <summary>
        /// The player will be prompted for confirmation before casting return.
        /// </summary>
        PromptYesNo = 1,

        /// <summary>
        /// The player will not be able to cast return and the effect will fail.
        /// </summary>
        Fail = 2
    }

    [DataDefinition]
    public record class PreventReturnReason
    {
        public PreventReturnReason()
        {
        }

        public PreventReturnReason(string message, PreventReturnSeverity severity)
        {
            Message = message;
            Severity = severity;
        }

        [DataField]
        public string Message { get; set; } = "Unknown reason";

        [DataField]
        public PreventReturnSeverity Severity { get; set; } = PreventReturnSeverity.Proceed;
    }

    /// <summary>
    /// Raised before the player casts magic that leaves the map.
    /// Allows systems to warn the player before the map is left.
    /// </summary>
    public sealed class BeforePlayerCastsReturnMagicEvent : EntityEventArgs
    {
        public BeforePlayerCastsReturnMagicEvent(IMap sourceMap)
        {
            SourceMap = sourceMap;
        }

        public IMap SourceMap { get; }

        /// <summary>
        /// Reasons the return should fail.
        /// If empty, the return will succeed.
        /// </summary>
        public IList<PreventReturnReason> OutWarningReasons { get; } = new List<PreventReturnReason>();
    }

    public sealed class AfterPlayerCastsReturnMagicEvent : EntityEventArgs
    {
        public AfterPlayerCastsReturnMagicEvent(IMap sourceMap, EffectArgSet args)
        {
            SourceMap = sourceMap;
            Args = args;
        }

        public IMap SourceMap { get; }
        public EffectArgSet Args { get; }
        public EffectCommonArgs CommonArgs => Args.Ensure<EffectCommonArgs>();
    }
}