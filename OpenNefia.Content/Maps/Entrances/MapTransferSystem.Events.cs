using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.World;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Sanity;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Sidequest;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Karma;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Factions;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.Random;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Quests;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Dungeons;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Locale;
using OpenNefia.Core;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.SaveLoad;
using OpenNefia.Content.Weather;
using OpenNefia.Content.Cargo;
using OpenNefia.Core.Configuration;
using OpenNefia.Content.Pickable;

namespace OpenNefia.Content.Maps
{
    public partial class MapTransferSystem
    {
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ICharaSystem _chara = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IQuestSystem _quests = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly CommonEffectsSystem _commonEffects = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IDisplayNameSystem _names = default!;
        [Dependency] private readonly ISaveLoadSystem _saveLoad = default!;
        [Dependency] private readonly ITurnOrderSystem _turnOrder = default!;
        [Dependency] private readonly IWeatherSystem _weather = default!;
        [Dependency] private readonly ICargoSystem _cargo = default!;
        [Dependency] private readonly IMapDebrisSystem _mapDebris = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly MapCommonSystem _mapCommon = default!;

        public const int MapRenewMajorIntervalHours = 120;
        public const int MapRenewMinorIntervalHours = 24;

        public override void Initialize()
        {
            SubscribeComponent<PlayerComponent, ExitingMapFromEdgesEventArgs>(HandleExitMapFromEdges, priority: EventPriorities.Low);
            SubscribeEntity<ActiveMapChangedEvent>(HandleActiveMapChanged, priority: EventPriorities.VeryHigh);
            SubscribeComponent<MapComponent, MapLeaveEventArgs>(HandleLeaveMap, priority: EventPriorities.VeryHigh);
        }

        private void HandleExitMapFromEdges(EntityUid playerUid, PlayerComponent component, ExitingMapFromEdgesEventArgs args)
        {
            if (args.Handled)
                return;

            SpatialComponent? spatial = null;

            if (!Resolve(playerUid, ref spatial))
                return;

            var turnResult = _mapEntrances.UseMapEntrance(playerUid, args.Entrance)
                ? TurnResult.Succeeded : TurnResult.Failed;
            args.Handle(turnResult);
        }

        private void HandleLeaveMap(EntityUid uid, MapComponent component, MapLeaveEventArgs args)
        {
            // >>>>>>>> shade2/map.hsp:202 	if gArea ! gAreaPrev{ ..
            if (!_areaManager.TryGetAreaOfMap(args.OldMap, out var oldArea)
                || !_areaManager.TryGetAreaOfMap(args.NewMap, out var newArea)
                || oldArea.Id == newArea.Id)
                return;

            if (IsTravelDestination(args.OldMap))
            {
                _world.State.TravelDistance = 0;
                _world.State.LastDepartedMapName = _names.GetDisplayName(args.OldMap.MapEntityUid);
                _world.State.TravelStartDate = _world.State.GameDate;
            }

            if (!HasComp<MapTypeFieldComponent>(args.NewMap.MapEntityUid)
                && !HasComp<MapTypeWorldMapComponent>(args.NewMap.MapEntityUid))
            {
                _saveLoad.QueueAutosave();
            }
            // <<<<<<<< shade2/map.hsp:204 		if (areaType(gArea)!mTypeWorld)&(areaType(gArea) ..

            _mes.Clear();

            // >>>>>>>> shade2/map.hsp:212 			if areaType(gAreaPrev)=mTypeWorld{ ..
            if (_turnOrder.PlayerAboutToRespawn)
            {
                _mes.Display(Loc.GetString("Elona.MapTransfer.Leave.DeliveredToYourHome"));
                _weather.ChangeFromWorldMap(args.NewMap);
            }
            else if (HasComp<MapTypeWorldMapComponent>(args.OldMap.MapEntityUid))
            {
                _mes.Display(Loc.GetString("Elona.MapTransfer.Leave.Entered", ("mapEntity", args.NewMap.MapEntityUid)));
            }
            else if (HasComp<MapTypeQuestComponent>(args.OldMap.MapEntityUid))
            {
                _mes.Display(Loc.GetString("Elona.MapTransfer.Leave.ReturnedTo", ("mapEntity", args.NewMap.MapEntityUid)));
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.MapTransfer.Leave.Left", ("mapEntity", args.OldMap.MapEntityUid)));
            }

            if (_cargo.IsBurdenedByCargo(_gameSession.Player))
            {
                _mes.Display(Loc.GetString("Elona.MapTransfer.Leave.BurdenedByCargo"));
            }
            // <<<<<<<< shade2/map.hsp:219 		} ..
        }

