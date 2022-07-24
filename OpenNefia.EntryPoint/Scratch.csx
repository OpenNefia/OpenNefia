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
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Directions;
using OpenNefia.Content.Rendering;
using OpenNefia.Core.Random;
using OpenNefia.Content.RandomEvent;

var entMan = IoCManager.Resolve<IEntityManager>();
var item = EntitySystem.Get<IItemGen>();
var chara = EntitySystem.Get<ICharaGen>();
var map = IoCManager.Resolve<IMapManager>();
var _gameSession = IoCManager.Resolve<IGameSessionManager>();
var _rand = IoCManager.Resolve<IRandom>();
var _randEvents = EntitySystem.Get<IRandomEventSystem>();
var _protos = IoCManager.Resolve<IPrototypeManager>();

var spatial = entMan.GetComponent<SpatialComponent>(_gameSession.Player);

foreach (var id in _protos.EnumeratePrototypes<RandomEventPrototype>())
{
    _randEvents.Trigger(id.GetStrongID());
}
