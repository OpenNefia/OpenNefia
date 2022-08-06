using OpenNefia.Core.ContentPack;
using OpenNefia.Core.HotReload;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;

namespace OpenNefia.Tests
{
    internal static class UnitTestIoC
    {
        public static void Setup()
        {
            IoCManager.Register<IModLoader, TestingModLoader>(overwrite: true);
            IoCManager.Register<IModLoaderInternal, TestingModLoader>(overwrite: true);
            IoCManager.Register<TestingModLoader, TestingModLoader>(overwrite: true);
            IoCManager.Register<ILocalizationManager, DummyLocalizationManager>(overwrite: true);
            IoCManager.Register<IHotReloadWatcher, DummyHotReloadWatcher>(overwrite: true);
            IoCManager.Register<IHotReloadWatcherInternal, DummyHotReloadWatcher>(overwrite: true);
        }
    }
}
