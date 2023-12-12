using OpenNefia.Content.Feats;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Weather;
using System.Drawing;
using OpenNefia.Core.Game;
using OpenNefia.Content.Cargo;
using OpenNefia.Content.Levels;
using OpenNefia.Core.SaveGames;
using OpenNefia.Content.FieldMap;
using OpenNefia.Content.WorldMap;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Areas;
using OpenNefia.Core.Log;
using OpenNefia.Core.EngineVariables;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Encounters
{
    public interface IEncounterSystem : IEntitySystem
    {
        /// <summary>
        /// Given coordinates in a (loaded) world map, returns the shortest
        /// distance to the nearest town entrance.
        /// A "town entrance" is defined as a <see cref="WorldMapEntranceComponent"/>
        /// leading to an area with an <see cref="AreaTypeTownComponent"/>.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        int DistanceFromNearestTown(MapCoordinates coords);

        /// <summary>
        /// Randomly chooses an encounter entity ID. Called on every step in the world map.
        /// <c>null</c> means no encounter was randomly chosen this time.
        /// </summary>
        /// <param name="coords">World map coordinates the player stepped into.</param>
        /// <returns></returns>
        PrototypeId<EntityPrototype>? PickRandomEncounterID(MapCoordinates coords);

        /// <summary>
        /// Starts an encounter. The encounter entity must have an <see cref="EncounterComponent"/>.
        /// </summary>
        /// <param name="encounterUid"></param>
        /// <param name="outerMapCoords"></param>
        void StartEncounter(EntityUid encounterUid, MapCoordinates outerMapCoords);

        /// <summary>
        /// Calculates the chance of running across rogues in the world map with each step.
        /// </summary>
        /// <param name="player">Current player.</param>
        /// <returns>Chance as a percentage.</returns>
        float CalcRogueAppearanceChance(EntityUid player);

        bool IsEncounterActive(IMap map);
        bool TryGetEncounter(IMap map, [NotNullWhen(true)] out EncounterComponent? encounter);
        bool TryGetEncounter<T>(IMap map, [NotNullWhen(true)] out T? encounter) where T: class, IComponent;
    }

    public sealed class EncounterSystem : EntitySystem, IEncounterSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IWeatherSystem _weather = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly ICargoSystem _cargos = default!;
        [Dependency] private readonly IWorldMapFieldsSystem _worldMapFields = default!;
        [Dependency] private readonly IMapTransferSystem _mapTransfers = default!;
        [Dependency] private readonly IMapEntranceSystem _mapEntrances = default!;

        public override void Initialize()
        {
            SubscribeBroadcast<MapCalcRandomEncounterIDEvent>(BaseCalcRandomEncounterID, priority: EventPriorities.VeryHigh);
            SubscribeBroadcast<MapCalcRandomEncounterIDEvent>(SetForceEncounterID, priority: EventPriorities.Lowest);
        }

        public int DistanceFromNearestTown(MapCoordinates coords)
        {
            if (!TryMap(coords, out var map))
                return int.MaxValue;

            // >>>>>>>> shade2/map_func.hsp:232 #module  ...
            bool Filter(WorldMapEntranceComponent comp)
            {
                var areaId = comp.Entrance.MapIdSpecifier.GetOrGenerateAreaId();
                if (areaId == null)
                    return false;

                if (!TryArea(areaId.Value, out var area))
                    return false;
                return HasComp<AreaTypeTownComponent>(area.AreaEntityUid);
            }

            int Distance(WorldMapEntranceComponent comp)
            {
                if (coords.TryDistanceTiled(Spatial(comp.Owner).MapPosition, out var distance))
                    return distance;
                return int.MaxValue;
            }

            return _lookup.EntityQueryInMap<WorldMapEntranceComponent>(map)
                .Where(Filter)
                .Select(Distance)
                .Order()
                .FirstOrDefault(int.MaxValue);
            // <<<<<<<< shade2/map_func.hsp:245 #global ..
        }

        public PrototypeId<EntityPrototype>? PickRandomEncounterID(MapCoordinates coords)
        {
            if (!TryMap(coords, out var map))
                return null;

            var ev = new MapCalcRandomEncounterIDEvent(coords);
            RaiseEvent(map.MapEntityUid, ev);
            return ev.OutEncounterId;
        }

        private void BaseCalcRandomEncounterID(MapCalcRandomEncounterIDEvent ev)
        {
            PrototypeId<EntityPrototype>? id = null;
            var map = GetMap(ev.Coords);

            if (_rand.OneIn(30))
                id = Protos.Encounter.Enemy;

            if (_weather.IsWeatherActive(Protos.Weather.HardRain) && _rand.OneIn(10))
                id = Protos.Encounter.Enemy;

            if (_weather.IsWeatherActive(Protos.Weather.Etherwind) && _rand.OneIn(13))
                id = Protos.Encounter.Enemy;

            var tile = map.GetTileID(ev.Coords.Position);
            if (tile != null && ProtoSets.Tile.WorldMapRoadTiles.Contains(tile.Value))
            {
                if (_rand.OneIn(2))
                    id = null;
                if (_rand.OneIn(100))
                    id = Protos.Encounter.Merchant;
            }

            var rogueChance = CalcRogueAppearanceChance(_gameSession.Player);
            if (_rand.Prob(rogueChance))
                id = Protos.Encounter.Rogue;

            ev.OutEncounterId = id;
        }

        [EngineVariable("Elona.DebugForceEncounter")]
        public PrototypeId<EntityPrototype>? DebugForceEncounter { get; } = null;

        private void SetForceEncounterID(MapCalcRandomEncounterIDEvent ev)
        {
            if (DebugForceEncounter != null)
                ev.OutEncounterId = DebugForceEncounter.Value;
        }

        public float CalcRogueAppearanceChance(EntityUid player)
        {
            // >>>>>>>> shade2/action.hsp:662 			if rnd(220+cLevel(pc)*10-limit(gCargoWeight*150 ...
            var oneInChance = 220;
            if (!TryComp<CargoHolderComponent>(player, out var cargoHolder))
                return 1.0f / oneInChance;

            var cargoWeight = _cargos.GetTotalCargoWeight(player);
            var level = _levels.GetLevel(player);
            oneInChance += level * 10 - Math.Clamp(cargoWeight * 150 / (cargoHolder.InitialMaxCargoWeight + 1), 0, 210 + level * 10);
            return 1.0f / oneInChance;
            // <<<<<<<< shade2/action.hsp:662 			if rnd(220+cLevel(pc)*10-limit(gCargoWeight*150 ..
        }

        private IMap GenerateDefaultEncounterMap(EntityUid encounterUid)
        {
            // TODO hack
            var encounter = EnsureComp<EncounterComponent>(encounterUid);
            var gen = new FieldMapGenerator()
            {
                FieldMap = _worldMapFields.GetFieldMapFromStoodTile(encounter.StoodWorldMapTile)
            };
            EntitySystem.InjectDependencies(gen);
            return gen.GenerateAndPopulate(new MapGeneratorOptions()
            {
                Width = 34,
                Height = 22
            });
        }

        public void StartEncounter(EntityUid encounterUid, MapCoordinates outerMapCoords)
        {
            if (_config.GetCVar(CCVars.DebugNoEncounters))
                return;

            if (!IsAlive(encounterUid) || !TryComp<EncounterComponent>(encounterUid, out var encounter))
            {
                Logger.ErrorS("encounter", "Could not start encounter, encounter entity was not valid");
                return;
            }

            if (!TryMap(outerMapCoords, out var outerMap))
            {
                Logger.ErrorS("encounter", "Could not start encounter, outer map coordinates were not valid");
                return;
            }

            var stoodMapTile = outerMap.GetTileID(outerMapCoords.Position);
            if (stoodMapTile == null)
            {
                Logger.ErrorS("encounter", "Could not start encounter, outer map tile coordinates were not valid");
                return;
            }

            encounter.StoodWorldMapTile = stoodMapTile.Value;

            var evLevel = new EncounterCalcLevelEvent(outerMapCoords);
            RaiseEvent(encounterUid, evLevel);
            encounter.Level = Math.Max(evLevel.OutLevel, 1);

            var evGen = new EncounterGenerateMapEvent();
            RaiseEvent(encounterUid, evGen);
            IMap encounterMap = evGen.ResultMap ?? GenerateDefaultEncounterMap(encounterUid);

            if (!EnsureComp<MapEncounterComponent>(encounterMap.MapEntityUid).EncounterContainer.Insert(encounterUid))
            {
                Logger.ErrorS("encounter", "Could not start encounter, cannot set encounter container slot");
                _mapManager.UnloadMap(encounterMap.Id);
                return;
            }

            var prevLoc = MapEntrance.FromMapCoordinates(outerMapCoords);
            encounter.PreviousLocation = prevLoc;
            _mapEntrances.SetPreviousMap(encounterMap, outerMapCoords);

            var evBefore = new EncounterBeforeMapEnteredEvent(outerMapCoords);
            RaiseEvent(encounterUid, evBefore);

            _mapTransfers.DoMapTransfer(Spatial(_gameSession.Player), encounterMap, new CenterMapLocation());

            var evAfter = new EncounterAfterMapEnteredEvent(encounterMap);
            RaiseEvent(encounterUid, evAfter);
        }

        public bool IsEncounterActive(IMap map)
        {
            return TryGetEncounter(map, out _);
        }

        public bool TryGetEncounter(IMap map, [NotNullWhen(true)] out EncounterComponent? encounter)
        {
            if (!TryComp<MapEncounterComponent>(map.MapEntityUid, out var mapEncounter)
                || !IsAlive(mapEncounter.EncounterContainer.ContainedEntity))
            {
                encounter = null;
                return false;
            }

            return TryComp(mapEncounter.EncounterContainer.ContainedEntity, out encounter);
        }

        public bool TryGetEncounter<T>(IMap map, [NotNullWhen(true)] out T? encounter)
            where T: class, IComponent
        {
            if (!TryGetEncounter(map, out var encounterComp))
            {
                encounter = default(T);
                return false;
            }

            return TryComp<T>(encounterComp.Owner, out encounter);
        }
    }

    [EventUsage(EventTarget.Encounter)]
    public sealed class EncounterGenerateMapEvent : HandledEntityEventArgs
    {
        public IMap? ResultMap { get; private set; }

        public void Handle(IMap resultMap)
        {
            Handled = true;
            ResultMap = resultMap;
        }
    }

    [EventUsage(EventTarget.Map)]
    public sealed class MapCalcRandomEncounterIDEvent : EntityEventArgs
    {
        public MapCoordinates Coords { get; }

        public PrototypeId<EntityPrototype>? OutEncounterId { get; set; }

        public MapCalcRandomEncounterIDEvent(MapCoordinates coords)
        {
            Coords = coords;
        }
    }
}