using OpenNefia.Core.Log;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Game;
using OpenNefia.Core.Utility;
using OpenNefia.Core.GameObjects;

var entMan = IoCManager.Resolve<IEntityManager>();
var mapMan = IoCManager.Resolve<IMapManager>();
var gameSess = IoCManager.Resolve<IGameSessionManager>();

var player = gameSess.Player!;

var blueprints = IoCManager.Resolve<IMapBlueprintLoader>();

var map = blueprints.LoadBlueprint(null, new ResourcePath("/Elona/Map/sqNightmare.yml"));

var spatial = entMan.GetComponent<SpatialComponent>(player.Uid);
spatial.Coordinates = new EntityCoordinates(map.MapEntityUid, spatial.WorldPosition.BoundWithin(map.Bounds));
