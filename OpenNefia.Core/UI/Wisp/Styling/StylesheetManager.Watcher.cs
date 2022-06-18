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

namespace OpenNefia.Core.UI.Wisp.Styling
{
    public sealed partial class StylesheetManager
    {
        [Dependency] private readonly ITaskManager _taskManager = default!;

        private readonly List<FileSystemWatcher> _watchers = new();

        private void WatchResources()
        {
#if !FULL_RELEASE
            foreach (var path in _resourceCache.GetContentRoots().Select(r => r.ToString())
                .Where(r => Directory.Exists(r + "/Stylesheets")).Select(p => p + "/Stylesheets"))
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

                            var sw = Stopwatch.StartNew();
                            TryLoadStylesheet(relative.ToRootedPath());
                            Logger.InfoS("wisp.stylesheet", $"Reloaded stylesheet in {sw.ElapsedMilliseconds}ms");
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
