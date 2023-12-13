using OpenNefia.Content.Maps;
using OpenNefia.Content.Nefia;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Nefia.Layout;
using OpenNefia.Core.Random;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Items;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Currency;
using OpenNefia.Content.Pickable;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Content.Charas;
using OpenNefia.Content.GameObjects;
using Love;
using YamlDotNet.RepresentationModel;
using System.Text;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Levels;
using OpenNefia.Content.FieldMap;
using OpenNefia.Content.Weight;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMapTilesetSystem _mapTilesets = default!;
        [Dependency] private readonly IVanillaNefiaGenSystem _vanillaNefiaGen = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;

        private void Initialize_QuestMapGenEvents()
        {
            SubscribeComponent<MapDerivedForQuestComponent, MapCreatedFromBlueprintEvent>(RaiseDerivedMapEvents);
        }

        int ItemCountAt(MapCoordinates coords)
        {
            return _lookup.QueryLiveEntitiesAtCoords<ItemComponent>(coords).Count();
        }

#pragma warning disable format
        // >>>>>>>> elona122/shade2/map_rand.hsp:645  ..
        private static readonly PrototypeId<EntityPrototype>[][] PartyRoomCharaTypes =
        {
            new[] { Protos.Chara.CitizenOfCyberDome, Protos.Chara.Beggar,      Protos.Chara.OldPerson       },
            new[] { Protos.Chara.Farmer,             Protos.Chara.Miner,       Protos.Chara.TownChild       },
            new[] { Protos.Chara.Farmer,             Protos.Chara.Punk,        Protos.Chara.Mercenary       },
            new[] { Protos.Chara.Punk,               Protos.Chara.Citizen,     Protos.Chara.HotSpringManiac },
            new[] { Protos.Chara.Citizen,            Protos.Chara.Tourist,     Protos.Chara.Wizard          },
            new[] { Protos.Chara.Tourist,            Protos.Chara.Noble,       Protos.Chara.NobleChild      },
            new[] { Protos.Chara.Noble,              Protos.Chara.Artist,      Protos.Chara.Bartender       },
            new[] { Protos.Chara.Artist,             Protos.Chara.Elder,       Protos.Chara.Shopkeeper      },
            new[] { Protos.Chara.Elder,              Protos.Chara.Nun,         Protos.Chara.Captain         },
            new[] { Protos.Chara.Nun,                Protos.Chara.ArenaMaster, Protos.Chara.Informer        }
        };
        // <<<<<<<< elona122/shade2/map_rand.hsp:656  ..

        // >>>>>>>> elona122/shade2/map_rand.hsp:683 	flt:p=217,218,216,215,215,152,152,91,189:item_cre ..
        private static readonly PrototypeId<EntityPrototype>[] PartyRoomItemTypes =
        {
            Protos.Item.EmptyBowl,
            Protos.Item.Bowl,
            Protos.Item.Basket,
            Protos.Item.LotOfEmptyBottles,
            Protos.Item.LotOfEmptyBottles,
            Protos.Item.LotOfAlcohols,
            Protos.Item.LotOfAlcohols,
            Protos.Item.Barrel,
            Protos.Item.StackOfDishes
        };
        // <<<<<<<< elona122/shade2/map_rand.hsp:683 	flt:p=217,218,216,215,215,152,152,91,189:item_cre ..
