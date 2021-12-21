using OpenNefia.Core.Log;
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

var mapComp = entMan.GetComponent<MapComponent>(theCoords.EntityId);

var map = mapMan.GetMap(mapComp.MapId);
map.RefreshTileEntities(theCoords.Position, entityLookup.GetLiveEntitiesAtPos(theCoords.ToMap(entMan)));

return map.CanAccess(theCoords.Position);
