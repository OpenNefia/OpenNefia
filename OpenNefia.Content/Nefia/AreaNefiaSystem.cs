using OpenNefia.Content.Levels;
using OpenNefia.Content.RandomAreas;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Dungeons;
using OpenNefia.Core.Locale;
using OpenNefia.Core;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Log;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using OpenNefia.Content.Logic;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.UI;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.CustomName;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Damage;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Ranks;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Chest;
using OpenNefia.Content.Charas;

namespace OpenNefia.Content.Nefia
{
    public sealed class AreaNefiaSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ILocalizationManager _loc = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IMapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvs = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IMusicManager _music = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IRankSystem _ranks = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IFameSystem _fame = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;

        public override void Initialize()
        {
            SubscribeComponent<AreaNefiaComponent, AreaEnteredEvent>(OnNefiaAreaEntered, priority: EventPriorities.High);
            SubscribeComponent<AreaNefiaComponent, AreaFloorGenerateEvent>(OnNefiaFloorGenerate, priority: EventPriorities.High);
            SubscribeComponent<AreaNefiaComponent, AreaGeneratedEvent>(OnNefiaGenerated, priority: EventPriorities.High);
            SubscribeComponent<AreaNefiaComponent, RandomAreaCheckIsActiveEvent>(OnCheckIsActive, priority: EventPriorities.High);
            SubscribeComponent<AreaNefiaComponent, AreaMapInitializeEvent>(SpawnNefiaBoss);
            SubscribeEntity<MapCalcDefaultMusicEvent>(SetNefiaMusic, priority: EventPriorities.High);

            SubscribeBroadcast<GenerateRandomAreaEvent>(GenerateRandomNefia, priority: EventPriorities.VeryLow);

            SubscribeComponent<MapComponent, GetDisplayNameEventArgs>(GetNefiaMapName, priority: EventPriorities.High);
            SubscribeComponent<NefiaBossComponent, GetDisplayNameEventArgs>(AppendBossLevelToName, priority: EventPriorities.High);
            SubscribeEntity<MapLeaveEventArgs>(RemoveNefiaBossOnMapLeave);
            SubscribeEntity<BeforeUseMapEntranceEvent>(CheckNefiaBossBeforeUseEntrance);

            SubscribeEntity<EntityKilledEvent>(CheckNefiaBossKilled);
        }

        private void OnNefiaAreaEntered(EntityUid uid, AreaNefiaComponent areaNefia, AreaEnteredEvent args)
        {
            if (areaNefia.State == NefiaState.Unvisited)
                areaNefia.State = NefiaState.Visited;
        }

        private void OnCheckIsActive(EntityUid uid, AreaNefiaComponent areaNefia, RandomAreaCheckIsActiveEvent args)
        {
            args.IsActive |= areaNefia.State == NefiaState.Unvisited || areaNefia.State == NefiaState.Visited;
        }

        private void SpawnNefiaBoss(EntityUid uid, AreaNefiaComponent areaNefia, AreaMapInitializeEvent args)
        {
            // -- >>>>>>>> shade2/map.hsp:395 	if areaId(gArea)=areaRandDungeon{ ...
            if (args.LoadType == MapLoadType.GameLoaded)
                return;

            if (!TryComp<AreaDungeonComponent>(uid, out var areaDungeon))
                return;

            if (areaDungeon.DeepestFloor == _mapEntrances.GetFloorNumber(args.Map))
            {
                if (areaNefia.State == NefiaState.BossVanished)
                {
                    _mes.Display(Loc.GetString("Elona.Nefia.NoDungeonMaster", ("mapEntity", args.Map.MapEntityUid)));
                }
                else if (areaNefia.State == NefiaState.Visited && areaNefia.BossEntityUid == null)
                {
                    _deferredEvs.Enqueue(() => EventNefiaBoss(args.Map));
                }
            }
            // -- <<<<<<<< shade2/map.hsp:398 		} ..
        }

        private TurnResult EventNefiaBoss(IMap map)
        {
            if (!_areaManager.TryGetAreaOfMap(map, out var area))
                return TurnResult.NoResult;

            if (!TryComp<AreaNefiaComponent>(area.AreaEntityUid, out var areaNefia))
                return TurnResult.NoResult;

            areaNefia.BossEntityUid = SpawnBoss(map);

            _mes.Display(Loc.GetString("Elona.Nefia.Event.ReachedDeepestLevel"));
            _mes.Display(Loc.GetString("Elona.Nefia.Event.GuardedByLord", ("mapEntity", map.MapEntityUid), ("bossEntity", areaNefia.BossEntityUid)), UiColors.MesRed);

            return TurnResult.NoResult;
        }

