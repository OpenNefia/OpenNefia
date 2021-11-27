using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Exceptions;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Serialization.Manager;

namespace Why
{
    public class IoCSetup
    {
        public static void Run()
        {
            IoCManager.Register<IRuntimeLog, RuntimeLog>();
            IoCManager.Register<ILogManager, LogManager>();
            IoCManager.Register<IDynamicTypeFactory, DynamicTypeFactory>();
            IoCManager.Register<IDynamicTypeFactoryInternal, DynamicTypeFactory>();
            IoCManager.Register<IEntitySystemManager, EntitySystemManager>();
            IoCManager.Register<IReflectionManager, ReflectionManager>();
            IoCManager.Register<IMapManager, MapManager>();
            IoCManager.Register<ITaskManager, TaskManager>();
            IoCManager.Register<IComponentDependencyManager, ComponentDependencyManager>();
            IoCManager.Register<IComponentFactory, ComponentFactory>();
            IoCManager.Register<IPrototypeManager, PrototypeManager>();
            IoCManager.Register<IResourceManager, ResourceCache>();
            IoCManager.Register<IResourceManagerInternal, ResourceCache>();
            IoCManager.Register<IResourceCache, ResourceCache>();
            IoCManager.Register<IResourceCacheInternal, ResourceCache>();
            IoCManager.Register<IModLoader, ModLoader>();
            IoCManager.Register<IModLoaderInternal, ModLoader>();
            IoCManager.Register<IEntityManager, EntityManager>();
            IoCManager.Register<IGraphics, LoveGraphics>();
            IoCManager.Register<ISerializationManager, SerializationManager>();
            IoCManager.Register<IAssetManager, AssetManager>();
            IoCManager.Register<IAtlasManager, AtlasManager>();
            IoCManager.Register<IRandom, SysRandom>();
            IoCManager.Register<IGameSessionManager, GameSessionManager>();
            IoCManager.Register<IGameController, GameController>();
        }
    }
}