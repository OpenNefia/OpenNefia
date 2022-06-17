using OpenNefia.Core.Areas;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Maps;

namespace OpenNefia.Content.Nefia
{
    public sealed class NefiaTemplateDungeon : IVanillaNefiaTemplate
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;

        public IVanillaNefiaLayout GetLayout(int floorNumber, Blackboard<NefiaGenParams> data)
        {
            IVanillaNefiaLayout gen = new NefiaLayoutWide();
            if (_rand.OneIn(4))
                gen = new NefiaLayoutStandard();
            if (_rand.OneIn(6))
                gen = new NefiaLayoutPuppyCave();
            if (_rand.OneIn(10))
                gen = new NefiaLayoutResident();
            if (_rand.OneIn(25))
                gen = new NefiaLayoutLong();

            return gen;
        }

        public void AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var common = _entityMan.GetComponent<MapCommonComponent>(map.MapEntityUid);
            common.Tileset = Protos.MapTileset.Dungeon;

            if (_rand.OneIn(20))
                common.Tileset = Protos.MapTileset.Water;

            common.MaterialSpotType = Protos.MaterialSpot.Dungeon;
        }
    }
}
