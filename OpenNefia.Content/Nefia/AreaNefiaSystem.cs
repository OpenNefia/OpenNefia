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
        [Dependency] private readonly ITagSystem _tags = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<AreaNefiaComponent, AreaGeneratedEvent>(OnNefiaGenerated, nameof(OnNefiaGenerated));
            SubscribeLocalEvent<AreaNefiaComponent, RandomAreaCheckIsActiveEvent>(OnCheckIsActive, nameof(OnCheckIsActive));
            SubscribeLocalEvent<GenerateRandomAreaEvent>(GenerateRandomNefia, nameof(GenerateRandomNefia));

            // TODO: temporary for now.
            SubscribeLocalEvent<AreaNefiaComponent, AreaFloorGenerateEvent>(OnNefiaFloorGenerate, nameof(OnNefiaFloorGenerate));
        }

        private void OnCheckIsActive(EntityUid uid, AreaNefiaComponent areaNefia, RandomAreaCheckIsActiveEvent args)
        {
            args.IsActive |= areaNefia.State == NefiaState.Unvisited || areaNefia.State == NefiaState.Visited;
        }

        private IMap GenerateFallbackMap(AreaFloorGenerateEvent args)
        {
            var map = _mapManager.CreateMap(20, 20);

            map.Clear(Protos.Tile.Dirt);
            foreach (var pos in EnumerateBorder(map.Bounds))
            {
                map.SetTile(pos, Protos.Tile.WallDirt);
            }

            var stairs = _entityGen.SpawnEntity(Protos.MObj.StairsUp, map.AtPos(3, 3))!.Value;
            var stairsComp = EntityManager.EnsureComponent<StairsComponent>(stairs);
            stairsComp.Entrance = MapEntrance.FromMapCoordinates(args.PreviousCoords);

            var tagComp = EntityManager.EnsureComponent<TagComponent>(stairs);
            tagComp.AddTag(Protos.Tag.DungeonStairsSurfacing);

            return map;
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

            var areaMetaData = EntityManager.EnsureComponent<MetaDataComponent>(areaEntity);

            // TODO make display names into events instead of mutating
            var baseName = EntityManager.GetComponent<MetaDataComponent>(areaEntity).DisplayName ?? "<???>";
            areaMetaData.DisplayName = PickRandomNefiaName(baseName, baseLevel);

            areaNefia.BaseLevel = baseLevel;

            var areaDungeonComp = EntityManager.EnsureComponent<AreaDungeonComponent>(areaEntity);
            areaDungeonComp.DeepestFloor = floorCount;

            // TODO: temporary until dungeon logic is in place.
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

        private readonly LocaleKey[] NefiaNameTypes =
        {
            "TypeA",
            "TypeB"
        };

        /// <remarks>
        /// TODO: Rewrite this in terms of <see cref="DisplayName.DisplayNameSystem"/>!
        /// To support language switching, <see cref="MetaDataComponent.DisplayName"/> should never 
        /// be set manually!
        /// </remarks>
        private string PickRandomNefiaName(string baseName, int baseLevel)
        {
            var rankFactor = 5;
            var type = _random.Pick(NefiaNameTypes);
            var rank = Math.Clamp(baseLevel / rankFactor, 0, 4);
            return _loc.GetString($"Elona.Nefia.Prefixes.{type}.Rank{rank}", ("baseName", baseName));
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

        private void OnNefiaFloorGenerate(EntityUid areaEnt, AreaNefiaComponent areaNefia, AreaFloorGenerateEvent args)
        {
            if (args.Handled)
                return;

            //var ev = new GenerateNefiaFloorEvent(args.Area, args.FloorId, args.PreviousCoords);
            //RaiseLocalEvent(areaEnt, ev);

            var mapId = _mapManager.GenerateMapId();
            var floorNumber = AreaIdToFloorNumber(args.FloorId);
            var generator = new NefiaFloorGenerator();
            
            if (!generator.TryToGenerate(args.Area, mapId, floorNumber, out var map))
            {
                Logger.ErrorS("nefia.gen", $"Failed to generate Nefia floor!");
                return;
            }

            if (!ConnectStairs(map, args.Area, floorNumber, args.PreviousCoords))
            {
                Logger.ErrorS("nefia.gen", $"Failed to connect stairs!");
                return;
            }

            args.Handle(map);
        }

        private bool ConnectStairs(IMap map, IArea area, int floorNumber, MapCoordinates previousCoords)
        {
            // Note: Delving always means "closer to the boss level" regardless of stairs up/down
            var delving = _tags.EntityWithTagInMap(map.Id, Protos.Tag.DungeonStairsDelving);
            if (delving != null)
            {
                var areaDungeon = EntityManager.GetComponent<AreaDungeonComponent>(area.AreaEntityUid);

                if (floorNumber >= areaDungeon.DeepestFloor)
                {
                    EntityManager.DeleteEntity(delving.Owner);
                }
                else
                {
                    var nextFloorId = FloorNumberToAreaId(floorNumber + 1);
                    var stairs = EntityManager.GetComponent<StairsComponent>(delving.Owner);
                    stairs.Entrance = new MapEntrance()
                    {
                        MapIdSpecifier = new AreaFloorMapIdSpecifier(area.Id, nextFloorId),
                        StartLocation = new TaggedEntityMapLocation(Protos.Tag.DungeonStairsSurfacing)
                    };

                    Logger.InfoS("nefia.gen.floor", $"Delving stairs to: {nextFloorId}");
                }
            }
            else
            {
                Logger.WarningS("nefia.gen.floor", "No stairs delving in dungeon.");
            }

            var surfacing = _tags.EntityWithTagInMap(map.Id, Protos.Tag.DungeonStairsSurfacing);
            if (surfacing != null)
            {
                var stairs = EntityManager.GetComponent<StairsComponent>(surfacing.Owner);

                if (floorNumber <= 1)
                {
                    if (!_areaManager.TryGetParentArea(area.Id, out var parentArea))
                    {
                        Logger.ErrorS("nefia.gen.floor", "No parent area to surface to!");
                    }
                    else
                    {
                        // TODO for better precision, the entity UID of the entrance needs to be passed here.
                        // but MapEntrance doesn't attach any entity information.
                        stairs.Entrance = MapEntrance.FromMapCoordinates(previousCoords);

                        Logger.InfoS("nefia.gen.floor", $"Surfacing stairs to coords: {previousCoords}");
                    }
                }
                else
                {
                    var prevFloorId = FloorNumberToAreaId(floorNumber - 1);
                    stairs.Entrance = new MapEntrance()
                    {
                        MapIdSpecifier = new AreaFloorMapIdSpecifier(area.Id, prevFloorId),
                        StartLocation = new TaggedEntityMapLocation(Protos.Tag.DungeonStairsDelving)
                    };

                    Logger.InfoS("nefia.gen.floor", $"Surfacing stairs to: {prevFloorId}");
                }
            }
            else
            {
                // Weird, we should always have surfacing stairs.
                Logger.ErrorS("nefia.gen.floor", "No stairs surfacing in dungeon.");
                return false;
            }

            return true;
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
}
