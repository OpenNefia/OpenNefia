using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Utility;
using System.Diagnostics;
using Timer = OpenNefia.Core.Timing.Timer;

namespace OpenNefia.Core.EngineVariables
{
    public sealed partial class EngineVariablesManager
    {
        [Dependency] private readonly IResourceManager _resources = default!;
        [Dependency] private readonly ITaskManager _taskManager = default!;

        private readonly List<FileSystemWatcher> _watchers = new();
        private readonly TimeSpan _reloadDelay = TimeSpan.FromMilliseconds(10);
        private CancellationTokenSource _reloadToken = new();
        private readonly HashSet<ResourcePath> _reloadQueue = new();

        private void WindowFocusedChanged(WindowFocusedEventArgs args)
        {
#if !FULL_RELEASE
            if (args.Focused && _reloadQueue.Count > 0)
            {
                Timer.Spawn(_reloadDelay, ReloadVariablesQueue, _reloadToken.Token);
            }
            else
            {
                _reloadToken.Cancel();
                _reloadToken = new CancellationTokenSource();
            }
#endif
        }

        private void ReloadVariablesQueue()
        {
#if !FULL_RELEASE
            var sw = Stopwatch.StartNew();

            ReloadVariables(_reloadQueue);

            _reloadQueue.Clear();

            Logger.Info($"Reloaded prototypes in {sw.ElapsedMilliseconds} ms");
#endif
        }

        public void WatchResources()
        {
#if !FULL_RELEASE
            foreach (var path in _resources.GetContentRoots().Select(r => r.ToString())
                .Where(r => Directory.Exists(r + "/Variables")).Select(p => p + "/Variables"))
            {
                var watcher = new FileSystemWatcher(path, "*.yml")
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.LastWrite
                };

                watcher.Changed += (_, args) =>
                {
                    switch (args.ChangeType)
                    {
                        case WatcherChangeTypes.Renamed:
                        case WatcherChangeTypes.Deleted:
                            return;
                        case WatcherChangeTypes.Created:
                        // case WatcherChangeTypes.Deleted:
                        case WatcherChangeTypes.Changed:
                        case WatcherChangeTypes.All:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    _taskManager.RunOnMainThread(() =>
                    {
                        var file = new ResourcePath(args.FullPath);

                        foreach (var root in IoCManager.Resolve<IResourceManager>().GetContentRoots())
                        {
                            if (!file.TryRelativeTo(root, out var relative))
                            {
                                continue;
                            }

                            _reloadQueue.Add(relative);
                        }
                    });
                };

                watcher.EnableRaisingEvents = true;
                _watchers.Add(watcher);
            }
#endif
        }
    }
}
