#nullable enable
#r "System.Runtime"
#r "NLua, Version=1.6.0.0, Culture=neutral, PublicKeyToken=6a194c04b9c89217"
#r "C:/Users/yuno/build/OpenNefia.NET/OpenNefia.EntryPoint/bin/Debug/net6.0/OpenNefia.Core.dll"
#r "C:/Users/yuno/build/OpenNefia.NET/OpenNefia.EntryPoint/bin/Debug/net6.0/Resources/Assemblies/OpenNefia.Content.dll"

using NLua;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Areas;
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
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Directions;
using OpenNefia.Content.Rendering;
using OpenNefia.Core.Random;
using OpenNefia.Content.RandomEvent;
using OpenNefia.Content.World;
using OpenNefia.Core.DebugServer;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Logic;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.UI.Layer;

var _entityMan = IoCManager.Resolve<IEntityManager>();
var _mapMan = IoCManager.Resolve<IMapManager>();
var _areaMan = IoCManager.Resolve<IAreaManager>();
var _itemGen = EntitySystem.Get<IItemGen>();
var _charaGen = EntitySystem.Get<ICharaGen>();
var _gameSession = IoCManager.Resolve<IGameSessionManager>();
var _rand = IoCManager.Resolve<IRandom>();
var _randEvents = EntitySystem.Get<IRandomEventSystem>();
var _protos = IoCManager.Resolve<IPrototypeManager>();
var _world = EntitySystem.Get<IWorldSystem>();
var _lookup = EntitySystem.Get<IEntityLookup>();
var _activities = EntitySystem.Get<IActivitySystem>();
var _playerQuery = IoCManager.Resolve<IPlayerQuery>();
var _uiMan = IoCManager.Resolve<IUserInterfaceManager>();
var _loc = IoCManager.Resolve<ILocalizationManager>();
var _mapEntrance = EntitySystem.Get<IMapEntranceSystem>();

public EntityUid player() => _gameSession.Player;
public SpatialComponent playerS() => _entityMan.GetComponent<SpatialComponent>(_gameSession.Player);
public IMap curMap() => _mapMan.ActiveMap!;

public MapCoordinates promptPos()
{
    var args = new PositionPrompt.Args(playerS().MapPosition);
    var result = _uiMan.Query<PositionPrompt, PositionPrompt.Args, PositionPrompt.Result>(args);
    return result.Value.Coords;
}

public T res<T>() => IoCManager.Resolve<T>();
public T sys<T>() where T : IEntitySystem => EntitySystem.Get<T>();

public T comp<T>(EntityUid uid) where T : class, IComponent
{
    return _entityMan.GetComponent<T>(uid);
}

public SpatialComponent entityAt()
{
    var coords = promptPos();
    return _lookup.GetLiveEntitiesAtCoords(coords).First();
}

public void gotoArea(string id)
{
    var entrance = new MapEntrance()
    {
        MapIdSpecifier = new GlobalAreaMapIdSpecifier(new GlobalAreaId(id)),
        StartLocation = new MapEdgesLocation()
    };
    _mapEntrance.UseMapEntrance(player(), entrance);
}

public void sexWith()
{
    var entity = entityAt();
    var activity = _entityMan.SpawnEntity(Protos.Activity.Sex, MapCoordinates.Global);
    comp<ActivitySexComponent>(activity).Partner = entity.Owner;
    comp<ActivitySexComponent>(activity).IsTopping = true;
    _activities.StartActivity(player(), activity);
}