        private bool IsTravelDestination(IMap map)
        {
            return HasComp<MapTypeTownComponent>(map.MapEntityUid)
                    || HasComp<MapTypeTownComponent>(map.MapEntityUid)
                    || Comp<MapCommonComponent>(map.MapEntityUid).IsTravelDestination;
        }

        private void HandleActiveMapChanged(EntityUid uid, ActiveMapChangedEvent args)
        {
            Logger.WarningS("map.transfer", $"Map load: {args.OldMap} -> {args.NewMap} {args.LoadType}");
            _mapCommon.PlayMapDefaultMusic(args.NewMap);
            RunMapInitializeEvents(args.NewMap, args.LoadType);
        }

        private void RunMapInitializeEvents(IMap map, MapLoadType loadType)
        {
            OnMapInitialize(map, loadType);

            if (loadType == MapLoadType.InitializeOnly)
                return;

            if (TryComp<MapVanillaAIComponent>(map.MapEntityUid, out var vai))
            {
                vai.NoAggroRefresh = false;
            }

            if (loadType == MapLoadType.GameLoaded)
                return;

            OnMapEnter(map, loadType);
        }

        private void OnMapInitialize(IMap map, MapLoadType loadType)
        {
            if (loadType != MapLoadType.Traveled)
            {
                _mes.Clear();
            }

            if (loadType == MapLoadType.GameLoaded)
                return;

            _mapDebris.Clear(map);

            var common = EntityManager.GetComponent<MapCommonComponent>(map.MapEntityUid);

            if (!common.IsTemporary)
                PrepareSaveableMap(map, loadType, common);

            CheckMapRenew(map);
            RecalculateMaxCharas(map);
            UpdateQuests(map);

            var ev = new MapInitializeEvent(map, loadType);
            RaiseEvent(map.MapEntityUid, ev);
            if (_areaManager.TryGetAreaOfMap(map.Id, out var area))
            {
                var areaEv = new AreaMapInitializeEvent(map, loadType);
                RaiseEvent(area.AreaEntityUid, areaEv);
            }
        }

        private void CheckMapRenew(IMap map)
        {
            // >>>>>>>> shade2/map.hsp:2173 *check_renew ..
            var common = EntityManager.GetComponent<MapCommonComponent>(map.MapEntityUid);

            var forceRenewal = _config.GetCVar(CCVars.DebugForceMapRenewal);

            if (_world.State.GameDate > common.RenewMajorDate || forceRenewal == ForceMapRenewalType.Major)
            {
                RenewMajor(map, common);
                common.RenewMajorDate = _world.State.GameDate + GameTimeSpan.FromHours(MapRenewMajorIntervalHours);
            }
            if (_world.State.GameDate > common.RenewMinorDate || forceRenewal == ForceMapRenewalType.Minor)
            {
                RenewMinor(map, common);
                common.RenewMinorDate = _world.State.GameDate + GameTimeSpan.FromHours(MapRenewMinorIntervalHours);
            }
            // <<<<<<<< shade2/map.hsp:2273 	return ..
        }

