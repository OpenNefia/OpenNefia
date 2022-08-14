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
    public sealed class NefiaTemplateAncientCastle : IVanillaNefiaTemplate
    {
        [Dependency] private readonly IEntityManager _entityMan = default!;

        public IVanillaNefiaLayout GetLayout(int floorNumber, Blackboard<NefiaGenParams> data)
        {
            return new NefiaLayoutStandard();
        }

        public void AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var level = _entityMan.GetComponent<LevelComponent>(map.MapEntityUid);
            var common = _entityMan.GetComponent<MapCommonComponent>(map.MapEntityUid);
            var charaGen = _entityMan.GetComponent<MapCharaGenComponent>(map.MapEntityUid);
            common.Tileset = Protos.MapTileset.DungeonCastle;
            charaGen.MaxCharaCount += level.Level / 2;
        }
    }
}
