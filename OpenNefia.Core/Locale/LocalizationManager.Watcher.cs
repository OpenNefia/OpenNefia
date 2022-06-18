using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = OpenNefia.Core.Timing.Timer;

namespace OpenNefia.Core.Locale
{
    public partial class LocalizationManager
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

            foreach (var file in _reloadQueue)
            {
                LoadContentFile(ResourcePath.Root / file);
            }

            Resync();

            _reloadQueue.Clear();

            Logger.Info("loc", $"Reloaded localization files in {sw.ElapsedMilliseconds} ms");
#endif
        }

        private void WatchResources()
        {
#if !FULL_RELEASE
            foreach (var path in _resourceManager.GetContentRoots().Select(r => r.ToString())
                .Where(r => Directory.Exists(r + "/Locale/" + Language.ToString())).Select(p => p + "/Locale/" + Language.ToString()))
            {
                var watcher = new FileSystemWatcher(path, "*.lua")
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
