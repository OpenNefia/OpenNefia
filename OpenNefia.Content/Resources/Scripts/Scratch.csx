#nullable enable
#r "System.Runtime"
#r "C:/Users/yuno/build/OpenNefia.NET/OpenNefia.EntryPoint/bin/Debug/net6.0/OpenNefia.Core.dll"
#r "C:/Users/yuno/build/OpenNefia.NET/OpenNefia.EntryPoint/bin/Debug/net6.0/Resources/Assemblies/OpenNefia.Content.dll"

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
var _activities = EntitySystem.Get<IActivitySystem>();
var _playerQuery = IoCManager.Resolve<IPlayerQuery>();
var _uiMan = IoCManager.Resolve<IUserInterfaceManager>();

public EntityUid player() => _gameSession.Player;
public SpatialComponent playerS() => _entityMan.GetComponent<SpatialComponent>(_gameSession.Player);
public IMap curMap() => _mapMan.ActiveMap!;

public MapCoordinates promptPos()
{
    var args = new PositionPrompt.Args(playerS().MapPosition);
    var result = _uiMan.Query<PositionPrompt, PositionPrompt.Args, PositionPrompt.Result>(args);
    return result.Value.Coords;
}
