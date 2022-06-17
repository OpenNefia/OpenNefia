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
            baseParams.MaxCharaCount = (width * height) / 2;

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

            Logger.DebugS("nefia.gen.floor", $"Populating {rooms.Rooms.Count} dungeon rooms.");
            PopulateRooms(map, rooms.Rooms, ev.BaseParams);

            var maxCrowdDensity = common.MaxCrowdDensity;
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
                var bounds = new UIBox2i(room.Bounds.TopLeft + (1, 1), room.Bounds.BottomRight - (2, 2));
                var size = bounds.Width * bounds.Height;

                for (var i = 0; i < size / 8 + 2; i++)
                {
                    if (_rand.OneIn(2))
                    {
                        // TODO
                        var itemPos = _rand.NextVec2iInBounds(bounds);
                        _entityGen.SpawnEntity(Protos.Item.Putitoro, map.AtPos(itemPos));
                    }

                    // TODO filter

                    var charaPos = _rand.NextVec2iInBounds(bounds);
                    var chara = _entityGen.SpawnEntity(Protos.Chara.Putit, map.AtPos(charaPos));
                    if (chara != null)
                    {
                        if (level.Level > 3)
                        {
                            if (EntityManager.TryGetComponent<CreaturePackComponent>(chara.Value, out var creaturePack))
                            {
                                if (_rand.OneIn(creaturePacks * 5 + 5))
                                {
                                    creaturePacks++;
                                    var creatureCount = 10 + _rand.Next(20);
                                    for (var j = 0; j < creatureCount; j++)
                                    {
                                        // TODO
                                        var pos = _rand.NextVec2iInBounds(bounds);
                                        _entityGen.SpawnEntity(Protos.Chara.RedPutit, map.AtPos(pos));
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

                            for (var ry = bounds.Top; ry < bounds.Bottom - 1; ry++)
                            {
                                for (var rx = bounds.Left; rx < bounds.Right - 1; rx++)
                                {
                                    // TODO
                                    _entityGen.SpawnEntity(Protos.Chara.Slime, map.AtPos(rx, ry));
                                }
                            }

                            if (!baseParams.CanHaveMultipleMonsterHouses)
                            {
                                for (var j = 0; j < _rand.Next(3); j++)
                                {
                                    // TODO
                                    var pos = _rand.NextVec2iInBounds(bounds);
                                    _entityGen.SpawnEntity(Protos.Item.BejeweledChest, map.AtPos(pos));
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
                // TODO
                var pos = _rand.NextVec2iInBounds(map.Bounds);
                _entityGen.SpawnEntity(Protos.Chara.Yeek, map.AtPos(pos));
            }

            for (var i = 0; i < density.ItemCount; i++)
            {
                // TODO
                var pos = _rand.NextVec2iInBounds(map.Bounds);
                _entityGen.SpawnEntity(Protos.Item.Aloe, map.AtPos(pos));
            }

            var trapDensity = _rand.Next(map.Width * map.Height / 80);
            for (var i = 0; i < trapDensity; i++)
            {
                PlaceTrap(map);
            }

            if (_rand.OneIn(5))
            {
                var webDensity = _rand.Next(map.Width * map.Height / 40);
                if (_rand.OneIn(5))
                {
                    webDensity = _rand.Next(map.Width * map.Height / 5);
                }
                for (var i = 0; i < webDensity; i++)
                {
                    PlaceWeb(map);
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
            if (coords == null)
                coords = map.AtPos(_rand.Next(map.Width - 5) + 2, _rand.Next(map.Height - 5) + 2);

            // TODO
            _entityGen.SpawnEntity(Protos.MObj.Mine, coords.Value);
        }

        private void PlaceWeb(IMap map, MapCoordinates? coords = null)
        {
            if (coords == null)
                coords = map.AtPos(_rand.Next(map.Width - 5) + 2, _rand.Next(map.Height - 5) + 2);

            // TODO
            _entityGen.SpawnEntity(Protos.Mef.Web, coords.Value);
        }

        private void PlacePot(IMap map, MapCoordinates? coords = null)
        {
            if (coords == null)
                coords = map.AtPos(_rand.Next(map.Width - 5) + 2, _rand.Next(map.Height - 5) + 2);

            // TODO
            _entityGen.SpawnEntity(Protos.MObj.Pot, coords.Value);
        }
    }
}
