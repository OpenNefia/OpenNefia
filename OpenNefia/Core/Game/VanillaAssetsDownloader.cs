using OpenNefia.Core.Data;
using OpenNefia.Core.IoC;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Game
{
    internal class VanillaAssetsDownloader : IProgressableJob
    {
        private readonly IResourceCacheInternal _resourceCache = default!;

        private const string URL_YLVANIA_ELONA122 = "http://ylvania.style.coocan.jp/file/elona122.zip";

        private static readonly ResourcePath _assetsGraphicPath =  new ResourcePath("/Assets/Elona/Graphic");
        private static readonly ResourcePath _assetsSoundPath =  new ResourcePath("/Assets/Elona/Sound");

        private static readonly ResourcePath _cachePath = new ResourcePath("/Cache/Deps/elona122.zip");

        public VanillaAssetsDownloader(IResourceCacheInternal resourceCache)
        {
            _resourceCache = resourceCache;
        }

        public bool NeedsDownload()
        {
            return !_resourceCache.UserData.IsDirectory(_assetsGraphicPath)
                || !_resourceCache.UserData.IsDirectory(_assetsSoundPath);
        }

        public uint NumberOfSteps => 2;
        public float Progress { get; set; }

        private async Task DownloadElona122Zip(ProgressOperation progress)
        {
            if (_resourceCache.UserData.Exists(_cachePath))
                return;

            if (!_resourceCache.UserData.IsDirectory(_cachePath))
                _resourceCache.UserData.CreateDirectory(_cachePath.Directory);

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(URL_YLVANIA_ELONA122, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];
                        Console.WriteLine("Download Started");
                        long receivedBytes = 0;
                        long totalBytes = response.Content.Headers.ContentLength!.Value;

                        for (;;)
                        {
                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                            if (bytesRead == 0)
                            {
                                await Task.Yield();
                                break;
                            }

                            receivedBytes += bytesRead;

                            progress.Report((double)receivedBytes / totalBytes);
                        }
                    }
                }
            }
        }

        private async Task ExtractSubdirectory(ZipArchive archive, ResourcePath fromDirectory, ResourcePath toDirectory, ProgressOperation progress)
        {
            _resourceCache.UserData.CreateDirectory(toDirectory);

            var matching = new List<(ZipArchiveEntry, ResourcePath)>();

            foreach (var entry in archive.Entries)
            {
                if (entry.Name != string.Empty)
                {
                    var isRelative = new ResourcePath(entry.FullName).Directory.TryRelativeTo(fromDirectory, out var relativePath);
                    if (isRelative)
                    {
                        matching.Add((entry, relativePath!));
                    }
                }
            }

            foreach (var ((entry, relativePath), index) in matching.WithIndex())
            {
                var fullPath = toDirectory / relativePath;
                _resourceCache.UserData.CreateDirectory(fullPath.Directory);
                using (var destStream = _resourceCache.UserData.Open(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var sourceStream = entry.Open())
                    {
                        await sourceStream.CopyToAsync(destStream).ConfigureAwait(false);
                        progress.Report((index + 1) / matching.Count);
                    }
                }
            }

            progress.Report(1);
        }

        private async Task UnpackVanillaAssets(ProgressOperation progress)
        {
            using (var fileStream = _resourceCache.UserData.Open(_cachePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                {
                    await ExtractSubdirectory(zip, new ResourcePath("elona/graphic"), _assetsGraphicPath, progress).ConfigureAwait(false);
                    await ExtractSubdirectory(zip, new ResourcePath("elona/sound"), _assetsSoundPath, progress).ConfigureAwait(false);
                }
            }
        }

        public IEnumerator<ProgressStep> GetEnumerator()
        {
            yield return new ProgressStep("Downloading 1.22...", DownloadElona122Zip);
            yield return new ProgressStep("Unpacking assets...", UnpackVanillaAssets);
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
