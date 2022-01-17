using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Utility;

namespace OpenNefia.Tests
{
    [Parallelizable]
    public abstract partial class OpenNefiaUnitTest
    {
        [OneTimeSetUp]
        public void BaseSetup()
        {
            // Clear state across tests.
            IoCManager.InitThread();
            IoCManager.Clear();

            RegisterIoC();

            var assemblies = new List<Assembly>();

            assemblies.Add(typeof(OpenNefia.Core.Engine).Assembly);
            assemblies.Add(Assembly.GetExecutingAssembly());

            var configurationManager = IoCManager.Resolve<IConfigurationManagerInternal>();

            configurationManager.Initialize();

            foreach (var assembly in assemblies)
            {
                configurationManager.LoadCVarsFromAssembly(assembly);
            }

            var contentAssemblies = GetContentAssemblies();

            foreach (var assembly in contentAssemblies)
            {
                configurationManager.LoadCVarsFromAssembly(assembly);
            }

            configurationManager.LoadCVarsFromAssembly(typeof(OpenNefiaUnitTest).Assembly);

            // Required systems
            var systems = IoCManager.Resolve<IEntitySystemManager>();
            systems.LoadExtraSystemType<EntityLookup>();
            systems.LoadExtraSystemType<SpatialSystem>();

            var entMan = IoCManager.Resolve<IEntityManager>();

            if (entMan.EventBus == null)
            {
                entMan.Initialize();
                entMan.Startup();
            }

            IoCManager.Resolve<IReflectionManager>().LoadAssemblies(assemblies);

            var modLoader = IoCManager.Resolve<TestingModLoader>();
            modLoader.Assemblies = contentAssemblies;
            modLoader.TryLoadModulesFrom(ResourcePath.Root, "");

            // Required since localization hooks into entity creation.
            IoCManager.Resolve<ILocalizationManager>().Initialize();

            // Required components for the engine to work
            var compFactory = IoCManager.Resolve<IComponentFactory>();
            compFactory.DoDefaultRegistrations();

            if (entMan.EventBus == null)
            {
                entMan.Startup();
            }

            // Make randomness deterministic.
            var random = IoCManager.Resolve<IRandom>();
            random.PushSeed(0);
        }

        [OneTimeTearDown]
        public void BaseTearDown()
        {
            IoCManager.Clear();
        }

        /// <summary>
        /// Called after all IoC registration has been done, but before the graph has been built.
        /// This allows one to add new IoC types or overwrite existing ones if needed.
        /// </summary>
        protected virtual void OverrideIoC()
        {
        }

        protected virtual Assembly[] GetContentAssemblies()
        {
            return Array.Empty<Assembly>();
        }
    }
}