        private void RenewMajor(IMap map, MapCommonComponent common)
        {
            if (!common.IsRenewable)
            {
                Logger.WarningS("map.renew", $"Skiping major renewal for {map}.");
                return;
            }

            var isFirstRenewal = common.RenewMajorDate == GameDateTime.Zero;

            Logger.InfoS("map.renew", $"Running major renewal for map {map}.");

            if (!isFirstRenewal)
            {
                foreach (var tempEnt in _lookup.EntityQueryInMap<TemporaryEntityComponent>(map.Id).ToList())
                {
                    EntityManager.DeleteEntity(tempEnt.Owner);
                }

                if (HasComp<MapTypeTownComponent>(map.MapEntityUid) || HasComp<MapTypeGuildComponent>(map.MapEntityUid))
                {
                    foreach (var item in _lookup.EntityQueryInMap<PickableComponent>(map.Id).ToList())
                    {
                        var evRenew = new OnItemRenewMajorEvent();
                        RaiseEvent(item.Owner, evRenew);

                        if (item.OwnState == OwnState.None || _stacks.GetCount(item.Owner) <= 0)
                            EntityManager.DeleteEntity(item.Owner);
                    }
                }
            }

            foreach (var chara in _chara.EnumerateNonAllies(map).ToList())
            {
                _chara.RenewStatus(chara.Owner, chara);
                if (EntityManager.IsAlive(chara.Owner))
                {
                    if (HasComp<SummonedEntityComponent>(chara.Owner) && _rand.OneIn(2))
                    {
                        EntityManager.DeleteEntity(chara.Owner);
                    }
                }
                else if (chara.Liveness == CharaLivenessState.Dead)
                {
                    EntityManager.DeleteEntity(chara.Owner);
                }
            }

            var ev = new MapRenewMajorEvent(map, isFirstRenewal);
            RaiseEvent(map.MapEntityUid, ev);
        }

        private void RenewMinor(IMap map, MapCommonComponent common)
        {
            if (!common.IsRenewable)
            {
                Logger.WarningS("map.renew", $"Skiping minor renewal for map {map.Id}.");
                return;
            }

            int renewSteps;

            if (common.RenewMinorDate == GameDateTime.Zero)
            {
                renewSteps = 1;
            }
            else
            {
                renewSteps = Math.Max((_world.State.GameDate - common.RenewMinorDate).TotalHours / MapRenewMinorIntervalHours, 1);
            }

            Logger.InfoS("map.renew", $"Running minor renewal for map {map}. ({renewSteps} steps)");

            var ev = new MapRenewMinorEvent(map, renewSteps);
            RaiseEvent(map.MapEntityUid, ev);
        }

        private void RecalculateMaxCharas(IMap map)
        {
            if (!TryComp<MapCharaGenComponent>(map.MapEntityUid, out var gen))
                return;

            var noAggroRefresh = false;
            if (TryComp<MapVanillaAIComponent>(map.MapEntityUid, out var vai))
                noAggroRefresh = vai.NoAggroRefresh;

            gen.CurrentCharaCount = 0;
            foreach (var (chara, turnOrder) in _lookup.EntityQueryInMap<CharaComponent, TurnOrderComponent>(map.Id))
            {
                turnOrder.TimeThisTurn = 0;
                if (!noAggroRefresh)
                {
                    _vanillaAI.ResetAI(chara.Owner);
                }
                if (!_parties.IsInPlayerParty(chara.Owner) && chara.Liveness == CharaLivenessState.Dead)
                {
                    gen.CurrentCharaCount++;
                }
            }
        }

        private void UpdateQuests(IMap map)
        {
            _quests.UpdateInMap(map);
        }

        private void PrepareSaveableMap(IMap map, MapLoadType loadType, MapCommonComponent common)
        {
            var isFirstRenewal = common.RenewMajorDate == GameDateTime.Zero;

            if (!common.IsTemporary && common.IsRenewable)
            {
                if ((_world.State.GameDate > common.RenewMajorDate
                    || _config.GetCVar(CCVars.DebugForceMapRenewal) == ForceMapRenewalType.Major)
                    && !isFirstRenewal)
                {
                    var ev = new MapRenewGeometryEvent();
                    Raise(map.MapEntityUid, ev);
                }
            }

            if (loadType == MapLoadType.Full || loadType == MapLoadType.Traveled)
                RenewCharaFlags(map);
        }

