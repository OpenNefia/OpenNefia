using System;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;

namespace OpenNefia.Tests
{
    public partial class OpenNefiaUnitTest
    {
        /// <summary>
        /// Registers all the types into the <see cref="IoCManager"/> with <see cref="IoCManager.Register{TInterface, TImplementation}"/>
        /// </summary>
        private void RegisterIoC()
        {
            IoCSetup.Register(GameController.DisplayMode.Headless);

            IoCManager.Register<IModLoader, TestingModLoader>(overwrite: true);
            IoCManager.Register<IModLoaderInternal, TestingModLoader>(overwrite: true);
            IoCManager.Register<TestingModLoader, TestingModLoader>(overwrite: true);
            IoCManager.Register<ILocalizationManager, TestingLocalizationManager>(overwrite: true);

            OverrideIoC();

            IoCManager.BuildGraph();
        }
    }
}
