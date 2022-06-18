using OpenNefia.Core.Areas;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Levels;

namespace OpenNefia.Content.Nefia
{
    public sealed class NefiaTemplateLesimas : IVanillaNefiaTemplate
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;

        private readonly IReadOnlyDictionary<int, IVanillaNefiaLayout> PredefinedLayouts
            = new Dictionary<int, IVanillaNefiaLayout>()
            {
                {1, new NefiaLayoutWide()},
                {5, new NefiaLayoutJail()},
                {10, new NefiaLayoutBigRoom()},
                {15, new NefiaLayoutJail()},
                {20, new NefiaLayoutBigRoom()},
                {25, new NefiaLayoutJail()},
                {30, new NefiaLayoutBigRoom()},
            };

        public IVanillaNefiaLayout GetLayout(int floorNumber, Blackboard<NefiaGenParams> data)
        {
            IVanillaNefiaLayout gen = new NefiaLayoutStandard();
            if (_rand.OneIn(30))
                gen = new NefiaLayoutBigRoom();

            if (PredefinedLayouts.TryGetValue(floorNumber, out var layout))
            {
                gen = layout;
            }
            else
            {
                if (floorNumber < 30 && _rand.OneIn(4))
                    gen = new NefiaLayoutWide();

                if (_rand.OneIn(5))
                    gen = new NefiaLayoutResident();
                if (_rand.OneIn(20))
                    gen = new NefiaLayoutLong();
                if (_rand.OneIn(6))
                    gen = new NefiaLayoutPuppyCave();
            }

            return gen;
        }

        public void AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var level = _entityMan.GetComponent<LevelComponent>(map.MapEntityUid);
            var common = _entityMan.GetComponent<MapCommonComponent>(map.MapEntityUid);
            var charaGen = _entityMan.GetComponent<MapCharaGenComponent>(map.MapEntityUid);
            common.Tileset = Protos.MapTileset.Tower2;
            charaGen.MaxCharaCount += level.Level / 2;

            if (_rand.OneIn(20))
                common.Tileset = Protos.MapTileset.Water;
            if (floorNumber < 35)
                common.Tileset = Protos.MapTileset.Dungeon;
            if (floorNumber < 20)
                common.Tileset = Protos.MapTileset.Tower1;
            if (floorNumber < 10)
                common.Tileset = Protos.MapTileset.Tower2;
            if (floorNumber < 5)
                common.Tileset = Protos.MapTileset.Dungeon;

            common.MaterialSpotType = Protos.MaterialSpot.Building;
        }
    }
}
