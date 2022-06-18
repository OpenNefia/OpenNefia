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
var _nefia = EntitySystem.Get<AreaNefiaSystem>();

var area = _script.GetOrCreateArea("TestArea", new("Elona.NefiaDungeon"), null);
var floorId = AreaNefiaSystem.FloorNumberToAreaId(1);
var cur = _maps.ActiveMap!;

var nefiaVanilla = _entities.GetComponent<NefiaVanillaComponent>(area.AreaEntityUid);
nefiaVanilla.Template = new BasicNefiaTemplate()
{
    Layout = new NefiaLayoutResident()
};

var ev = new NefiaFloorGenerateEvent(area, floorId, cur.AtPos(1, 1));
_entities.EventBus.RaiseLocalEvent(area.AreaEntityUid, ev);

if (ev.Handled)
{
    var result = _maps.GetMap(ev.ResultMapId.Value);
    return _script.PrintMap(result);
}

return "???";