        /// <summary>
        /// Revives dead villagers, procs guard aggro if wanted, sets villagers
        /// to sleep depending on the time.
        /// </summary>
        /// <param name="map"></param>
        private void RenewCharaFlags(IMap map)
        {
            void Renew(CharaComponent chara)
            {
                var ent = chara.Owner;

                if (chara.Liveness == CharaLivenessState.VillagerDead
                    && _world.State.GameDate >= chara.RespawnDate)
                {
                    _chara.Revive(ent, chara: chara);
                }

                if (EntityManager.IsAlive(ent))
                {
                    if (!_parties.IsInPlayerParty(ent))
                    {
                        if (TryComp<SkillsComponent>(ent, out var skills))
                        {
                            skills.HP = skills.MaxHP;
                            skills.MP = skills.MaxMP;
                        }

                        if (TryComp<SanityComponent>(ent, out var sanity))
                        {
                            sanity.Insanity = 0;
                        }

                        if (!HasComp<SidequestTargetComponent>(ent))
                        {
                            _vanillaAI.SetTarget(ent, null);
                        }

                        if (HasComp<RoleGuardComponent>(ent))
                        {
                            var player = _gameSession.Player;
                            if (Comp<KarmaComponent>(player).Karma < KarmaLevels.Bad)
                            {
                                if (_levels.GetLevel(player) < _levels.GetLevel(ent))
                                {
                                    _levels.GainLevel(ent, showMessage: false);
                                }
                            }
                            if (!TryComp<KarmaComponent>(player, out var karma) || !karma.IsIncognito.Buffed)
                            {
                                _vanillaAI.SetTarget(ent, player, 200);
                                EnsureComp<FactionComponent>(ent).RelationToPlayer = Relation.Enemy;
                            }
                        }
                    }

                    if (HasComp<MapTypeTownComponent>(map.MapEntityUid)
                        || HasComp<MapTypeGuildComponent>(map.MapEntityUid))
                    {
                        _effects.Remove(ent, Protos.StatusEffect.Sleep);
                        var date = _world.State.GameDate;
                        if (date.Hour >= 22 || date.Hour < 7)
                        {
                            if (_rand.OneIn(6))
                            {
                                _effects.SetTurns(ent, Protos.StatusEffect.Sleep, _rand.Next(400));
                            }
                        }
                    }
                }
            }

            foreach (var chara in _lookup.EntityQueryInMap<CharaComponent>(map.Id))
                Renew(chara);
        }

        private void OnMapEnter(IMap map, MapLoadType loadType)
        {
            _commonEffects.WakeUpEveryone(map);
            _vanillaAI.ResetAI(_gameSession.Player);
            RevealFog(map);

            var ev = new MapEnterEvent(map, loadType);
            RaiseEvent(map.MapEntityUid, ev);
            if (_areaManager.TryGetAreaOfMap(map.Id, out var area))
            {
                var areaEv = new AreaMapEnterEvent(map, loadType);
                RaiseEvent(area.AreaEntityUid, areaEv);
            }

            UpdateDeepestFloor(map);
            GainTravelingExperience(map);
        }

        private bool RevealsFog(IMap map)
        {
            var mapEnt = map.MapEntityUid;

            var revealsFog = Comp<MapCommonComponent>(mapEnt).RevealsFog;
            if (revealsFog.HasValue)
                return revealsFog.Value;

            return HasComp<MapTypeTownComponent>(mapEnt)
                || HasComp<MapTypeWorldMapComponent>(mapEnt)
                || HasComp<MapTypePlayerOwnedComponent>(mapEnt)
                || HasComp<MapTypeGuildComponent>(mapEnt);
        }

        private void RevealFog(IMap map)
        {
            if (!RevealsFog(map))
                return;

            foreach (var tile in map.AllTiles)
            {
                map.MemorizeTile(tile.Position);
            }
        }

