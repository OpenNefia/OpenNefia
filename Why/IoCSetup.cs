using Why.Core.Asynchronous;
using Why.Core.ContentPack;
using Why.Core.Exceptions;
using Why.Core.GameController;
using Why.Core.GameObjects;
using Why.Core.Graphics;
using Why.Core.IoC;
using Why.Core.Log;
using Why.Core.Maps;
using Why.Core.Prototypes;
using Why.Core.Reflection;
using Why.Core.ResourceManagement;
using Why.Core.Serialization.Manager;

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
            IoCManager.Register<IGameController, GameController>();
        }
    }
}