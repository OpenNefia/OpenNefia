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

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMapTilesetSystem _mapTilesets = default!;

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

        /// <summary>
        /// Generates the map for party quests.
        /// </summary>
        /// <param name="difficulty">Difficulty of the quest. Controls the level of inebriated partygoers.</param>
        /// <returns></returns>
        private IMap QuestMap_GenerateParty(int difficulty)
        {
            // >>>>>>>> elona122/shade2/map_rand.hsp:584 *map_createDungeonPerform ..
            var map = _mapManager.CreateMap(38, 28, new("Elona.MapQuestParty"));

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
    }
}
