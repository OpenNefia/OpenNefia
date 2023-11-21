using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Religion;
using OpenNefia.Core.Game;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Food;
using OpenNefia.Content.Pickable;
using OpenNefia.Core.Audio;
using OpenNefia.Content.World;
using OpenNefia.Content.TitleScreen;

namespace OpenNefia.Content.Maps
{
    public interface IMapCommonSystem : IEntitySystem
    {
        void PlayDefaultMapMusic(IMap map);
    }
    
    public class MapCommonSystem : EntitySystem, IMapCommonSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IReligionSystem _religion = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IFoodSystem _food = default!;
        [Dependency] private readonly IMusicManager _music = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            SubscribeEntity<MapCreatedEvent>(AddRequiredComponents, priority: EventPriorities.Highest);
            SubscribeEntity<MapRenewMajorEvent>(SpawnRandomSites, priority: EventPriorities.Low);
            SubscribeComponent<MapCommonComponent, MapEnterEvent>(SpoilFood, priority: EventPriorities.Low);
            SubscribeEntity<MapCalcDefaultMusicEvent>(CalcDefaultMapMusic, priority: EventPriorities.Highest);
            SubscribeBroadcast<GameQuickLoadedEventArgs>(HandleQuickLoaded);
        }

        private void AddRequiredComponents(EntityUid mapEntity, MapCreatedEvent args)
        {
            EntityManager.EnsureComponent<MapCommonComponent>(mapEntity);
        }

        private void SpawnRandomSites(EntityUid mapEntity, MapRenewMajorEvent args)
        {
            // >>>>>>>> shade2/map_func.hsp:793 #deffunc map_randSite int dx,int dy ...
            var amount = CalcRandomSiteGenerateCount(args.Map);

            for (var i = 0; i < amount; i++)
            {
                SpawnRandomSite(args.Map, args.IsFirstRenewal);
            }
            // <<<<<<<< shade2/map_func.hsp:891 	return ..
        }

        private void SpoilFood(EntityUid uid, MapCommonComponent common, MapEnterEvent args)
        {
            if (!common.IsTemporary)
                _food.SpoilFoodInMap(args.Map);
        }

        private void HandleQuickLoaded(GameQuickLoadedEventArgs ev)
        {
            PlayDefaultMapMusic(_mapManager.ActiveMap!);
        }

        private static readonly PrototypeId<MusicPrototype>[] DungeonMusicIDs = new[]
        {
            Protos.Music.Dungeon1,
            Protos.Music.Dungeon2,
            Protos.Music.Dungeon3,
            Protos.Music.Dungeon4,
            Protos.Music.Dungeon5,
            Protos.Music.Dungeon6,
        };

        private static readonly PrototypeId<MusicPrototype>[] FieldMusicIDs = new[]
        {
            Protos.Music.Field1,
            Protos.Music.Field2,
            Protos.Music.Field3,
        };

        private void CalcDefaultMapMusic(EntityUid uid, MapCalcDefaultMusicEvent args)
        {
            if (HasComp<MapTypeFieldComponent>(uid))
            {
                args.OutMusicID = null;
                return;
            }

            if (TryComp<MapCommonComponent>(uid, out var mapCommon) && mapCommon.Music != null)
            {
                args.OutMusicID = mapCommon.Music.Value;
                return;
            }

            if (HasComp<MapTypeTownComponent>(uid))
                args.OutMusicID = Protos.Music.Town1;

            if (HasComp<MapTypePlayerOwnedComponent>(uid))
                args.OutMusicID = Protos.Music.Home;

            if (HasComp<MapTypeDungeonComponent>(uid))
                args.OutMusicID = DungeonMusicIDs[_world.State.GameDate.Hour % DungeonMusicIDs.Length];

            if (HasComp<MapTypeWorldMapComponent>(uid))
                args.OutMusicID = FieldMusicIDs[_world.State.GameDate.Hour % FieldMusicIDs.Length];
        }

        public int CalcRandomSiteGenerateCount(IMap map)
        {
            // >>>>>>>> shade2/map.hsp:2214 			p=rnd(mHeight*mWidth/400+3)  ...
            var amount = _rand.Next(map.Width * map.Height / 400 + 3);

            if (HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
                amount = _rand.Next(40);

            if (HasComp<MapTypeTownComponent>(map.MapEntityUid))
                amount = _rand.Next(_rand.Next(_rand.Next(12) + 1) + 1);

            if (HasComp<MapTypeGuildComponent>(map.MapEntityUid))
                amount = _rand.Next(amount + 1);

            return amount;
            // <<<<<<<< shade2/map.hsp:2217 			if mType=mTypeVillage	: p=rnd(p+1) ..
        }

        private bool SpawnRandomSite(IMap map, bool isFirstRenewal, Vector2i? pos = null)
        {
            if (pos == null)
            {
                for (var i = 0; i < 20; i++)
                {
                    var newPos = new Vector2i(_rand.Next(Math.Max(map.Width - 5, 1)) + 2, _rand.Next(Math.Max(map.Height - 5, 1)) + 2);

                    if (map.CanAccess(newPos)
                        && _lookup.GetLiveEntitiesAtCoords(map.AtPos(newPos)).Count() == 0)
                    {
                        pos = newPos;
                        break;
                    }
                }
            }

            if (pos == null)
                return false;

            var coords = map.AtPos(pos.Value);

            if (HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
            {
                var tile = map.GetTileID(pos.Value)!;
                var sea = _protos.Index(Protos.FieldMap.Sea);
                if (sea.WorldMapTiles.Contains(tile.Value) || ProtoSets.Tile.WorldMapRoadTiles.Contains(tile.Value))
                {
                    return false;
                }
            }

            if (HasComp<MapTypeDungeonComponent>(map.MapEntityUid))
            {
                if (isFirstRenewal)
                {
                    if (_rand.OneIn(25))
                    {
                        var fountain = _itemGen.GenerateItem(coords, Protos.Item.Fountain, args: EntityGenArgSet.Make(new ItemGenArgs() { OwnState = OwnState.NPC }));
                        if (fountain != null)
                            return true;
                    }
                    if (_rand.OneIn(100))
                    {
                        var altar = _itemGen.GenerateItem(coords, Protos.Item.Fountain, args: EntityGenArgSet.Make(new ItemGenArgs() { OwnState = OwnState.NPC }));
                        if (altar != null)
                        {
                            var altarComp = EnsureComp<AltarComponent>(altar.Value);
                            altarComp.GodID = _religion.PickRandomGodID();
                            return true;
                        }
                    }
                }

                var matSpotID = Protos.MObj.MaterialSpotDefault;

                if (_rand.OneIn(14))
                    matSpotID = Protos.MObj.MaterialSpotRemains;
                if (_rand.OneIn(13))
                    matSpotID = Protos.MObj.MaterialSpotMine;
                if (_rand.OneIn(12))
                    matSpotID = Protos.MObj.MaterialSpotSpring;
                if (_rand.OneIn(11))
                    matSpotID = Protos.MObj.MaterialSpotBush;

                _entityGen.SpawnEntity(matSpotID, coords);
                return true;
            }

            if (HasComp<MapTypeTownComponent>(map.MapEntityUid) || HasComp<MapTypeGuildComponent>(map.MapEntityUid))
            {
                if (_rand.OneIn(3))
                {
                    _itemGen.GenerateItem(map, Protos.Item.MoonGate);
                    return true;
                }
                if (_rand.OneIn(15))
                {
                    _itemGen.GenerateItem(map, Protos.Item.PlatinumCoin);
                    return true;
                }
                if (_rand.OneIn(20))
                {
                    _itemGen.GenerateItem(map, Protos.Item.Wallet);
                    return true;
                }
                if (_rand.OneIn(15))
                {
                    _itemGen.GenerateItem(map, Protos.Item.Suitcase);
                    return true;
                }
                if (_rand.OneIn(18))
                {
                    var filter = new ItemFilter()
                    {
                        MinLevel = _randomGen.CalcObjectLevel(_rand.Next(_levels.GetLevel(_gameSession.Player))),
                        Quality = _randomGen.CalcObjectQuality(Quality.Good),
                        Tags = new[] { _randomGen.PickTag(Protos.TagSet.ItemWear) },
                    };
                    _itemGen.GenerateItem(coords, filter);
                    return true;
                }

                _itemGen.GenerateItem(coords, new ItemFilter() { MinLevel = 10, Tags = new[] { Protos.Tag.ItemCatJunkTown } });
                return true;
            }

            return false;
        }

        public PrototypeId<MusicPrototype>? GetMapDefaultMusic(IMap map)
        {
            var ev = new MapCalcDefaultMusicEvent();
            RaiseEvent(map.MapEntityUid, ev);
            return ev.OutMusicID;
        }

        public void PlayDefaultMapMusic(IMap map)
        {
            var musicId = GetMapDefaultMusic(map);
            if (musicId != null)
                _music.Play(musicId.Value);
        }
    }

    public sealed class MapCalcDefaultMusicEvent : EntityEventArgs
    {
        public PrototypeId<MusicPrototype>? OutMusicID { get; set; } = null;

        public MapCalcDefaultMusicEvent()
        {
        }
    }
}
