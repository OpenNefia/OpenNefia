using System;
using System.Diagnostics;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;
using Timer = OpenNefia.Core.Timing.Timer;

namespace OpenNefia.Core.Prototypes
{
    public sealed partial class PrototypeManager : IPrototypeManager
    {
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
                Timer.Spawn(_reloadDelay, ReloadPrototypeQueue, _reloadToken.Token);
            }
            else
            {
                _reloadToken.Cancel();
                _reloadToken = new CancellationTokenSource();
            }
#endif
        }

        private void ReloadPrototypeQueue()
        {
#if !FULL_RELEASE
            var sw = Stopwatch.StartNew();

            ReloadPrototypes(_reloadQueue);

            _reloadQueue.Clear();

            Logger.Info($"Reloaded prototypes in {sw.ElapsedMilliseconds} ms");
#endif
        }

        private void WatchResources()
        {
#if !FULL_RELEASE
            foreach (var path in Resources.GetContentRoots().Select(r => r.ToString())
                .Where(r => Directory.Exists(r + "/Prototypes")).Select(p => p + "/Prototypes"))
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