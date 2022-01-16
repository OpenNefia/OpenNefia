using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Log;
using OpenNefia.Content.Areas;

namespace OpenNefia.Content.RandomAreas
{
    /// <summary>
    /// System for randomly generating areas in a world map. This
    /// logic is part of what makes random Nefias tick. In ON, the
    /// random generation/placement logic has been decoupled from the Nefia
    /// logic, so that a more varied assortment of random areas can be generated.
    /// </summary>
    public class AreaRandomGenSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IMapRandom _mapRandom = default!;
        [Dependency] private readonly ITileDefinitionManager _tileDefs = default!;
        [Dependency] private readonly MapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IAreaEntranceSystem _areaEntrances = default!;
        [Dependency] private readonly IMessage _mes = default!;

        /// <summary>
        /// The number of active random areas that should exist in the world map at any given time.
        /// If the live number drops below this amount, then enough new random areas will be generated
        /// to fill the needed amount.
        /// </summary>
        // TODO: Make into a CVar.
        public const int ActiveRandomAreaThreshold = 25;

        public override void Initialize()
        {
            SubscribeLocalEvent<MapRandomAreaManagerComponent, MapEnteredEvent>(OnMapEntered, nameof(OnMapEntered));
        }

        private void OnMapEntered(EntityUid mapEnt, MapRandomAreaManagerComponent mapRandomAreas, MapEnteredEvent args)
        {
            if (ShouldRegenerateRandomAreas(args.NewMap.Id, mapRandomAreas))
            {
                RegenerateRandomAreas(args.NewMap.Id, mapRandomAreas);
            }
        }

        public bool ShouldRegenerateRandomAreas(MapId mapId, MapRandomAreaManagerComponent mapRandomAreas)
        {
            if (mapRandomAreas.RegenerateRandomAreas)
                return true;

            var totalActiveAreas = GetTotalActiveRandomAreasInMap(mapId);
            if (totalActiveAreas < ActiveRandomAreaThreshold)
                return true;

            if (_random.OneIn(150))
                return true;

            return false;
        }

        private void RegenerateRandomAreas(MapId mapId, MapRandomAreaManagerComponent mapRandomAreas)
        {
            mapRandomAreas.RegenerateRandomAreas = false;

            _mes.Display(Loc.GetString("Elona.RandomArea.SuddenDiastrophism"));
            DeleteRandomAreasAndEntrancesInMap(mapId);
            GenerateRandomAreas(mapId);
        }

        private void GenerateRandomAreas(MapId mapId)
        {
            var map = _mapManager.GetMap(mapId);
            var activeAreaCount = GetTotalActiveRandomAreasInMap(mapId);
            var numberToGenerate = Math.Max(0, ActiveRandomAreaThreshold - activeAreaCount);

            for (int i = 0; i < numberToGenerate; i++)
            {
                var mapCoords = FindPositionForRandomArea(map);
                if (mapCoords != null)
                {
                    var ev = new GenerateRandomAreaEvent(mapCoords.Value);
                    RaiseLocalEvent(map.MapEntityUid, ev);
                    if (ev.ResultArea != null)
                    {
                        _areaEntrances.CreateAreaEntrance(ev.ResultArea, ev.RandomAreaCoords);
                        EntityManager.EnsureComponent<AreaRandomGenComponent>(ev.ResultArea.AreaEntityUid);
                    }
                    else
                    {
                        Logger.WarningS("randomArea", $"Failed to generate random area at {ev.RandomAreaCoords}.");
                    }
                }
            }
        }

        /// <summary>
        /// Map tiles that random areas can be spawned on.
        /// </summary>
        // TODO: less hardcoding
        public static readonly IReadOnlySet<PrototypeId<TilePrototype>> RandomAreaSpawnableTiles
            = new HashSet<PrototypeId<TilePrototype>>()
        {
            Protos.Tile.WorldGrass,
            Protos.Tile.WorldSmallTrees1,
            Protos.Tile.WorldSmallTrees2,
            Protos.Tile.WorldSmallTrees3,
            Protos.Tile.WorldTrees1,
            Protos.Tile.WorldTrees2,
            Protos.Tile.WorldTrees3,
            Protos.Tile.WorldTrees4,
            Protos.Tile.WorldTrees5,
            Protos.Tile.WorldTrees6,
            Protos.Tile.WorldPlants1,
            Protos.Tile.WorldPlants2,
            Protos.Tile.WorldPlants3,
            Protos.Tile.WorldDirt1,
            Protos.Tile.WorldDirt2,
        };

