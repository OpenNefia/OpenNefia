using OpenNefia.Core.HotReload;
using OpenNefia.Core.Timing;

namespace OpenNefia.Tests
{
    internal class DummyHotReloadWatcher : IHotReloadWatcherInternal
    {
        public event HotReloadDelegate? OnClearCache;
        public event HotReloadDelegate? OnUpdateApplication;

        public void Initialize()
        {
        }

        public void EnqueueClearCache(Type[]? updatedTypes)
        {
        }

        public void EnqueueUpdateApplication(Type[]? updatedTypes)
        {
        }

        public void FrameUpdate(FrameEventArgs frame)
        {
        }
    }
}