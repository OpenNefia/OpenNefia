using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Game;
using OpenNefia.Core.Utility;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;

var entMan = IoCManager.Resolve<IEntityManager>();
entMan.Startup();

var mapMan = IoCManager.Resolve<IMapManager>();
var gameSess = IoCManager.Resolve<IGameSessionManager>();
var mapLoad = IoCManager.Resolve<IMapBlueprintLoader>();

var map = mapLoad.LoadBlueprint(null, new ResourcePath("/Maps/Elona/ntyris.yml"));

foreach (var tile in map.AllTiles)
{
    entMan.SpawnEntity(Protos.Item.AcidproofLiquid, map.AtPos(tile.Position));
}

mapLoad.SaveBlueprint(map.Id, new ResourcePath("/SavedMaps/Test.yml"));