        private MapCoordinates? FindPositionForRandomArea(IMap map)
        {
            var startingTile = _mapRandom.PickRandomTile(map);
            if (startingTile == null)
                return null;

            var validBounds = new UIBox2i(map.Bounds.TopLeft + (6, 6),
                                          map.Bounds.BottomRight - (6, 6));

            TileRef? PickRandomTile(int radius)
            {
                return _mapRandom.PickRandomTileInRadius(startingTile.Value, radius + 1);
            }

            bool CanPlaceRandomArea(TileRef? randTile)
            {
                if (randTile == null)
                    return false;

                var mapCoords = randTile.Value.MapPosition;
                var tileId = _tileDefs.GetPrototypeID(randTile.Value);

                if (!validBounds.Contains(mapCoords.Position))
                    return false;

                if (!RandomAreaSpawnableTiles.Contains(tileId))
                    return false;

                if (EntityBlocksWorldEntrancesAt(randTile.Value.MapPosition))
                    return false;

                if (PositionTooCloseToRandomArea(mapCoords))
                    return false;

                return true;
            }

            return Enumerable.Range(0, 1000)
                .Select(PickRandomTile)
                .Where(CanPlaceRandomArea)
                .FirstOrDefault()?.MapPosition;
        }

        private bool EntityBlocksWorldEntrancesAt(MapCoordinates mapPosition)
        {
            return _lookup.GetLiveEntitiesAtCoords(mapPosition)
                .Any(spatial => spatial.IsSolid || EntityManager.HasComponent<WorldMapEntranceComponent>(spatial.Owner));
        }

        private static UIBox2i Box2iCenteredAt(Vector2i worldPosition, int radius)
        {
            return new UIBox2i(worldPosition - (radius, radius), worldPosition + (radius, radius));
        }

        private bool PositionTooCloseToRandomArea(MapCoordinates mapCoords)
        {
            bool TooClose((WorldMapEntranceComponent, IArea) pair)
            {
                var worldEntrance = pair.Item1;
                var spatial = EntityManager.GetComponent<SpatialComponent>(worldEntrance.Owner);
                var bounds = Box2iCenteredAt(spatial.WorldPosition, 2);
                return bounds.Contains(mapCoords.Position);
            }

            return EnumerateRandomMapEntrancesIn(mapCoords.MapId).Any(TooClose);
        }

        /// <summary>
        /// Calculates how many world map entrances to random areas in this map
        /// area are still active.
        /// </summary>
        public int GetTotalActiveRandomAreasInMap(MapId mapId)
        {
            var totalActive = 0;

            foreach (var (entrance, area) in EnumerateRandomMapEntrancesIn(mapId).ToList())
            {
                if(IsRandomAreaActive(area))
                    totalActive++;
            }

            return totalActive;
        }

        public bool IsRandomAreaActive(IArea area)
        {
            var ev = new RandomAreaCheckIsActiveEvent();
            ev.IsActive = false;
            RaiseLocalEvent(area.AreaEntityUid, ev);
            return ev.IsActive;
        }

        private void DeleteRandomAreasAndEntrancesInMap(MapId mapId)
        {
            foreach (var (entrance, area) in EnumerateRandomMapEntrancesIn(mapId).ToList())
            {
                _areaManager.DeleteArea(area.Id);
                EntityManager.DeleteEntity(entrance.Owner);
            }
        }

        public bool IsRandomAreaEntrance(WorldMapEntranceComponent entrance, [NotNullWhen(true)] out IArea? area)
        {
            if (!_mapEntrances.TryGetAreaOfEntrance(entrance.Entrance, out area))
                return false;

            return EntityManager.HasComponent<AreaRandomGenComponent>(area.AreaEntityUid);
        }

        private IEnumerable<(WorldMapEntranceComponent, IArea)> EnumerateRandomMapEntrancesIn(MapId mapId)
        {
            foreach (var entrance in _lookup.EntityQueryInMap<WorldMapEntranceComponent>(mapId))
            {
                if (IsRandomAreaEntrance(entrance, out var area))
                    yield return (entrance, area);
            }
        }
    }

    /// <summary>
    /// This event is used to see if a random area is "dead" and can be cleaned up
    /// by the random area manager. Examples include Nefias that have been conquered
    /// by the player.
    /// </summary>
    public sealed class RandomAreaCheckIsActiveEvent : EntityEventArgs
    {
        /// <summary>
        /// If true, this area shouldn't be cleaned up by the random area manager.
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Fired when the current map is being regenerated according to world map rules.
    /// This is used to regenerate global area entrances and to regenerate random areas
    /// like Nefia.
    /// </summary>
    public sealed class WorldMapRegeneratingEvent : EntityEventArgs
    {
    }

    public sealed class GenerateRandomAreaEvent : HandledEntityEventArgs
    {
        public MapCoordinates RandomAreaCoords { get; }

        public IArea? ResultArea { get; private set; }

        public GenerateRandomAreaEvent(MapCoordinates randomAreaPos)
        {
            RandomAreaCoords = randomAreaPos;
        }

        public void Handle(IArea resultArea)
        {
            Handled = true;
            ResultArea = resultArea;
        }
    }
}
