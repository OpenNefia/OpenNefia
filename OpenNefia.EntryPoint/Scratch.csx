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

var entMan = IoCManager.Resolve<IEntityManager>();
var item = EntitySystem.Get<IItemGen>();
var chara = EntitySystem.Get<ICharaGen>();
var map = IoCManager.Resolve<IMapManager>();
var _mapDrawables = IoCManager.Resolve<IMapDrawables>();
var _gameSession = IoCManager.Resolve<IGameSessionManager>();
var _rand = IoCManager.Resolve<IRandom>();

var spatial = entMan.GetComponent<SpatialComponent>(_gameSession.Player);

foreach (var dir in DirectionUtility.RandomDirections())
{
    var drawable = new RangedAttackMapDrawable(spatial.MapPosition, new(spatial.MapPosition.MapId, spatial.MapPosition.Position + dir.ToIntVec() * 8), Protos.Chip.ItemProjectileArrow);
    _mapDrawables.Enqueue(drawable, spatial.MapPosition);
    Console.WriteLine(dir.ToString());
}

return _rand.NextVec2iInRadius(2);
