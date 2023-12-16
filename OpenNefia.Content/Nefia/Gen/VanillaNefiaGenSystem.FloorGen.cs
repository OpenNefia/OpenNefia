using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Log;
using OpenNefia.Core.Random;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Charas;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Areas;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Dungeons;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Nefia;
using Love;
using OpenNefia.Content.Web;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomGen;

namespace OpenNefia.Content.Nefia
{
    public sealed partial class VanillaNefiaGenSystem
    {
        private void SetupBaseParams(EntityUid uid, NefiaVanillaComponent component, GenerateNefiaFloorParamsEvent ev)
        {
            var baseParams = ev.BaseParams;
            var (width, height) = baseParams.MapSize;
            baseParams.MapSize = new(width, height);
            baseParams.RoomCount = width * height / 70;
            baseParams.TunnelLength = width * height;
            baseParams.MaxCharaCount = (width * height) / 100;

            var areaNefia = EntityManager.GetComponent<AreaNefiaComponent>(ev.Area.AreaEntityUid);
            baseParams.DangerLevel = AreaNefiaSystem.NefiaFloorNumberToLevel(ev.FloorNumber, areaNefia.BaseLevel);
        }

        private void GenerateFloorAttempt(EntityUid uid, NefiaVanillaComponent component, GenerateNefiaFloorAttemptEvent args)
        {
            var layout = args.Data.Get<StandardNefiaGenParams>().Layout;
            var map = layout.Generate(args.Area, args.MapId, args.GenerationAttempt, args.FloorNumber, args.Data);

            if (map != null)
                args.Handle();
        }

        private void FinalizeNefia(EntityUid uid, NefiaVanillaComponent component, AfterGenerateNefiaFloorEvent ev)
        {
            var map = ev.Map;
            var area = ev.Area;
            var floorNumber = ev.FloorNumber;

            if (ev.Data.TryGet<StandardNefiaGenParams>(out var stdParams))
            {
                stdParams.Template.AfterGenerateMap(area, map, floorNumber, ev.Data);
                stdParams.Layout.AfterGenerateMap(area, map, floorNumber, ev.Data);
            }

            var common = EntityManager.EnsureComponent<MapCommonComponent>(map.MapEntityUid);
            _mapTilesets.ApplyTileset(map, common.Tileset);

            var rooms = EntityManager.EnsureComponent<NefiaRoomsComponent>(map.MapEntityUid);

            var charaGen = EntityManager.EnsureComponent<MapCharaGenComponent>(map.MapEntityUid);
            if (charaGen.CharaFilterGen == null)
                charaGen.CharaFilterGen = new DungeonCharaFilterGen();

            Logger.DebugS("nefia.gen.floor", $"Populating {rooms.Rooms.Count} dungeon rooms.");
            PopulateRooms(map, rooms.Rooms, ev.BaseParams);

            var maxCrowdDensity = charaGen.MaxCharaCount;
            var density = new NefiaCrowdDensity(maxCrowdDensity / 4, maxCrowdDensity / 4);
            if (EntityManager.TryGetComponent<NefiaCrowdDensityModifierComponent>(map.MapEntityUid, out var modifier))
            {
                density = modifier.Modifier.Calculate(map);
            }

            AddMobsAndTraps(map, density);

            // TODO little sister
        }

