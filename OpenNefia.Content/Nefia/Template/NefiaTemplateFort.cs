using OpenNefia.Core.Areas;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Maps;
using System.Drawing;

namespace OpenNefia.Content.Nefia
{
    public sealed class NefiaTemplateFort : IVanillaNefiaTemplate
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;

        public IVanillaNefiaLayout GetLayout(int floorNumber, Blackboard<NefiaGenParams> data)
        {
            IVanillaNefiaLayout gen = new NefiaLayoutStandard();
            if (_rand.OneIn(5))
                gen = new NefiaLayoutResident();
            if (_rand.OneIn(6))
                gen = new NefiaLayoutBigRoom();
            if (_rand.OneIn(7))
                gen = new NefiaLayoutWide();

            return gen;
        }

        public void AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var common = _entityMan.GetComponent<MapCommonComponent>(map.MapEntityUid);
            common.Tileset = Protos.MapTileset.DungeonCastle;

            if (_rand.OneIn(40))
                common.Tileset = Protos.MapTileset.Water;

            common.MaterialSpotType = Protos.MaterialSpot.Building;
        }
    }
}
