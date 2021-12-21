﻿using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.Audio;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Exceptions;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia
{
    public class IoCSetup
    {
        public static void Register()
        {
            IoCManager.Register<IGraphics, LoveGraphics>();
            IoCManager.Register<IRuntimeLog, RuntimeLog>();
            IoCManager.Register<ILogManager, LogManager>();
            IoCManager.Register<IDynamicTypeFactory, DynamicTypeFactory>();
            IoCManager.Register<IDynamicTypeFactoryInternal, DynamicTypeFactory>();
            IoCManager.Register<IEntitySystemManager, EntitySystemManager>();
            IoCManager.Register<IReflectionManager, ReflectionManager>();
            IoCManager.Register<IMapManager, MapManager>();
            IoCManager.Register<IComponentDependencyManager, ComponentDependencyManager>();
            IoCManager.Register<IComponentFactory, ComponentFactory>();
            IoCManager.Register<IPrototypeManager, PrototypeManager>();
            IoCManager.Register<IResourceManager, ResourceCache>();
            IoCManager.Register<IResourceManagerInternal, ResourceCache>();
            IoCManager.Register<IResourceCache, ResourceCache>();
            IoCManager.Register<IResourceCacheInternal, ResourceCache>();
            IoCManager.Register<IModLoader, ModLoader>();
            IoCManager.Register<IModLoaderInternal, ModLoader>();
            IoCManager.Register<ITileDefinitionManager, TileDefinitionManager>();
            IoCManager.Register<IEntityManager, EntityManager>();
            IoCManager.Register<ISerializationManager, SerializationManager>();
            IoCManager.Register<IAssetManager, AssetManager>();
            IoCManager.Register<ITileAtlasManager, TileAtlasManager>();
            IoCManager.Register<IUiLayerManager, UiLayerManager>();
            IoCManager.Register<IRandom, SysRandom>();
            IoCManager.Register<IFontManager, FontManager>();
            IoCManager.Register<ILocalizationManager, LocalizationManager>();
            IoCManager.Register<ITaskManager, TaskManager>();
            IoCManager.Register<IGameSessionManager, GameSessionManager>();
            IoCManager.Register<IGameController, GameController>();
            IoCManager.Register<ICoords, OrthographicCoords>();
            IoCManager.Register<IMapRenderer, MapRenderer>();
            IoCManager.Register<IMapDrawables, MapDrawables>();
            IoCManager.Register<IMusicManager, LoveMusicManager>();
        }
    }
}