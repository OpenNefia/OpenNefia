using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Game;
using OpenNefia.Core.Utility;
using OpenNefia.Core.GameObjects;

var entMan = IoCManager.Resolve<IEntityManager>();
var mapMan = IoCManager.Resolve<IMapManager>();
var gameSess = IoCManager.Resolve<IGameSessionManager>();
var entityLookup = EntitySystem.Get<IEntityLookup>();

var player = gameSess.Player!;

var spatial = entMan.GetComponent<SpatialComponent>(player.Uid);

var coords = new MapCoordinates(new Vector2i(1, 22), spatial.MapID);

var map = mapMan.GetMap(spatial.MapID);
map.MapObjectMemory.RevealObjects(coords.Position);

return player.Spatial.Direction;
