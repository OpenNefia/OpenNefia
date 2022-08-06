using System;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;

namespace OpenNefia.Tests
{
    public partial class OpenNefiaUnitTest
    {
        /// <summary>
        /// Registers all the types into the <see cref="IoCManager"/> with <see cref="IoCManager.Register{TInterface, TImplementation}"/>
        /// </summary>
        private void RegisterIoC()
        {
            IoCSetup.Register(DisplayMode.Headless);

            UnitTestIoC.Setup();

            OverrideIoC();

            IoCManager.BuildGraph();
        }
    }
}
