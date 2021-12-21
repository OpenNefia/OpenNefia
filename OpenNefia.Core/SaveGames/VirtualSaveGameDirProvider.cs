using OpenNefia.Core.Utility;

namespace OpenNefia.Core.SaveGames
{
    internal class VirtualSaveGameDirProvider : ISaveGameDirProvider
    {
        public string? RootDir => throw new NotImplementedException();

        public void ClearTemp()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
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
    }
}