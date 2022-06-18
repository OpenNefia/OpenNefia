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
    public sealed class NefiaTemplateForest : IVanillaNefiaTemplate
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;

        public IVanillaNefiaLayout GetLayout(int floorNumber, Blackboard<NefiaGenParams> data)
        {
            IVanillaNefiaLayout gen = new NefiaLayoutWide();
            if (_rand.OneIn(6))
                gen = new NefiaLayoutStandard();
            if (_rand.OneIn(6))
                gen = new NefiaLayoutPuppyCave();
            if (_rand.OneIn(25))
                gen = new NefiaLayoutLong();
            if (_rand.OneIn(20))
                gen = new NefiaLayoutResident();

            return gen;
        }

        public void AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var common = _entityMan.GetComponent<MapCommonComponent>(map.MapEntityUid);
            common.Tileset = Protos.MapTileset.DungeonForest;
            common.MaterialSpotType = Protos.MaterialSpot.Forest;
        }
    }
}
