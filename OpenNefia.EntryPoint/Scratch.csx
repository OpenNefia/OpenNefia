#nullable enable
#r "System.Runtime"
#r "C:/Users/yuno/build/OpenNefia.NET/OpenNefia.EntryPoint/bin/Debug/net6.0/OpenNefia.Core.dll"
#r "C:/Users/yuno/build/OpenNefia.NET/OpenNefia.EntryPoint/bin/Debug/net6.0/Resources/Assemblies/OpenNefia.Content.dll"

using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Game;
using OpenNefia.Core.Utility;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.Debug;
using OpenNefia.Content.Nefia;
using OpenNefia.Content.Maps;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.GameObjects;

var _maps = IoCManager.Resolve<IMapManager>();
var _entities = IoCManager.Resolve<IEntityManager>();
var _script = EntitySystem.Get<ScriptTools>();

var area = _script.GetOrCreateArea("TestArea", new("Elona.NefiaDungeon"), null);
// var gen = new NefiaFloorGenerator();
// var mapId = new MapId(999);

// if (gen.TryToGenerate(area, mapId, 1, out var map))
// {
//     return _script.PrintMap(map);
// }

var tmap = _maps.ActiveMap!;
var _tags = EntitySystem.Get<TagSystem>();
var stair = _tags.EntityWithTagInMap(tmap.Id, Protos.Tag.DungeonStairsDelving);
var stairs = _entities.GetComponent<StairsComponent>(stair!.Owner);

return stairs.Entrance;
