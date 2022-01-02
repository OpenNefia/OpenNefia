using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests
{
    /// <summary>
    /// https://codereview.stackexchange.com/a/241031
    /// </summary>
    public class TempFolder : IDisposable
    {
        private static readonly Random _Random = new Random();

        public DirectoryInfo Folder { get; }

        public TempFolder(string prefix = "TempFolder")
        {
            string folderName;

            lock (_Random)
            {
                folderName = prefix + _Random.Next(1000000000);
            }

            Folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), folderName));
        }

        public void Dispose()
        {
            Directory.Delete(Folder.FullName, true);
        }
    }

    public class TempWritableDirProvider : IWritableDirProvider, IDisposable
    {
        private readonly TempFolder _tempFolder;
        private readonly WritableDirProvider _dirProvider;

        public string RootDir => _tempFolder.Folder.ToString();

        public TempWritableDirProvider(string prefix = "TempFolder")
        {
            _tempFolder = new TempFolder(prefix);
            _dirProvider = new WritableDirProvider(_tempFolder.Folder);
        }

        public void Dispose()
        {
            _tempFolder.Dispose();
        }

        public IWritableDirProvider GetChild(ResourcePath path)
        {
            return _dirProvider.GetChild(path);
        }

        public void CreateDirectory(ResourcePath path)
        {
            _dirProvider.CreateDirectory(path);
        }

        public void Delete(ResourcePath path)
        {
            _dirProvider.Delete(path);
        }

        public bool Exists(ResourcePath path)
        {
            return _dirProvider.Exists(path);
        }

        public (IEnumerable<ResourcePath> files, IEnumerable<ResourcePath> directories) Find(string pattern, bool recursive = true)
        {
            return _dirProvider.Find(pattern, recursive);
        }

        public bool IsDirectory(ResourcePath path)
        {
            return _dirProvider.IsDirectory(path);
        }

        public Stream Open(ResourcePath path, FileMode fileMode, FileAccess access, FileShare share)
        {
            return _dirProvider.Open(path, fileMode, access, share);
        }

        public void Rename(ResourcePath oldPath, ResourcePath newPath)
        {
            _dirProvider.Rename(oldPath, newPath);
        }
    }
}