        private void PopulateRooms(IMap map, List<Room> rooms, BaseNefiaGenParams baseParams)
        {
            var creaturePacks = baseParams.CreaturePacks;
            var hasMonsterHouse = false;
            var level = EntityManager.EnsureComponent<LevelComponent>(map.MapEntityUid);

            foreach (var room in rooms)
            {
                var bounds = new UIBox2i(room.Bounds.TopLeft + (1, 1), room.Bounds.BottomRight - (1, 1));
                var size = bounds.Width * bounds.Height;

                if (size <= 1)
                    continue;

                var creatureCount = _rand.Next(size / 8 + 2);

                for (var i = 0; i < creatureCount; i++)
                {
                    if (_rand.OneIn(2))
                    {
                        var pos = _rand.NextVec2iInBounds(bounds);
                        _itemGen.GenerateItem(map.AtPos(pos), 
                            minLevel: _randomGen.CalcObjectLevel(map), 
                            quality: _randomGen.CalcObjectQuality(Quality.Normal),
                            tags: new[] { RandomGenConsts.FilterSets.Dungeon(_rand, _randomGen) });
                    }

                    var charaPos = _rand.NextVec2iInBounds(bounds);
                    var chara = _charaGen.GenerateCharaFromMapFilter(map.AtPos(charaPos));
                    if (chara != null)
                    {
                        if (level.Level > 3)
                        {
                            if (EntityManager.TryGetComponent<CreaturePackComponent>(chara.Value, out var creaturePack))
                            {
                                if (_rand.OneIn(creaturePacks * 5 + 5))
                                {
                                    creaturePacks++;
                                    var creatureCount2 = 10 + _rand.Next(20);
                                    for (var j = 0; j < creatureCount2; j++)
                                    {
                                        var pos = _rand.NextVec2iInBounds(bounds);
                                        var args = new CharaGenArgs()
                                        {
                                            Category = creaturePack.Category
                                        };
                                        _charaGen.GenerateChara(map.AtPos(pos),
                                            args: EntityGenArgSet.Make(args),
                                            minLevel: _levels.GetLevel(chara.Value),
                                            quality: _randomGen.CalcObjectQuality(Quality.Normal));
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!HasStairsInRoom(map, room))
                {
                    if (!hasMonsterHouse || baseParams.CanHaveMultipleMonsterHouses)
                    {
                        if (_rand.OneIn(8) && size < 40)
                        {
                            hasMonsterHouse = true;

                            for (var ry = bounds.Top; ry < bounds.Bottom; ry++)
                            {
                                for (var rx = bounds.Left; rx < bounds.Right; rx++)
                                {
                                    _charaGen.GenerateCharaFromMapFilter(map.AtPos(rx, ry));
                                }
                            }

                            if (!baseParams.CanHaveMultipleMonsterHouses)
                            {
                                var count = _rand.Next(3);
                                for (var j = 0; j < count; j++)
                                {
                                    var pos = _rand.NextVec2iInBounds(bounds);
                                    _itemGen.GenerateItem(map.AtPos(pos), tags: new[] { Protos.Tag.ItemCatContainer });
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool HasStairsInRoom(IMap map, Room room)
        {
            return _lookup.EntityQueryInMap<SpatialComponent, StairsComponent>(map.Id)
                .Any(ent => room.Bounds.IsInBounds(ent.Item1.WorldPosition));
        }

        private void AddMobsAndTraps(IMap map, NefiaCrowdDensity density)
        {
            for (var i = 0; i < density.MobCount; i++)
            {
                _charaGen.GenerateCharaFromMapFilter(map);
            }

            for (var i = 0; i < density.ItemCount; i++)
            {
                _itemGen.GenerateItem(map, minLevel: _levels.GetLevel(map.MapEntityUid),
                    quality: Quality.Bad,
                    tags: new[] { RandomGenConsts.FilterSets.Dungeon(_rand, _randomGen) });
            }

            var trapDensity = _rand.Next(map.Width * map.Height / 80);
            for (var i = 0; i < trapDensity; i++)
            {
                PlaceTrap(map);
            }

            if (_rand.OneIn(5))
            {
                var level = EntityManager.EnsureComponent<LevelComponent>(map.MapEntityUid);

                var webDensity = _rand.Next(map.Width * map.Height / 40);
                if (_rand.OneIn(5))
                {
                    webDensity = _rand.Next(map.Width * map.Height / 5);
                }
                for (var i = 0; i < webDensity; i++)
                {
                    var difficulty = level.Level * 10 + 100;
                    PlaceWeb(map, difficulty);
                }
            }

            if (_rand.OneIn(4))
            {
                var potDensity = Math.Clamp(_rand.Next(map.Width * map.Height / 500 + 1) + 1, 3, 15);
                for (var i = 0; i < potDensity; i++)
                {
                    PlacePot(map);
                }
            }
        }

        private void PlaceTrap(IMap map, MapCoordinates? coords = null)
        {
            for (var i = 0; i < 3; i++)
            {
                MapCoordinates coords2;

                if (coords == null)
                    coords2 = map.AtPos(_rand.Next(map.Width - 5) + 2, _rand.Next(map.Height - 5) + 2);
                else
                    coords2 = coords.Value;

                if (map.IsFloor(coords2.Position) && !_targetable.TryGetBlockingEntity(coords2, out _))
                {
                    // TODO
                    _entityGen.SpawnEntity(Protos.MObj.Mine, coords2);
                    return;
                }
            }
        }

        private void PlaceWeb(IMap map, int difficulty, MapCoordinates? coords = null)
        {
            for (var i = 0; i < 3; i++)
            {
                MapCoordinates coords2;

                if (coords == null)
                    coords2 = map.AtPos(_rand.Next(map.Width - 5) + 2, _rand.Next(map.Height - 5) + 2);
                else
                    coords2 = coords.Value;

                if (map.IsFloor(coords2.Position) && !_targetable.TryGetBlockingEntity(coords2, out _))
                {
                    var entity = _entityGen.SpawnEntity(Protos.Mef.Web, coords2);
                    if (entity != null && EntityManager.TryGetComponent<WebComponent>(entity.Value, out var web))
                    {
                        web.UntangleDifficulty = difficulty;
                    }
                    return;
                }
            }
        }

        private void PlacePot(IMap map, MapCoordinates? coords = null)
        {
            for (var i = 0; i < 3; i++)
            {
                MapCoordinates? coords2;

                if (coords == null)
                    coords2 = _placement.FindFreePosition(map.AtPos(_rand.Next(map.Width - 5) + 2, _rand.Next(map.Height - 5) + 2));
                else
                    coords2 = coords;

                if (coords2 != null && map.IsFloor(coords2.Value.Position) && !_lookup.GetLiveEntitiesAtCoords(coords2.Value).Any())
                {
                    _entityGen.SpawnEntity(Protos.MObj.Pot, coords2.Value);
                    return;
                }
            }
        }
    }
}