#pragma warning restore format

        private IMap QuestMap_GenerateHuntBaseMap(PrototypeId<MapTilesetPrototype>? tileset)
        {
            var map = _mapManager.CreateMap(28 + _rand.Next(6), 20 + _rand.Next(6), Protos.Map.QuestHunt);
            map.Clear(Protos.Tile.MapgenRoom);

            var wallCount = _rand.Next((map.Width * map.Height) / 30);
            for (var i = 0; i < wallCount; i++)
            {
                var point = _rand.NextVec2iInBounds(map.Bounds);
                map.SetTile(point, Protos.Tile.MapgenWall);
            }

            if (tileset != null)
                EnsureComp<MapCommonComponent>(map.MapEntityUid).Tileset = tileset.Value;

            _mapTilesets.ApplyTileset(map);

            return map;
        }

        /// <summary>
        /// Generates the map for hunt quests.
        /// </summary>
        /// <param name="difficulty">Difficulty of the quest. Controls the level of wild monsters.</param>
        /// <returns></returns>
        private IMap QuestMap_GenerateHunt(int difficulty, PrototypeId<MapTilesetPrototype>? tileset)
        {
            // >>>>>>>> shade2/map_rand.hsp:117 		if gQuest=qHunt{ ..

            // NOTE: in vanilla this uses the nefia generation engine
            // but currently the new nefia code is too confusing, so here it's
            // just generated directly until nefia generation is reworked

            // >>>>>>>> shade2/map_rand.hsp:305 *map_createDungeonHunt ..
            var map = QuestMap_GenerateHuntBaseMap(tileset);

            var mapCharaGen = EnsureComp<MapCharaGenComponent>(map.MapEntityUid);
            mapCharaGen.MaxCharaCount = 0;
            mapCharaGen.CharaFilterGen = new QuestHuntCharaFilterGen(difficulty);
            var huntCharaCount = 10 + _rand.Next(6);
            for (var i = 0; i < huntCharaCount; i++)
            {
                var chara = _charaGen.GenerateCharaFromMapFilter(map);
                if (IsAlive(chara))
                {
                    EnsureComp<FactionComponent>(chara.Value).RelationToPlayer = Relation.Enemy;
                    EnsureComp<TargetForEliminateQuestComponent>(chara.Value).Tag = QuestHuntTargetTagId;
                }
            }

            var huntTreeCount = 10 + _rand.Next(6);
            for (var i = 0; i < huntTreeCount; i++)
            {
                var point = _rand.NextVec2iInBounds(map.Bounds);
                var coords = map.AtPos(point);
                var item = _itemGen.GenerateItem(coords, tags: new[] { Protos.Tag.ItemCatTree });
                if (IsAlive(item))
                    EnsureComp<PickableComponent>(item.Value).OwnState = OwnState.NPC;
            }

            return map;
            // <<<<<<<< shade2/map_rand.hsp:331 	return true ..

            // <<<<<<<< shade2/map_rand.hsp:122 			} ..
        }

        private void RaiseDerivedMapEvents(EntityUid uid, MapDerivedForQuestComponent component, MapCreatedFromBlueprintEvent args)
        {
            Logger.DebugS("quest.mapgen", $"Map {args.Map} was created for conquer/huntEX/etc., running events");

            var map = args.Map;
            var ev = new AfterDerivedQuestMapGeneratedEvent(map);
            RaiseEvent(map.MapEntityUid, ev);

            foreach (var entity in _lookup.EntityQueryInMap<SpatialComponent>(map, includeChildren: true, includeDead: true))
            {
                var ev2 = new InitEntityInDerivedQuestMapEvent(map);
                RaiseEvent(entity.Owner, ev2);
            }
        }

        /// <summary>
        /// Generates and cleans up a map from a blueprint for use with the conquer and huntEX quests.
        /// </summary>
        /// <param name="mapBlueprintPath"></param>
        /// <returns></returns>
        private IMap? QuestMap_GenerateDerivedHuntMap(ResourcePath mapBlueprintPath, PrototypeId<EntityPrototype> mapEntityProto)
        {
            // TODO: YAML transformations will make this easier.
            bool IsMapEntity(MapBlueprintEntity entity)
            {
                return entity.HasComponent<MapComponent>(_protos);
            }

            bool CanKeepBlueprintEntity(MapBlueprintEntity entity)
            {
                return !entity.HasComponent<CharaComponent>(_protos)
                    && !entity.HasComponent<MObjComponent>(_protos);
            }

            var blueprintYaml = _resourceCache.ContentFileReadYaml(mapBlueprintPath);
            var rootNode = blueprintYaml.Documents[0].RootNode.ToDataNodeCast<MappingDataNode>();
            var blueprint = _serialization.Read<MapBlueprint>(rootNode);
            var mapEntity = blueprint.Entities.FirstOrDefault(IsMapEntity);

            if (mapEntity == null)
            {
                Logger.ErrorS("quest.huntex", $"Map blueprint {mapBlueprintPath} did not contain a map entity.");
                return null;
            }

            // Overwrite the map's prototype/components with the quest's.
            mapEntity.ProtoId = mapEntityProto;
            mapEntity.Components.Clear();

            // Clear characters and mobjs.
            foreach (var entity in blueprint.Entities.ToList())
            {
                if (!CanKeepBlueprintEntity(entity))
                {
                    blueprint.Entities.Remove(entity);
                }
            }

            // I dunno...
            var newYaml = _serialization.WriteValue(blueprint, alwaysWrite: true);
            var document = new YamlDocument(newYaml.ToYamlNode());
            var yamlStream = new YamlStream(document);

            // The blueprint should have MapDerivedForQuestComponent so some extra
            // events can be run modifying wells and safes to prevent powergaming.
            var map = _mapLoader.LoadBlueprint(yamlStream);

            return map;
        }

        /// <summary>
        /// Generates the map for hunt quests.
        /// </summary>
        /// <param name="questDifficulty">Difficulty of the quest. Controls the level of wild monsters.</param>
        /// <returns></returns>
        private IMap QuestMap_GenerateHuntEX(IMap originMap, PrototypeId<EntityPrototype> enemyID, int enemyLevel, int questDifficulty)
        {
            IMap? map = null;

            // These maps will reuse the geometry of the origin map (town) for the quest itself,
            // so it looks like the player is hunting monsters in the town.
            // To do this a MapRenewGeometryComponent is required so the initial geometry is known.
            if (TryComp<MapRenewGeometryComponent>(originMap.MapEntityUid, out var mapRenewGeometry))
            {
                map = QuestMap_GenerateDerivedHuntMap(mapRenewGeometry.MapBlueprintPath, Protos.Map.QuestHuntEX);
            }

            if (map == null)
            {
                Logger.ErrorS("quest.huntex", $"Quest origin map did not have a {nameof(MapRenewGeometryComponent)} for use with the huntEX quest, defaulting to a fallback map.");
                map = QuestMap_GenerateHuntBaseMap(null);
            }

            // >>>>>>>> shade2/map_rand.hsp:722 	if gQuest=qHuntEx{ ..
            var count = _rand.Next(6) + 4;
            for (var i = 0; i < count; i++)
            {
                var filter = new CharaFilter()
                {
                    MinLevel = (int)(questDifficulty * 1.5),
                    LevelOverride = enemyLevel,
                    Quality = Quality.Bad,
                    Id = enemyID
                };
                var chara = _charaGen.GenerateChara(map, filter);
                if (IsAlive(chara))
                {
                    EnsureComp<FactionComponent>(chara.Value).RelationToPlayer = Relation.Enemy;
                    EnsureComp<TargetForEliminateQuestComponent>(chara.Value).Tag = QuestHuntEXTargetTagId;
                    EnsureComp<LevelComponent>(chara.Value).ShowLevelInName = true;
                }
            }
            // <<<<<<<< shade2/map_rand.hsp:726 		} ..

            return map;
        }

        /// <summary>
        /// Generates the map for hunt quests.
        /// </summary>
        /// <param name="questDifficulty">Difficulty of the quest. Controls the level of wild monsters.</param>
        /// <returns></returns>
        private IMap QuestMap_GenerateConquer(IMap originMap, PrototypeId<EntityPrototype> enemyID, int enemyLevel, int questDifficulty, EntityUid questUid)
        {
            IMap? map = null;

            if (TryComp<MapRenewGeometryComponent>(originMap.MapEntityUid, out var mapRenewGeometry))
            {
                map = QuestMap_GenerateDerivedHuntMap(mapRenewGeometry.MapBlueprintPath, Protos.Map.QuestConquer);
            }

            if (map == null)
            {
                Logger.ErrorS("quest.conquer", $"Quest origin map did not have a {nameof(MapRenewGeometryComponent)} for use with the conquer quest, defaulting to a fallback map.");
                map = QuestMap_GenerateHuntBaseMap(null);
            }

            // >>>>>>>> shade2/map_rand.hsp:718 	if gQuest=qConquer{ ..
            var filter = new CharaFilter()
            {
                MinLevel = questDifficulty,
                LevelOverride = enemyLevel,
                Quality = Quality.God,
                Id = enemyID
            };
            var chara = _charaGen.GenerateChara(map, filter);
            if (IsAlive(chara))
            {
                EnsureComp<FactionComponent>(chara.Value).RelationToPlayer = Relation.Enemy;
                EnsureComp<LevelComponent>(chara.Value).ShowLevelInName = true;
                EnsureComp<ConquerQuestTargetComponent>(chara.Value).QuestUid = questUid;
            }
            // <<<<<<<< shade2/map_rand.hsp:721 		} ..

            return map;
        }

        /// <summary>
        /// Generates the map for party quests.
        /// </summary>
        /// <param name="difficulty">Difficulty of the quest. Controls the level of inebriated partygoers.</param>
        /// <returns></returns>
        private IMap QuestMap_GenerateParty(int difficulty)
        {
            // >>>>>>>> elona122/shade2/map_rand.hsp:584 *map_createDungeonPerform ..
            var map = _mapManager.CreateMap(38, 28, Protos.Map.QuestParty);

            var roomGenCount = 80;

            map.Clear(Protos.Tile.MapgenTunnel);
            foreach (var pos in MapGenUtils.EnumerateBorder(map.Bounds))
            {
                map.SetTile(pos, Protos.Tile.MapgenWall);
            }

            var rooms = new List<Room>();
            for (var i = 0; i < roomGenCount; i++)
            {
                if (rooms.Count > roomGenCount)
                    break;

                _nefiaLayout.TryDigRoom(map, rooms, RoomType.Inner, 5, 5, out _);
            }

            var roomsComp = EnsureComp<NefiaRoomsComponent>(map.MapEntityUid);
            roomsComp.Rooms.AddRange(rooms);

            for (var i = 0; i < 500; i++)
            {
                var point = _rand.NextVec2iInVec(map.Size - (5, 5));
                var spaceForDecor = true;
                var spaceForTable = true;

                foreach (var point2 in MapGenUtils.EnumerateBounds(UIBox2i.FromDimensions(point, (5, 5))))
                {
                    var tile = map.GetTileID(point2);
                    if (tile == null)
                        continue;

                    var itemCount = ItemCountAt(map.AtPos(point2));

                    if (tile.Value != Protos.Tile.MapgenTunnel || itemCount > 0)
                        spaceForDecor = false;
                    if (tile.Value != Protos.Tile.MapgenRoom || itemCount > 0)
                        spaceForTable = false;
                }

                if (spaceForDecor)
                    Party_GenerateDecor(map, point);

                if (spaceForTable)
                    Party_GenerateTable(map, point);
            }

            foreach (var room in roomsComp.Rooms)
                Party_GenerateRoomGuests(difficulty, map, room);

            Party_GenerateRandomItems(map);
            Party_GenerateSpecialCharas(map);

            _mapTilesets.ApplyTileset(map);

            foreach (var pickable in _lookup.EntityQueryInMap<PickableComponent>(map))
            {
                pickable.OwnState = OwnState.NPC;
            }

            return map;
            // <<<<<<<< elona122/shade2/map_rand.hsp:703 	return true ..
        }

        private void Party_GenerateSpecialCharas(IMap map)
        {
            Party_GenerateSpecialChara(map, Protos.Chara.Loyter);
            Party_GenerateSpecialChara(map, Protos.Chara.Gilbert);
            Party_GenerateSpecialChara(map, Protos.Chara.Shena);
            Party_GenerateSpecialChara(map, Protos.Chara.Mia);

            if (_rand.OneIn(10))
                Party_GenerateSpecialChara(map, Protos.Chara.Lomias);
            if (_rand.OneIn(10))
                Party_GenerateSpecialChara(map, Protos.Chara.WhomDwellInTheVanity);
            if (_rand.OneIn(10))
                Party_GenerateSpecialChara(map, Protos.Chara.Raphael);
            if (_rand.OneIn(10))
                Party_GenerateSpecialChara(map, Protos.Chara.Renton);
            if (_rand.OneIn(10))
                Party_GenerateSpecialChara(map, Protos.Chara.StrangeScientist);
        }

        private void Party_GenerateSpecialChara(IMap map, PrototypeId<EntityPrototype> id)
        {
            var chara = _charaGen.GenerateChara(map, id);
            if (!IsAlive(chara))
                return;

            EnsureComp<RoleSpecialComponent>(chara.Value);
            EnsureComp<FactionComponent>(chara.Value).RelationToPlayer = Relation.Dislike;
        }

        private void Party_GenerateRandomItems(IMap map)
        {
            var itemCount = 25 + _rand.Next(10);
            for (var i = 0; i < itemCount; i++)
            {
                var point = _rand.NextVec2iInBounds(map.Bounds);
                var coords = map.AtPos(point);
                if (ItemCountAt(coords) == 0 && map.CanAccess(coords))
                {
                    _itemGen.GenerateItem(coords, _rand.Pick(PartyRoomItemTypes));
                }
            }
        }

        private void Party_GenerateRoomGuests(int difficulty, IMap map, Room room)
        {
            var bounds = new UIBox2i(room.Bounds.TopLeft + (1, 1), room.Bounds.BottomRight - (1, 1));
            var roomSize = bounds.Width * bounds.Height;
            var roomDifficulty = Math.Clamp(_rand.Next(difficulty / 3 + 3), 0, PartyRoomCharaTypes.Length);

            if (_rand.OneIn(2))
                PlaceItemInRoom(Protos.Item.GrandPiano, map, room);
            if (_rand.OneIn(3))
                PlaceItemInRoom(Protos.Item.CasinoTable, map, room);
            if (_rand.OneIn(2))
                PlaceItemInRoom(Protos.Item.NarrowDiningTable, map, room);
            if (_rand.OneIn(3))
                PlaceItemInRoom(Protos.Item.BarbecueSet, map, room);

            var count = _rand.Next(roomSize / 5 + 2) + roomSize / 5 + 2;
            for (var i = 0; i < count; i++)
            {
                var filter = new CharaFilter()
                {
                    MinLevel = roomDifficulty * 5,
                    Quality = _randomGen.CalcObjectQuality(Quality.Normal),
                    LevelOverride = roomDifficulty * 7 + _rand.Next(5),
                    Id = _rand.Pick(PartyRoomCharaTypes[roomDifficulty])
                };

                var point2 = _rand.NextVec2iInBounds(room.Bounds);
                var chara = _charaGen.GenerateChara(map.AtPos(point2), filter);
                if (IsAlive(chara))
                {
                    EnsureComp<RoleSpecialComponent>(chara.Value);
                    EnsureComp<FactionComponent>(chara.Value).RelationToPlayer = Relation.Dislike;
                    EnsureComp<MoneyComponent>(chara.Value).Gold = _levels.GetLevel(chara.Value) * (20 + _rand.Next(20));
                }
            }
        }

        void PlaceItemInRoom(PrototypeId<EntityPrototype> id, IMap map, Room room)
        {
            var point = _rand.NextVec2iInBounds(room.Bounds);
            var coords = map.AtPos(point);
            if (ItemCountAt(coords) == 0)
                _itemGen.GenerateItem(coords, id);
        }

        private void Party_GenerateTable(IMap map, Vector2i point)
        {
            _itemGen.GenerateItem(map.AtPos(point + (1, 1)), Protos.Item.ModernTable);
            if (_rand.OneIn(2))
                _itemGen.GenerateItem(map.AtPos(point + (1, 0)), Protos.Item.ModernChair);
            if (_rand.OneIn(2))
                _itemGen.GenerateItem(map.AtPos(point + (1, 2)), Protos.Item.ModernChair);
            if (_rand.OneIn(2))
                _itemGen.GenerateItem(map.AtPos(point + (0, 1)), Protos.Item.ModernChair);
            if (_rand.OneIn(2))
                _itemGen.GenerateItem(map.AtPos(point + (2, 1)), Protos.Item.ModernChair);
        }

        private void Party_GenerateDecor(IMap map, Vector2i point)
        {
            var choice = _rand.Next(5);

            for (var j = 0; j < 4; j++)
            {
                for (var i = 0; i < 4; i++)
                {
                    var genPos = point + (i, j);
                    switch (choice)
                    {
                        case 0:
                        case 1:
                            if (i != 0 && i != 3 && j != 0 && j != 3)
                                map.SetTile(genPos, Protos.Tile.WallConcreteLight);
                            break;
                        case 2:
                            if (i == 3 || j == 3)
                                break;
                            if (i == 1 && j == 1)
                            {
                                map.SetTile(genPos, Protos.Tile.StackedCratesGreen);
                            }
                            else
                            {
                                map.SetTile(genPos, Protos.Tile.WoodFloor2);
                                _itemGen.GenerateItem(map.AtPos(genPos), Protos.Item.Barrel);
                            }
                            break;
                        case 3:
                            if (i == 1 && j == 1)
                            {
                                map.SetTile(genPos, Protos.Tile.CarpetBlue);
                                _itemGen.GenerateItem(map.AtPos(genPos), Protos.Item.FancyLamp);
                            }
                            break;
                        case 4:
                            if (i == 1 && j == 1)
                            {
                                map.SetTile(genPos, Protos.Tile.BallroomRoomFloor);
                                _itemGen.GenerateItem(map.AtPos(genPos), Protos.Item.StatueOrnamentedWithPlants);
                            }
                            break;
                    }
                }
            }
        }

        private IMap QuestMap_GenerateHarvest(int difficulty)
        {
            // >>>>>>>> shade2/map_rand.hsp:337 	mField=mFieldOutdoor ..
            var map = _mapManager.CreateMap(50 + _rand.Next(16), 50 + _rand.Next(16), Protos.Map.QuestHarvest);
            map.Clear(Protos.Tile.MapgenDefault);

            FieldMapGenerator.SprayTile(map, Protos.Tile.GrassBush3, 10, _rand);
            FieldMapGenerator.SprayTile(map, Protos.Tile.GrassPatch3, 10, _rand);
            FieldMapGenerator.SprayTile(map, Protos.Tile.Grass, 30, _rand);
            FieldMapGenerator.SprayTile(map, Protos.Tile.GrassViolets, 4, _rand);
            FieldMapGenerator.SprayTile(map, Protos.Tile.GrassTall2, 2, _rand);
            FieldMapGenerator.SprayTile(map, Protos.Tile.GrassTall1, 2, _rand);
            FieldMapGenerator.SprayTile(map, Protos.Tile.GrassTall2, 2, _rand);
            FieldMapGenerator.SprayTile(map, Protos.Tile.GrassPatch1, 2, _rand);

            _mapTilesets.ApplyTileset(map);

            EnsureComp<MapCharaGenComponent>(map.MapEntityUid).CharaFilterGen = new QuestHarvestCharaFilterGen(difficulty);

            for (var i = 0; i < 30; i++)
            {
                var width = _rand.Next(5) + 5;
                var height = _rand.Next(4) + 4;
                var point = _rand.NextVec2iInBounds(map.Bounds);

                var tile = Protos.Tile.Field1;
                if (_rand.OneIn(2))
                    tile = Protos.Tile.Field2;

                var size = int.Clamp((int)(point - (map.Size / 2)).Length / 8, 0, 8);
                var cropItemId = _rand.Pick(RandomGenConsts.ItemSets.Crop);

                for (var y = point.Y; y < point.Y + height - 1; y++)
                {
                    if (y >= map.Height)
                        break;

                    for (var x = point.X; x < point.X + width - 1; x++)
                    {
                        if (y >= map.Width)
                            break;

                        var coords = new Vector2i(x, y);
                        map.SetTile(coords, tile);

                        if (_rand.OneIn(10) && ItemCountAt(map.AtPos(coords)) == 0)
                        {
                            PrototypeId<EntityPrototype> itemId;
                            if (_rand.OneIn(4))
                                itemId = cropItemId;
                            else
                                itemId = _rand.Pick(RandomGenConsts.ItemSets.Crop);

                            var crop = _itemGen.GenerateItem(map.AtPos(coords), itemId);
                            if (IsAlive(crop))
                            {
                                EnsureComp<PickableComponent>(crop.Value).OwnState = OwnState.Quest;
                                var weight = int.Clamp(size + _rand.Next(_rand.Next(4) + 1), 0, 9);
                                EnsureComp<WeightComponent>(crop.Value).Weight.Base *= (int)((80 + weight * weight * 100) / 100);
                                EnsureComp<HarvestQuestCropComponent>(crop.Value).WeightClass = weight;
                            }
                        }
                    }
                }
            }

            var itemCount = 70 + _rand.Next(20);
            for (var i = 0; i < itemCount; i++)
            {
                var point = _rand.NextVec2iInBounds(map.Bounds);
                var tileID = map.GetTileID(point);
                if (tileID != Protos.Tile.Field1 && tileID != Protos.Tile.Field2 && ItemCountAt(map.AtPos(point)) == 0) 
                {
                    if (_rand.OneIn(8))
                    {
                        var tree = _itemGen.GenerateItem(map.AtPos(point), tags: new[] { Protos.Tag.ItemCatTree });
                        if (IsAlive(tree))
                            EnsureComp<PickableComponent>(tree.Value).OwnState = OwnState.NPC;
                    }
                    else
                    {
                        _entityGen.SpawnEntity(Protos.MObj.Pot, map.AtPos(point));
                    }
                }
            }

            for (var i = 0; i < 30; i++)
            {
                _charaGen.GenerateCharaFromMapFilter(map);
            }

            return map;
            // <<<<<<<< shade2/map_rand.hsp:393 	return true ..
        }
    }

    public class QuestHuntCharaFilterGen : IMapCharaFilterGen
    {
        [Dependency] private readonly IMapImmediateQuestSystem _immQuests = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;

        public QuestHuntCharaFilterGen() { }

        public QuestHuntCharaFilterGen(int difficulty)
        {
            Difficulty = difficulty;
        }

        [DataField]
        public int? Difficulty { get; set; } = null;

        public CharaFilter GenerateFilter(IMap map)
        {
            // >>>>>>>> shade2/map.hsp:70 	if gArea=areaQuest{ ..
            var questDifficulty = Difficulty;
            if (questDifficulty == null && _immQuests.TryGetImmediateQuest(map, out var quest, out _))
                questDifficulty = quest.Difficulty;

            if (questDifficulty == null)
                Logger.ErrorS("quest.hunt", "No immediate quest found in hunt character generation!");

            return new CharaFilter()
            {
                MinLevel = _randomGen.CalcObjectLevel((questDifficulty ?? 0) + 1),
                Quality = _randomGen.CalcObjectQuality(Quality.Normal)
            };
            // <<<<<<<< shade2/map.hsp:76 		} ..
        }
    }

    /// <summary>
    /// Indicates this map was created from a map blueprint then modified
    /// for use in a quest.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public sealed class MapDerivedForQuestComponent : Component
    {
    }

    /// <summary>
    /// Raised when a quest map is generated which is derived from some other map.
    /// This is to reset objects inside the map to a sane state.
    /// For example, you would use this event to clear out safes and
    /// reduce the number of well usages so the player can't keep grabbing items from them.
    /// </summary>
    [EventUsage(EventTarget.Map)]
    public sealed class AfterDerivedQuestMapGeneratedEvent : EntityEventArgs
    {
        public AfterDerivedQuestMapGeneratedEvent(IMap map)
        {
            Map = map;
        }

        public IMap Map { get; }
    }

    /// <summary>
    /// Raised on each entity in a quest map which is derived from some other map.
    /// This is to reset objects inside the map to a sane state.
    /// For example, you would use this event to clear out safes and
    /// reduce the number of well usages so the player can't keep grabbing items from them.
    /// </summary>
    public sealed class InitEntityInDerivedQuestMapEvent : EntityEventArgs
    {
        public InitEntityInDerivedQuestMapEvent(IMap map)
        {
            Map = map;
        }

        public IMap Map { get; }
    }
}
