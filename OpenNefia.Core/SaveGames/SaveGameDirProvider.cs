using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.SaveGames
{
    internal class SaveGameDirProvider : ISaveGameDirProvider
    {
        public string? RootDir { get; }

        private readonly IWritableDirProvider _committedDir;
        private readonly IWritableDirProvider _tempDir;

        public SaveGameDirProvider(DirectoryInfo rootDir)
        {            
            // FullName does not have a trailing separator, and we MUST have a separator.
            RootDir = rootDir.FullName + Path.DirectorySeparatorChar.ToString();

            var committedDir = Path.Join(RootDir, "Data");
            var tempDir = Path.Join(RootDir, "Temp");

            _committedDir = new WritableDirProvider(Directory.CreateDirectory(committedDir));
            _tempDir = new WritableDirProvider(Directory.CreateDirectory(tempDir));
        }

        public void CreateDirectory(ResourcePath path)
        {
            throw new NotImplementedException();
        }

        public void Delete(ResourcePath path)
        {
            throw new NotImplementedException();
        }

        public bool Exists(ResourcePath path)
        {
            throw new NotImplementedException();
        }

        public (IEnumerable<ResourcePath> files, IEnumerable<ResourcePath> directories) Find(string pattern, bool recursive = true)
        {
            throw new NotImplementedException();
        }

        public bool IsDirectory(ResourcePath path)
        {
            throw new NotImplementedException();
        }

        public Stream Open(ResourcePath path, FileMode fileMode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        public void Rename(ResourcePath oldPath, ResourcePath newPath)
        {
            throw new NotImplementedException();
        }


        public void ClearTemp()
        {
        }

        public void Commit()
        {
        }
    }
}