        private void UpdateDeepestFloor(IMap map)
        {
            if (_areaManager.TryGetAreaOfMap(map, out var area)
                && TryComp<AreaDungeonComponent>(area.AreaEntityUid, out var areaDungeon))
            {
                areaDungeon.DeepestFloor = Math.Max(areaDungeon.DeepestFloor, _mapEntrances.GetFloorNumber(map));
            }
        }

        private void GainTravelingExperience(IMap map)
        {
            if (!IsTravelDestination(map))
                return;

            if (_world.State.TravelDistance < 16)
                return;

            var timePassed = _world.State.GameDate - _world.State.TravelStartDate;
            _mes.Display(Loc.GetString("Elona.MapTransfer.Travel.TimePassed", ("days", timePassed.TotalDays), ("hours", timePassed.Hour), ("lastTownName", _world.State.LastDepartedMapName ?? "???"), ("date", _world.State.TravelStartDate)));

            var player = _gameSession.Player;
            var playerLevel = EnsureComp<LevelComponent>(player);
            var playerExpPerMember = _levels.GetLevel(player, playerLevel) * _world.State.TravelDistance * _skills.Level(player, Protos.Skill.Traveling) / 100 + 1;
            var totalAllies = 0;

            foreach (var ally in _parties.EnumerateMembers(player).Where(e => EntityManager.IsAlive(e)))
            {
                totalAllies++;
                playerLevel.Experience += playerExpPerMember;
            }

            LocaleKey key = "Elona.MapTransfer.Travel.Walked.You";
            if (totalAllies > 1)
                key = "Elona.MapTransfer.Travel.Walked.YouAndAllies";
            _mes.Display(Loc.GetString(key, ("travelDistance", _world.State.TravelDistance)));

            _skills.GainSkillExp(player, Protos.Skill.Traveling, 25 + _world.State.TravelDistance * 2 / 3, 0, 1000);
            _world.State.TravelDistance = 0;
        }
    }

    public sealed class OnItemRenewMajorEvent : EntityEventArgs
    {
        public OnItemRenewMajorEvent()
        {
        }
    }

    [EventUsage(EventTarget.Map)]
    public sealed class MapInitializeEvent : EntityEventArgs
    {
        public IMap Map { get; }
        public MapLoadType LoadType { get; }

        public MapInitializeEvent(IMap map, MapLoadType loadType)
        {
            Map = map;
            LoadType = loadType;
        }
    }

    [EventUsage(EventTarget.Area)]
    public sealed class AreaMapInitializeEvent : EntityEventArgs
    {
        public IMap Map { get; }
        public MapLoadType LoadType { get; }

        public AreaMapInitializeEvent(IMap map, MapLoadType loadType)
        {
            Map = map;
            LoadType = loadType;
        }
    }

    public sealed class MapRenewMajorEvent : EntityEventArgs
    {
        public IMap Map { get; }
        public bool IsFirstRenewal { get; }

        public MapRenewMajorEvent(IMap map, bool isFirstRenewal)
        {
            Map = map;
            IsFirstRenewal = isFirstRenewal;
        }
    }

    public sealed class MapRenewMinorEvent : EntityEventArgs
    {
        public IMap Map { get; }
        public int RenewSteps { get; }

        public MapRenewMinorEvent(IMap map, int renewSteps)
        {
            Map = map;
            RenewSteps = renewSteps;
        }
    }

    [EventUsage(EventTarget.Map)]
    public sealed class MapEnterEvent : EntityEventArgs
    {
        public IMap Map { get; }
        public MapLoadType LoadType { get; }

        public MapEnterEvent(IMap map, MapLoadType loadType)
        {
            Map = map;
            LoadType = loadType;
        }
    }

    [EventUsage(EventTarget.Area)]
    public sealed class AreaMapEnterEvent : EntityEventArgs
    {
        public IMap Map { get; }
        public MapLoadType LoadType { get; }

        public AreaMapEnterEvent(IMap map, MapLoadType loadType)
        {
            Map = map;
            LoadType = loadType;
        }
    }

    public sealed class MapRenewGeometryEvent : HandledEntityEventArgs
    {
    }
}
