using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Weather;
using OpenNefia.Core;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.Money;
using System.Runtime.InteropServices;
using OpenNefia.Content.Roles;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.Currency;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Loot;
using OpenNefia.Content.Items;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Quests;
using OpenNefia.Core.Log;
using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Encounters
{
    public sealed class VanillaEncountersSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IEncounterSystem _encounters = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IWeatherSystem _weathers = default!;
        [Dependency] private readonly IPlayerQuery _playerQueries = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAIs = default!;
        [Dependency] private readonly IMoneySystem _money = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;
        [Dependency] private readonly IDialogSystem _dialogs = default!;
        [Dependency] private readonly IKarmaSystem _karmas = default!;
        [Dependency] private readonly IMapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IQuestSystem _quests = default!;
        [Dependency] private readonly IMapImmediateQuestSystem _mapImmediateQuests = default!;
        [Dependency] private readonly IFameSystem _fames = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IShopkeeperSystem _shopkeepers = default!;
        [Dependency] private readonly IQuestEliminateTargetSystem _questElimTargets = default!;
        [Dependency] private readonly IMusicManager _music = default!;

        public override void Initialize()
        {
            SubscribeComponent<EncounterEnemyComponent, EncounterCalcLevelEvent>(Enemy_CalcLevel);
            SubscribeComponent<EncounterEnemyComponent, EncounterBeforeMapEnteredEvent>(Enemy_BeforeStart);
            SubscribeComponent<EncounterEnemyComponent, EncounterAfterMapEnteredEvent>(Enemy_OnMapEntered);

            SubscribeComponent<EncounterMerchantComponent, EncounterCalcLevelEvent>(Merchant_CalcLevel);
            SubscribeComponent<EncounterMerchantComponent, EncounterAfterMapEnteredEvent>(Merchant_OnMapEntered);
            SubscribeComponent<WanderingMerchantComponent, GetDefaultDialogChoicesEvent>(WanderingMerchant_AddDialogChoices);
            SubscribeComponent<WanderingMerchantComponent, AfterDialogEndedEvent>(WanderingMerchant_ExitMapAfterDialog);
            SubscribeComponent<WanderingMerchantComponent, OnGenerateLootDropsEvent>(WanderingMerchant_GenerateLootDrops);

            SubscribeComponent<MapEncounterComponent, BeforeExitMapFromEdgesEventArgs>(Assassin_CheckActiveQuest);
            SubscribeComponent<MapEncounterComponent, MapQuestTargetKilledEvent>(Assassin_TargetKilled);
            SubscribeComponent<MapEncounterComponent, MapQuestTargetsEliminatedEvent>(Assassin_TargetsEliminated);
            SubscribeComponent<MapEncounterComponent, BeforeMapLeaveEventArgs>(Assassin_BeforeMapLeave);
            SubscribeComponent<EncounterAssassinComponent, EncounterCalcLevelEvent>(Assassin_CalcLevel);
            SubscribeComponent<EncounterAssassinComponent, EncounterBeforeMapEnteredEvent>(Assassin_BeforeStart);
            SubscribeComponent<EncounterAssassinComponent, EncounterAfterMapEnteredEvent>(Assassin_OnMapEntered);

            SubscribeComponent<EncounterRogueComponent, EncounterCalcLevelEvent>(Rogue_CalcLevel);
            SubscribeComponent<EncounterRogueComponent, EncounterAfterMapEnteredEvent>(Rogue_OnMapEntered);
        }

        #region Elona.Enemy

        private void Enemy_CalcLevel(EntityUid uid, EncounterEnemyComponent comp, EncounterCalcLevelEvent args)
        {
            // >>>>>>>> shade2/action.hsp:690 			p=dist_town() ...
            var townDist = _encounters.DistanceFromNearestTown(args.OuterMapCoords);
            var level = townDist * 3 / 2 - 10;
            if (_levels.GetLevel(_gameSession.Player) <= 5)
                level /= 2;

            var encounter = EnsureComp<EncounterComponent>(uid);
            var tile = encounter.StoodWorldMapTile;
            if (ProtoSets.Tile.WorldMapRoadTiles.Contains(tile))
                level /= 2;
            else if (_weathers.IsWeatherActive(Protos.Weather.Etherwind))
                level = level * 3 / 2 + 10;

            args.OutLevel = level;
            // <<<<<<<< shade2/action.hsp:698 			if encounterLv<0:encounterLv=1 ..
        }

        private void Enemy_BeforeStart(EntityUid uid, EncounterEnemyComponent comp, EncounterBeforeMapEnteredEvent args)
        {
            // >>>>>>>> shade2/action.hsp:690 			p=dist_town() ...
            var townDist = _encounters.DistanceFromNearestTown(args.OuterMapCoords);
            // <<<<<<<< shade2/action.hsp:690 			p=dist_town() ..

            // >>>>>>>> shade2/action.hsp:699 			valn=lang(" (ç≈Ç‡ãﬂÇ¢äXÇ‹Ç≈ÇÃãóó£:"+p+" ìGê®óÕ:"," (Distance ...
            var encounter = EnsureComp<EncounterComponent>(uid);
            LocaleKey rank = new LocaleKey("Elona.Encounter.Types.Enemy.Rank");
            if (encounter.Level < 5)
                rank = rank.With("Putit");
            else if (encounter.Level < 10)
                rank = rank.With("Orc");
            else if (encounter.Level < 20)
                rank = rank.With("GrizzlyBear");
            else if (encounter.Level < 30)
                rank = rank.With("Drake");
            else if (encounter.Level < 40)
                rank = rank.With("Lich");
            else
                rank = rank.With("Dragon");

            _mes.Display(Loc.GetString("Elona.Encounter.Types.Enemy.Message", ("distNearestTown", townDist), ("rank", Loc.GetString(rank))));
            _playerQueries.PromptMore();
            // <<<<<<<< shade2/action.hsp:711 			txt lang("èPåÇÇæÅI","Ambush!")+valn:msg_halt ..
        }

        private void Enemy_OnMapEntered(EntityUid uid, EncounterEnemyComponent comp, EncounterAfterMapEnteredEvent args)
        {
            // >>>>>>>> shade2/map.hsp:1640 			p=rnd(9):if cLevel(pc)<=5:p=rnd(3) ...
            var enemyCount = _rand.Next(9);
            if (_levels.GetLevel(_gameSession.Player) <= 5)
                enemyCount = _rand.Next(3);

            var encounter = EnsureComp<EncounterComponent>(uid);
            var tile = encounter.StoodWorldMapTile;
            var isRoad = ProtoSets.Tile.WorldMapRoadTiles.Contains(tile);

            for (var i = 0; i < enemyCount + 1; i++)
            {
                var filter = new CharaFilter()
                {
                    MinLevel = _randomGen.CalcObjectLevel(encounter.Level),
                    Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Normal)
                };

                if (_weathers.IsWeatherActive(Protos.Weather.Etherwind) && !isRoad)
                {
                    if (_rand.OneIn(3))
                        filter.Quality = Qualities.Quality.God;
                }

                EntityUid? chara;
                if (i < 4)
                    chara = _charaGen.GenerateChara(_gameSession.Player, filter);
                else
                    chara = _charaGen.GenerateChara(args.EncounterMap, filter);

                if (IsAlive(chara))
                {
                    _vanillaAIs.SetTarget(chara.Value, _gameSession.Player, aggro: 30);
                    if (TryComp<FactionComponent>(chara.Value, out var faction))
                        faction.RelationToPlayer = Relation.Enemy;
                }
            }
            // <<<<<<<< shade2/map.hsp:1647 			loop ..
        }

        #endregion

        #region Elona.Merchant

        private void Merchant_CalcLevel(EntityUid uid, EncounterMerchantComponent comp, EncounterCalcLevelEvent args)
        {
            //  >>>>>>>> shade2/action.hsp:685 			encounterLv=10+rnd(100) ...
            args.OutLevel = 10 + _rand.Next(100);
            // <<<<<<<< shade2/action.hsp:685 			encounterLv=10+rnd(100) ..
        }

        private void Merchant_OnMapEntered(EntityUid uid, EncounterMerchantComponent comp, EncounterAfterMapEnteredEvent args)
        {
            // >>>>>>>> shade2/map.hsp:1623 			flt  ...
            var merchant = _charaGen.GenerateChara(args.EncounterMap.AtPos(10, 11), Protos.Chara.Shopkeeper);
            if (!IsAlive(merchant))
                return;

            var encounter = EnsureComp<EncounterComponent>(uid);
            var merchantComp = EnsureComp<RoleShopkeeperComponent>(merchant.Value);
            merchantComp.ShopRank = encounter.Level;
            merchantComp.ShopInventoryId = Protos.ShopInventory.WanderingMerchant;
            EnsureComp<WanderingMerchantComponent>(merchant.Value);
            _money.TryGenerateExtraGoldForChara(merchant.Value);
            for (var i = 1; i < encounter.Level / 2; i++)
            {
                _levels.GainLevel(merchant.Value, showMessage: false);
            }

            _shopkeepers.RestockShop(merchant.Value);

            var guardCount = 6 + _rand.Next(6);
            for (var i = 0; i < guardCount; i++)
            {
                var filter = new CharaFilter()
                {
                    MinLevel = 20, // base level of all CharaShopguard tagged entities in vanilla
                    LevelOverride = encounter.Level + _rand.Next(6),
                    Tags = new[] { Protos.Tag.CharaShopguard }
                };
                var guard = _charaGen.GenerateChara(args.EncounterMap.AtPos(14, 11), filter);
                if (IsAlive(guard))
                {
                    EnsureComp<RoleShopGuardComponent>(guard.Value); // Prevents incognito from having an effect
                    EnsureComp<LevelComponent>(guard.Value).ShowLevelInName = true;
                }
            }

            _deferredEvents.Enqueue(() =>
            {
                if (IsAlive(merchant.Value))
                {
                    _dialogs.TryToChatWith(_gameSession.Player, merchant.Value);
                }
                return TurnResult.Aborted;
            });
            // <<<<<<<< shade2/map.hsp:1636 	 ..
        }

        private void WanderingMerchant_AddDialogChoices(EntityUid uid, WanderingMerchantComponent component, GetDefaultDialogChoicesEvent args)
        {
            // >>>>>>>> shade2/chat.hsp:2217 		if cRole(tc)=cRoleShopWander:chatList 31,lang("èP ...
            args.OutChoices.Add(new()
            {
                Text = DialogTextEntry.FromLocaleKey("Elona.Dialog.Shopkeeper.Choices.Attack"),
                NextNode = new(Protos.Dialog.WanderingMerchant, "AttackConfirm")
            });
            // <<<<<<<< shade2/chat.hsp:2217 		if cRole(tc)=cRoleShopWander:chatList 31,lang("èP ..
        }

        public QualifiedDialogNode? WanderingMerchant_Attack(IDialogEngine engine, IDialogNode node)
        {
            // >>>>>>>> shade2/chat.hsp:2590 		goHostile ...
            _karmas.TurnGuardsHostile(_gameSession.Player);
            return null;
            // <<<<<<<< shade2/chat.hsp:2591 		goto *chat_end ..
        }

        private void WanderingMerchant_ExitMapAfterDialog(EntityUid uid, WanderingMerchantComponent component, AfterDialogEndedEvent args)
        {
            if (args.NodeEndedOn.DialogID == Protos.Dialog.Default)
            {
                var map = GetMap(uid);
                if (TryComp<MapEncounterComponent>(map.MapEntityUid, out var mapEncounter) 
                    && IsAlive(mapEncounter.EncounterContainer.ContainedEntity)
                    && TryComp<EncounterComponent>(mapEncounter.EncounterContainer.ContainedEntity, out var encounter))
                {
                    _mapEntrances.UseMapEntrance(_gameSession.Player, encounter.PreviousLocation);
                }
            }
        }

        private void WanderingMerchant_GenerateLootDrops(EntityUid uid, WanderingMerchantComponent component, OnGenerateLootDropsEvent args)
        {
            // >>>>>>>> elona122/shade2/item.hsp:298 	if cRole(rc)=cRoleShopWander{ ...
            var item = _itemGen.GenerateItem(uid, Protos.Item.ShopkeepersTrunk);
            if (IsAlive(item))
            {
                EnsureComp<PickableComponent>(item.Value).OwnState = OwnState.Shop;
                if (TryComp<TrunkComponent>(item.Value, out var trunk) && TryComp<RoleShopkeeperComponent>(uid, out var shopkeeper))
                {
                    foreach (var shopItem in shopkeeper.ShopContainer.ContainedEntities.ToList())
                        trunk.Container.Insert(shopItem);
                }
            }
            // <<<<<<<< elona122/shade2/item.hsp:301 		} ...
        }

        #endregion

        #region Elona.Assassin

        private const string EncounterAssassinEnemyTag = "Elona.Assassin";

        private void Assassin_CheckActiveQuest(EntityUid uid, MapEncounterComponent comp, BeforeExitMapFromEdgesEventArgs args)
        {
            // >>>>>>>> elona122/shade2/action.hsp:583 		if mType=mTypeQuest:if gQuestStatus!qSuccess:txt ...
            if (_encounters.TryGetEncounter<EncounterAssassinComponent>(args.Map, out var encounterAssassin)
                && !encounterAssassin.AllEnemiesDefeated)
                _mes.Display(Loc.GetString("Elona.Quest.AboutToAbandon"));
            // <<<<<<<< elona122/shade2/action.hsp:583 		if mType=mTypeQuest:if gQuestStatus!qSuccess:txt ...
        }

        private void Assassin_TargetKilled(EntityUid uid, MapEncounterComponent component, MapQuestTargetKilledEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:192 			if p=0:	evAdd evQuestEliminate :else: txtMore:t ...
            if (args.Tag != EncounterAssassinEnemyTag)
                return;

            if (args.TargetsRemaining > 0)
                _mes.Display(Loc.GetString("Elona.Quest.Eliminate.TargetsRemaining", ("count", args.TargetsRemaining)), color: UiColors.MesBlue);
            // <<<<<<<< elona122/shade2/chara_func.hsp:192 			if p=0:	evAdd evQuestEliminate :else: txtMore:t ...
        }

        private void Assassin_TargetsEliminated(EntityUid uid, MapEncounterComponent component, MapQuestTargetsEliminatedEvent args)
        {
            if (args.Tag != EncounterAssassinEnemyTag || !TryMap(uid, out var map))
                return;

            // >>>>>>>> elona122/shade2/quest.hsp:420 	call *music_play,(music=mcFanfare,musicLoop=1) ...
            _music.Play(Protos.Music.Fanfare, loop: false);
            if (_encounters.TryGetEncounter<EncounterAssassinComponent>(map, out var encounterAssassin))
                encounterAssassin.AllEnemiesDefeated = true;
            _mes.Display(Loc.GetString("Elona.Quest.Eliminate.Complete"), color: UiColors.MesGreen);
            // <<<<<<<< elona122/shade2/quest.hsp:421 	gQuestStatus=qSuccess ...
        }

        private void Assassin_BeforeMapLeave(EntityUid uid, MapEncounterComponent component, BeforeMapLeaveEventArgs args)
        {
            // >>>>>>>> elona122/shade2/quest.hsp:322 	if gQuestStatus!qSuccess{ ...
            if (_encounters.TryGetEncounter<EncounterAssassinComponent>(args.OldMap, out var encounterAssassin)
                && !encounterAssassin.AllEnemiesDefeated)
            {
                _mes.Display(Loc.GetString("Elona.Quest.LeftYourClient"));
                _quests.FailQuest(encounterAssassin.EscortQuestUid);
                _playerQueries.PromptMore();
            }
            // <<<<<<<< elona122/shade2/quest.hsp:327 		} ...
        }

        private void Assassin_CalcLevel(EntityUid uid, EncounterAssassinComponent comp, EncounterCalcLevelEvent args)
        {
            // >>>>>>>> shade2/map.hsp:1615 			flt qLevel(rq),fixGood ...
            var quests = _quests.EnumerateAcceptedQuests<QuestTypeEscortComponent>()
                .Where(q => q.QuestType.EscortType == EscortType.Protect && q.QuestType.EncountersSeen < 2)
                .ToList();

            if (quests.Count == 0)
            {
                Logger.ErrorS("encounters.vanilla", "No active escort quests!");
                return;
            }

            var quest = _rand.Pick(quests);
            quest.QuestType.EncountersSeen++;
            comp.EscortQuestUid = quest.Quest.Owner;

            args.OutLevel = quest.Quest.Difficulty;
            // <<<<<<<< shade2/map.hsp:1615 			flt qLevel(rq),fixGood ..
        }

        private void Assassin_BeforeStart(EntityUid uid, EncounterAssassinComponent comp, EncounterBeforeMapEnteredEvent args)
        {
            // >>>>>>>> shade2/action.hsp:678 			txt lang("à√éEé“Ç…Ç¬Ç©Ç‹Ç¡ÇΩÅBÇ†Ç»ÇΩÇÕÉNÉâÉCÉAÉìÉgÇéÁÇÁÇ»ÇØÇÍÇŒÇ»ÇÁÇ»Ç¢ÅB","Yo ...
            _mes.Display(Loc.GetString("Elona.Quest.Types.Escort.CaughtByAssassins"));
            comp.AllEnemiesDefeated = false;
            _playerQueries.PromptMore();
            // <<<<<<<< shade2/action.hsp:679 			msg_halt ..
        }

        private void Assassin_OnMapEntered(EntityUid uid, EncounterAssassinComponent comp, EncounterAfterMapEnteredEvent args)
        {
            // >>>>>>>> shade2/map.hsp:1608 		if encounter=encounterAssassin{ ...
            if (!IsAlive(comp.EscortQuestUid) || !TryComp<QuestComponent>(comp.EscortQuestUid, out var quest))
            {
                Logger.ErrorS("encounters.vanilla", "No active escort quest!");
                return;
            }

            var encounter = EnsureComp<EncounterComponent>(uid);
            quest.State = QuestState.Accepted;
            _mapImmediateQuests.SetImmediateQuest(args.EncounterMap, quest, encounter.PreviousLocation);

            EnsureComp<MapCharaGenComponent>(args.EncounterMap.MapEntityUid).MaxCharaCount = 0;
            EnsureComp<MapTypeQuestComponent>(args.EncounterMap.MapEntityUid);

            var charaCount = _rand.Next(3) + 5;
            for (var i = 0; i < charaCount; i++)
            {
                var filter = new CharaFilter()
                {
                    MinLevel = encounter.Level,
                    Quality = Qualities.Quality.Good
                };

                var assassin = _charaGen.GenerateChara(_gameSession.Player, filter);
                if (IsAlive(assassin))
                {
                    _vanillaAIs.SetTarget(assassin.Value, _gameSession.Player, 30);
                    EnsureComp<FactionComponent>(assassin.Value).RelationToPlayer = Relation.Enemy;
                    EnsureComp<QuestEliminateTargetComponent>(assassin.Value).Tag = EncounterAssassinEnemyTag;
                }
            }

            comp.AllEnemiesDefeated = _questElimTargets.AllTargetsEliminated(args.EncounterMap, EncounterAssassinEnemyTag);
            // <<<<<<<< shade2/map.hsp:1620 			} ...   end
        }

        #endregion

        #region Elona.Rogue

        private void Rogue_CalcLevel(EntityUid uid, EncounterRogueComponent comp, EncounterCalcLevelEvent args)
        {
            // >>>>>>>> shade2/action.hsp:673 			encounterLv=cFame(pc)/1000 ...
            args.OutLevel = _fames.GetFame(_gameSession.Player) / 1000;
            // <<<<<<<< shade2/action.hsp:673 			encounterLv=cFame(pc)/1000 ..
        }

        private void Rogue_OnMapEntered(EntityUid uid, EncounterRogueComponent comp, EncounterAfterMapEnteredEvent args)
        {
            EnsureComp<MapCharaGenComponent>(args.EncounterMap.MapEntityUid).MaxCharaCount = 0;

            var encounter = EnsureComp<EncounterComponent>(uid);
            var rogueBoss = _charaGen.GenerateChara(_gameSession.Player, Protos.Chara.RogueBoss, args: EntityGenArgSet.Make(new EntityGenCommonArgs()
            {
                LevelOverride = encounter.Level
            }));

            var rogueCount = 6 + _rand.Next(6);
            for (var i = 0; i < rogueCount; i++)
            {
                var filter = new CharaFilter()
                {
                    LevelOverride = encounter.Level,
                    Tags = new[] { Protos.Tag.CharaRogueGuard }
                };
                var rogue = _charaGen.GenerateChara(args.EncounterMap.AtPos(14, 11), filter);
                if (IsAlive(rogue))
                {
                    EnsureComp<LevelComponent>(rogue.Value).ShowLevelInName = true;
                }
            }

            _deferredEvents.Enqueue(() =>
            {
                if (IsAlive(rogueBoss))
                    _dialogs.TryToChatWith(_gameSession.Player, rogueBoss.Value);
                return TurnResult.Aborted;
            });
        }

        #endregion
    }

    /// <summary>
    /// Controls the level of the encounter.
    /// </summary>
    public sealed class EncounterCalcLevelEvent : EntityEventArgs
    {
        public EncounterCalcLevelEvent(MapCoordinates coords)
        {
            OuterMapCoords = coords;
        }

        public MapCoordinates OuterMapCoords { get; }

        public int OutLevel { get; set; } = 1;
    }

    /// <summary>
    /// This is run before the player is transported to the encounter map.
    /// </summary>
    public sealed class EncounterBeforeMapEnteredEvent : EntityEventArgs
    {
        public EncounterBeforeMapEnteredEvent(MapCoordinates coords)
        {
            OuterMapCoords = coords;
        }

        public MapCoordinates OuterMapCoords { get; }
    }

    /// <summary>
    /// Generates the encounter. This function receives map that the encounter will take
    /// place in, like the open fields. Create any enemies you want here, and maybe add
    /// a deferred event to trigger a scripted dialog.
    /// </summary>
    public sealed class EncounterAfterMapEnteredEvent : EntityEventArgs
    {
        public EncounterAfterMapEnteredEvent(IMap encounterMap)
        {
            EncounterMap = encounterMap;
        }

        public IMap EncounterMap { get; }
    }
}
