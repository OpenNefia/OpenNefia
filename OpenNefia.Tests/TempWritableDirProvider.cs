using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Utility;

namespace OpenNefia.Tests
{
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