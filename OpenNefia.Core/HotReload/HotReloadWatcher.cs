using JetBrains.Annotations;
using Microsoft.Extensions.FileSystemGlobbing;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.HotReload;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Timing;
using System.Reflection.Metadata;

// This attribute hooks us into the .NET runtime so that our code is
// notified of Hot Reload events.
[assembly: MetadataUpdateHandler(typeof(HotReloadUpdateHandler))]

namespace OpenNefia.Core.HotReload
{
    public delegate void HotReloadDelegate(HotReloadArgs args);

    public record HotReloadArgs(Type[]? UpdatedTypes);

    public interface IHotReloadWatcher
    {
        event HotReloadDelegate? OnClearCache;
        event HotReloadDelegate? OnUpdateApplication;
    }

    internal interface IHotReloadWatcherInternal : IHotReloadWatcher
    {
        void Initialize();

        void EnqueueClearCache(Type[]? updatedTypes);
        void EnqueueUpdateApplication(Type[]? updatedTypes);

        void FrameUpdate(FrameEventArgs frame);
    }

    /// <summary>
    /// Dispatches events at the start of each frame after a .NET Hot Reload
    /// (also known as Edit and Continue/EnC) is detected.
    /// </summary>
    public sealed class HotReloadWatcher : IHotReloadWatcherInternal
    {
        [Dependency] private readonly ITaskManager _taskManager = default!;

        public event HotReloadDelegate? OnClearCache;
        public event HotReloadDelegate? OnUpdateApplication;

        private Queue<HotReloadArgs> _queuedClearCache = new();
        private Queue<HotReloadArgs> _queuedUpdateApplication = new();

        public void Initialize()
        {
            HotReloadUpdateHandler.TaskManager = _taskManager;
        }

        public void EnqueueClearCache(Type[]? updatedTypes)
        {
            var typenames = updatedTypes != null ? updatedTypes.Select(t => t.FullName) : new string[] { "<none>" };
            Logger.DebugS("hotreload", $"ClearCache received");
            foreach (var typename in typenames)
                Logger.DebugS("hotreload", $"  - {typename}");

            _queuedClearCache.Enqueue(new HotReloadArgs(updatedTypes));
        }

        public void EnqueueUpdateApplication(Type[]? updatedTypes)
        {
            var typenames = updatedTypes != null ? updatedTypes.Select(t => t.FullName) : new string[] { "<none>" };
            Logger.DebugS("hotreload", $"UpdateApplication received");
            foreach (var typename in typenames)
                Logger.DebugS("hotreload", $"  - {typename}");

            _queuedUpdateApplication.Enqueue(new HotReloadArgs(updatedTypes));
        }

        public void FrameUpdate(FrameEventArgs frame)
        {
            while (_queuedClearCache.Count != 0)
            {
                var args = _queuedClearCache.Dequeue();
                OnClearCache?.Invoke(args);
            }

            while (_queuedUpdateApplication.Count != 0)
            {
                var args = _queuedUpdateApplication.Dequeue();
                OnUpdateApplication?.Invoke(args);
            }
        }
    }

    /// <seealso cref="MetadataUpdateHandlerAttribute"/>
    internal static class HotReloadUpdateHandler
    {
        internal static ITaskManager? TaskManager { get; set; }

        [UsedImplicitly]
        internal static void ClearCache(Type[]? updatedTypes)
        {
            TaskManager?.RunOnMainThread(() => IoCManager.Resolve<IHotReloadWatcherInternal>().EnqueueClearCache(updatedTypes));
        }

        [UsedImplicitly]
        internal static void UpdateApplication(Type[]? updatedTypes)
        {
            TaskManager?.RunOnMainThread(() => IoCManager.Resolve<IHotReloadWatcherInternal>().EnqueueUpdateApplication(updatedTypes));
        }
    }
}