        private void RemoveNefiaBossOnMapLeave(EntityUid uid, MapLeaveEventArgs args)
        {
            if (!TryArea(args.OldMap, out var area)
                || !TryComp<AreaDungeonComponent>(area.AreaEntityUid, out var areaDungeon)
                || !TryComp<AreaNefiaComponent>(area.AreaEntityUid, out var areaNefia))
                return;

            if (areaDungeon.DeepestFloor == _mapEntrances.GetFloorNumber(args.OldMap) && areaNefia.State == NefiaState.Visited)
            {
                if (IsAlive(areaNefia.BossEntityUid))
                    EntityManager.DeleteEntity(areaNefia.BossEntityUid.Value);

                areaNefia.BossEntityUid = null;
                areaNefia.State = NefiaState.BossVanished;
            }
        }

        private void CheckNefiaBossBeforeUseEntrance(EntityUid uid, BeforeUseMapEntranceEvent args)
        {
            if (args.Cancelled)
                return;

            if (!TryMap(uid, out var map) || !TryArea(map, out var area)
                || !TryComp<AreaDungeonComponent>(area.AreaEntityUid, out var areaDungeon)
                || !TryComp<AreaNefiaComponent>(area.AreaEntityUid, out var areaNefia))
                return;

            if (areaDungeon.DeepestFloor == _mapEntrances.GetFloorNumber(map) && areaNefia.State == NefiaState.Visited
                && IsAlive(areaNefia.BossEntityUid))
            {
                if (_playerQuery.YesOrNoOrCancel(Loc.GetString("Elona.Nefia.PromptGiveUpQuest")) != true)
                {
                    args.Cancel();
                    return;
                }
            }
        }

        private void SetNefiaMusic(EntityUid uid, MapCalcDefaultMusicEvent args)
        {
            if (!TryMap(uid, out var map) || !TryArea(map, out var area)
                || !TryComp<AreaDungeonComponent>(area.AreaEntityUid, out var areaDungeon)
                || !TryComp<AreaNefiaComponent>(area.AreaEntityUid, out var areaNefia))
                return;

            if (areaDungeon.DeepestFloor == _mapEntrances.GetFloorNumber(map)
                && areaNefia.State == NefiaState.Visited)
            {
                args.OutMusicID = Protos.Music.Boss;
            }
        }

        private readonly LocaleKey[] NefiaNameTypes =
        {
            "Elona.Nefia.NameModifiers.TypeA",
            "Elona.Nefia.NameModifiers.TypeB"
        };

        /// <remarks>
        /// TODO: Rewrite this in terms of <see cref="DisplayName.DisplayNameSystem"/>!
        /// To support language switching, <see cref="MetaDataComponent.DisplayName"/> should never 
        /// be set manually!
        /// </remarks>
        private (string Type, int Rank) PickRandomNefiaNameArgs(int baseLevel)
        {
            var rankFactor = 5;
            var type = _random.Pick(NefiaNameTypes);
            var rank = Math.Clamp(baseLevel / rankFactor, 0, 4);
            return (type, rank);
        }

        private void GetNefiaMapName(EntityUid uid, MapComponent component, ref GetDisplayNameEventArgs args)
        {
            if (!TryMap(uid, out var map) || !TryArea(map, out var area)
                || !TryComp<AreaNefiaComponent>(area.AreaEntityUid, out var areaNefia))
                return;

            // !!! Recursion alert !!!
            var areaBaseName = _displayNames.GetBaseName(area.AreaEntityUid);
            args.OutName = _loc.GetString($"{areaNefia.NameType}.Rank{areaNefia.NameRank}", ("baseName", areaBaseName));
        }

        private EntityUid SpawnBoss(IMap map)
        {
            EntityUid? boss = null;
            var filter = new CharaFilter()
            {
                Quality = Qualities.Quality.Great,
                LevelOverride = _levels.GetLevel(map.MapEntityUid) + _random.Next(5)
            };
            while (boss == null)
            {
                boss = _charaGen.GenerateChara(map, filter);
            }

            EnsureComp<FactionComponent>(boss.Value).RelationToPlayer = Relation.Enemy;
            EnsureComp<NefiaBossComponent>(boss.Value);

            return boss.Value;
        }

        private void AppendBossLevelToName(EntityUid uid, NefiaBossComponent component, ref GetDisplayNameEventArgs args)
        {
            args.OutName = $"{args.OutName} Lv{_levels.GetLevel(uid)}";
        }

        private int CalcBossPlatinumAmount(EntityUid player)
        {
            return Math.Clamp(_random.Next(3) + _levels.GetLevel(player) / 10, 1, 6);
        }

        private int CalcBossFameGained(EntityUid player, IMap map)
        {
            // >>>>>>>> shade2/main.hsp:1763 	gQuestFame	=calcFame(pc,gLevel*30+200) ...
            return _fame.CalcFameGained(player, _levels.GetLevel(map.MapEntityUid) * 30 + 200);
            // <<<<<<<< shade2/main.hsp:1763 	gQuestFame	=calcFame(pc,gLevel*30+200) ...
        }

