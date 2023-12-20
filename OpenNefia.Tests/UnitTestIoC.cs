using OpenNefia.Core.ContentPack;
using OpenNefia.Core.HotReload;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;

namespace OpenNefia.Tests
{
    internal static class UnitTestIoC
    {
        public static void Setup(bool loadLocalizationsFromDisk = false)
        {
            IoCManager.Register<IModLoader, TestingModLoader>(overwrite: true);
            IoCManager.Register<IModLoaderInternal, TestingModLoader>(overwrite: true);
            IoCManager.Register<TestingModLoader, TestingModLoader>(overwrite: true);
            if (loadLocalizationsFromDisk)
                IoCManager.Register<ILocalizationManager, TestingLocalizationManager>(overwrite: true);
            else
                IoCManager.Register<ILocalizationManager, DummyLocalizationManager>(overwrite: true);
            IoCManager.Register<IHotReloadWatcher, DummyHotReloadWatcher>(overwrite: true);
            IoCManager.Register<IHotReloadWatcherInternal, DummyHotReloadWatcher>(overwrite: true);
        }
    }
}
