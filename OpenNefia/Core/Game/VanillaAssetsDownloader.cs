using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;
using System.Collections;
using System.IO.Compression;
using System.Security.Cryptography;

namespace OpenNefia.Core.Game
{
    internal class VanillaAssetsDownloader : IProgressableJob
    {
        [Dependency] private readonly IResourceManager _resourceManager = default!;

        private const string URL_YLVANIA_ELONA122 = "http://ylvania.style.coocan.jp/file/elona122.zip";
        private const string ELONA122_ZIP_SHA256 = "6880f616f34be608435977dd3725d2cc76eaf6d2ad3f40e2d14b36f8f7a802d8";

        private static readonly ResourcePath _assetsGraphicPath = new ResourcePath("/Assets/Elona/Graphic");
        private static readonly ResourcePath _assetsSoundPath = new ResourcePath("/Assets/Elona/Sound");
        private static readonly ResourcePath _elona122ZipPath = new ResourcePath("/Cache/Deps/elona122.zip");

        private readonly WritableDirProvider _assetsDir;

        public VanillaAssetsDownloader(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;

            _assetsDir = new WritableDirProvider(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));
        }

        public bool NeedsDownload()
        {
            return !_assetsDir.IsDirectory(_assetsGraphicPath)
                || !_assetsDir.IsDirectory(_assetsSoundPath);
        }

        public uint NumberOfSteps => 2;
        public float Progress { get; set; }

        private async Task DownloadElona122Zip(ProgressOperation progress)
        {
            if (_resourceManager.UserData.Exists(_elona122ZipPath))
            {
                bool valid = false;
                using (var stream = _resourceManager.UserData.Open(_elona122ZipPath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    var sha256 = SHA256.Create();
                    var bytes = sha256.ComputeHash(stream);
                    valid = bytes.ToHexString() == ELONA122_ZIP_SHA256;
                }
                if (valid)
                {
                    return;
                }
                else
                {
                    Logger.LogS(LogLevel.Warning, CommonSawmills.Boot, "Integrity check of elona122.zip failed, redownloading.");
                    _resourceManager.UserData.Delete(_elona122ZipPath);
                }
            }

            if (!_resourceManager.UserData.IsDirectory(_elona122ZipPath))
                _resourceManager.UserData.CreateDirectory(_elona122ZipPath.Directory);

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(URL_YLVANIA_ELONA122, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    using (var destStream = _resourceManager.UserData.Open(_elona122ZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[1024];
                        int receivedBytes = 0;
                        long totalBytes = response.Content.Headers.ContentLength!.Value;

                        for (;;)
                        {
                            int bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                            if (bytesRead == 0)
                            {
                                progress.Report(1);
                                await Task.Yield();
                                break;
                            }

                            await destStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                            receivedBytes += bytesRead;
                            progress.Report((double)receivedBytes / totalBytes);
                        }
                    }
                }
            }
        }

        private async Task ExtractSubdirectory(ZipArchive archive, ResourcePath fromDirectory, ResourcePath toDirectory, ProgressOperation progress)
        {
            _assetsDir.CreateDirectory(toDirectory);

            var matching = new List<(ZipArchiveEntry, string)>();

            foreach (var entry in archive.Entries)
            {
                if (entry.Name != string.Empty)
                {
                    var isRelative = new ResourcePath(entry.FullName).Directory.TryRelativeTo(fromDirectory, out var _);
                    if (isRelative && entry.Name != string.Empty)
                    {
                        matching.Add((entry, entry.Name));
                    }
                }
            }

            foreach (var ((entry, filename), index) in matching.WithIndex())
            {
                var fullPath = toDirectory / filename;
                _assetsDir.CreateDirectory(fullPath.Directory);
                using (var destStream = _assetsDir.Open(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
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
            using (var fileStream = _assetsDir.Open(_elona122ZipPath, FileMode.Open, FileAccess.Read, FileShare.None))
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