        private void CreateBossRewards(EntityUid player, EntityUid boss)
        {
            // >>>>>>>> shade2/main.hsp:1751 	case evRandBossWin ...
            if (!TryMap(player, out var map))
                return;

            var filter = new ItemFilter()
            {
                MinLevel = _randomGen.CalcObjectLevel(map),
                Quality = _randomGen.CalcObjectQuality(),
                Tags = new[] { Protos.Tag.ItemCatSpellbook }
            };
            _itemGen.GenerateItem(player, filter);

            var scroll = _itemGen.GenerateItem(player, Protos.Item.ScrollOfReturn);

            var goldAmount = 200 + _stacks.GetCount(scroll) * 5; // XXX: ...What?
            _itemGen.GenerateItem(player, Protos.Item.GoldPiece, amount: goldAmount);

            var platinumAmount = CalcBossPlatinumAmount(player);
            _itemGen.GenerateItem(player, Protos.Item.PlatinumCoin, amount: platinumAmount);

            var chest = _itemGen.GenerateItem(player, Protos.Item.BejeweledChest);
            if (TryComp<ChestComponent>(chest, out var chestComp))
                chestComp.LockpickDifficulty = 0;
            // <<<<<<<< shade2/main.hsp:1765 	cFame(pc)+=gQuestFame ..
        }

