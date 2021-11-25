using System;
using Why;
using Why.Core.ContentPack;
using Why.Core.IoC;

namespace Why.Tests
{
    public partial class WhyUnitTest
    {
        /// <summary>
        /// Registers all the types into the <see cref="IoCManager"/> with <see cref="IoCManager.Register{TInterface, TImplementation}"/>
        /// </summary>
        private void RegisterIoC()
        {
            IoCSetup.Run();

            IoCManager.Register<IModLoader, TestingModLoader>(overwrite: true);
            IoCManager.Register<IModLoaderInternal, TestingModLoader>(overwrite: true);
            IoCManager.Register<TestingModLoader, TestingModLoader>(overwrite: true);

            OverrideIoC();

            IoCManager.BuildGraph();
        }
    }
}
