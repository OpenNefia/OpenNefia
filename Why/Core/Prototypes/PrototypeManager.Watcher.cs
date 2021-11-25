/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Why.Core.ContentPack;
using Why.Core.Graphics;
using Why.Core.IoC;
using Why.Core.Log;
using Why.Core.Prototypes;
using Why.Core.Utility;
using Timer = Why.Core.Timing.Timer;

namespace Why.Core.Prototypes
{
    public sealed partial class PrototypeManager : IPrototypeManager
    {
        [Dependency] private readonly IGraphics _graphics = default!;

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
            _graphics.OnWindowFocused += WindowFocusedChanged;

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

                    TaskManager.RunOnMainThread(() =>
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
*/