        private TurnResult EventNefiaBossDefeated(IMap map)
        {
            if (!_areaManager.TryGetAreaOfMap(map, out var area))
                return TurnResult.NoResult;

            if (!TryComp<AreaNefiaComponent>(area.AreaEntityUid, out var areaNefia))
                return TurnResult.NoResult;

            _music.Play(Protos.Music.Victory);
            _audio.Play(Protos.Sound.Complete1);

            var boss = areaNefia.BossEntityUid;

            if (boss != null)
                CreateBossRewards(_gameSession.Player, boss.Value);

            // >>>>>>>> shade2/main.hsp:1762 	txtEf coGreen:txtQuestComplete:txtMore:txtQuestIt ...
            _mes.Display(Loc.GetString("Elona.Quest.Completed"), UiColors.MesGreen);
            _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));
            _ranks.ModifyRank(Protos.Rank.Crawler, 300, 8);

            if (TryComp<FameComponent>(_gameSession.Player, out var fame))
            {
                var fameGained = CalcBossFameGained(_gameSession.Player, map);
                _mes.Display(Loc.GetString("Elona.Quest.GainFame", ("fameGained", fameGained)), UiColors.MesGreen);
                fame.Fame.Base += fameGained;
            }
            // <<<<<<<< shade2/main.hsp:1765 	cFame(pc)+=gQuestFame ..

            // >>>>>>>> shade2/main.hsp:1771 	}else{ ...
            areaNefia.BossEntityUid = null;
            areaNefia.State = NefiaState.Conquered;
            // <<<<<<<< shade2/main.hsp:1773 	} ..

            return TurnResult.NoResult;
        }

        private void CheckNefiaBossKilled(EntityUid uid, ref EntityKilledEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1717 			if (gLevel=areaMaxLevel(gArea))or(gArea=areaVoi ...
            if (!TryMap(uid, out var map))
                return;

            if (!_areaManager.TryGetAreaOfMap(map, out var area))
                return;

            if (!TryComp<AreaNefiaComponent>(area.AreaEntityUid, out var areaNefia))
                return;

            if (areaNefia.BossEntityUid == uid)
            {
                _deferredEvs.Enqueue(() => EventNefiaBossDefeated(map));
            }
            // <<<<<<<< shade2/chara_func.hsp:1719 				} ..
        }

        private void OnNefiaFloorGenerate(EntityUid uid, AreaNefiaComponent component, AreaFloorGenerateEvent args)
        {
            if (args.Handled)
                return;

            var ev = new NefiaFloorGenerateEvent(args.Area, args.FloorId, args.PreviousCoords);
            RaiseEvent(uid, ev);

            if (ev.Handled)
            {
                args.Handle(ev.ResultMapId!.Value);
            }
        }

        public void GenerateRandomNefia(GenerateRandomAreaEvent args)
        {
            if (args.Handled)
                return;

            if (!_mapManager.TryGetMap(args.RandomAreaCoords.MapId, out var map))
                return;

            if (!_areaManager.TryGetAreaOfMap(map, out var parentArea))
                return;

            var areaEntityProto = PickRandomNefiaEntityID();

            var area = _areaManager.CreateArea(areaEntityProto, parent: parentArea.Id);

            args.Handle(area);
        }

        private void OnNefiaGenerated(EntityUid areaEntity, AreaNefiaComponent areaNefia, AreaGeneratedEvent args)
        {
            var baseLevel = Math.Max(PickRandomNefiaLevel(), 1);
            var floorCount = Math.Max(PickRandomNefiaFloorCount(), 1);

            var (nefiaNameType, nefiaNameRank) = PickRandomNefiaNameArgs(baseLevel);
            areaNefia.NameType = nefiaNameType;
            areaNefia.NameRank = nefiaNameRank;

            areaNefia.BaseLevel = baseLevel;

            var areaDungeonComp = EntityManager.EnsureComponent<AreaDungeonComponent>(areaEntity);
            areaDungeonComp.DeepestFloor = floorCount;

            var areaEntranceComp = EntityManager.EnsureComponent<AreaEntranceComponent>(areaEntity);
            areaEntranceComp.StartingFloor = FloorNumberToAreaId(1);
        }

        public static int AreaIdToFloorNumber(AreaFloorId id)
        {
            var n = Regex.Replace((string)id, @"^Elona.Nefia:Floor:(\d+)", "$1");
            if (int.TryParse(n, out int floorNumber))
                return floorNumber;

            Logger.ErrorS("nefia.gen", $"Failed to parse floor number: {id}");
            return 1;
        }

        public static AreaFloorId FloorNumberToAreaId(int floorNumber)
        {
            return new AreaFloorId($"Elona.Nefia:Floor:{floorNumber}");
        }

        private PrototypeId<EntityPrototype> PickRandomNefiaEntityID()
        {
            var protos = _protos.EnumeratePrototypes<EntityPrototype>()
                .Where(proto => proto.Components.HasComponent<AreaNefiaComponent>())
                .ToList();

            return _random.Pick(protos).GetStrongID();
        }

        private int PickRandomNefiaLevel()
        {
            if (_random.OneIn(3))
            {
                var playerLevel = _levels.GetLevel(_gameSession.Player);
                return _random.Next(playerLevel + 5) + 1;
            }
            else
            {
                var level = _random.Next(50) + 1;
                if (_random.OneIn(5))
                {
                    level *= (_random.Next(3) + 1);
                }
                return level;
            }
        }

        private int PickRandomNefiaFloorCount()
        {
            return _random.Next(4) + 2;
        }

        /// <summary>
        /// Given a Nefia's base level and the number of floors deep into it,
        /// returns the creature level of that floor.
        /// </summary>
        public static int NefiaFloorNumberToLevel(int floorNumber, int nefiaBaseLevel)
        {
            // In OpenNefia, we start dungeons on floor 1. Nefia level is what used to be
            // "starting floor", so a nefia of level 5 would start on the fifth floor.
            // This means (nefia_level + floor) would be off by one, so subtract 1.
            return nefiaBaseLevel + floorNumber - 1;
        }

        private static IEnumerable<Vector2i> EnumerateBorder(UIBox2i bounds)
        {
            var start = bounds.TopLeft;
            var end = bounds.BottomRight;
            var pos = start;

            while (pos.Y <= end.Y)
            {
                yield return pos;

                if (pos.Y == start.Y || pos.Y == end.Y - 1)
                {
                    if (pos.X == end.X - 1)
                    {
                        pos.X = start.X;
                        pos.Y++;
                    }
                    else
                    {
                        pos.X++;
                    }
                }
                else
                {
                    if (pos.X == start.X)
                    {
                        pos.X = end.X - 1;
                    }
                    else
                    {
                        pos.X = start.X;
                        pos.Y++;
                    }
                }
            }
        }
    }

    public sealed class NefiaFloorGenerateEvent : HandledEntityEventArgs
    {
        /// <summary>
        /// Area a floor is being generated in.
        /// </summary>
        public IArea Area { get; }

        /// <summary>
        /// ID of the floor.
        /// </summary>
        public AreaFloorId FloorId { get; }

        /// <summary>
        /// Map coordinates of the player at the time the floor is generated.
        /// This can be used to generate a map based on the terrain the player
        /// is standing on (fields, forest, desert, etc.)
        /// </summary>
        public MapCoordinates PreviousCoords { get; }

        // TODO would be nice to have EntityCoordinates also

        /// <summary>
        /// Map of the area's floor that was created. If this is left as <c>null</c>,
        /// then floor creation failed.
        /// </summary>
        public MapId? ResultMapId { get; private set; }

        public NefiaFloorGenerateEvent(IArea area, AreaFloorId floorId, MapCoordinates previousCoords)
        {
            Area = area;
            FloorId = floorId;
            PreviousCoords = previousCoords;
        }

        public void Handle(IMap map) => Handle(map.Id);

        public void Handle(MapId mapId)
        {
            Handled = true;
            ResultMapId = mapId;
        }
    }
}
