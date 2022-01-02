using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.SaveGames
{
    internal class SaveGameDirProvider : ISaveGameDirProvider
    {
        /// <summary>
        /// This directory holds the save's temporary data that should be wiped
        /// when loading.
        /// </summary>
        private readonly IWritableDirProvider _tempDir;

        /// <summary>
        /// This directory holds the comitted save data.
        /// </summary>
        private readonly IWritableDirProvider _committedDir;

        /// <summary>
        /// Paths that will be deleted when the save is committed.
        /// </summary>
        private readonly HashSet<ResourcePath> _pendingDeletions = new();

        public string? RootDir => _tempDir.RootDir;

        public SaveGameDirProvider(IWritableDirProvider temp, IWritableDirProvider committed)
        {
            _committedDir = committed;
            _tempDir = temp;
        }

        public IWritableDirProvider GetChild(ResourcePath path)
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(ResourcePath path)
        {
            _tempDir.CreateDirectory(path);
        }

        public void Delete(ResourcePath path)
        {
            if (_tempDir.Exists(path))
            {
                _tempDir.Delete(path);
            }
            _pendingDeletions.Add(path);
        }

        public bool Exists(ResourcePath path)
        {
            return _committedDir.Exists(path) || _tempDir.Exists(path);
        }

        public (IEnumerable<ResourcePath> files, IEnumerable<ResourcePath> directories) Find(string pattern, bool recursive = true)
        {
            var (comittedFiles, committedDirs) = _committedDir.Find(pattern, recursive);
            var (tempFiles, tempDirs) = _tempDir.Find(pattern, recursive);

            var allFiles = tempFiles.Union(comittedFiles)!;
            var allDirs = tempDirs.Union(committedDirs)!;

            return (allFiles, allDirs);
        }

        public bool IsDirectory(ResourcePath path)
        {
            return _committedDir.Exists(path) || _tempDir.Exists(path);
        }

        private static bool IsCreateMode(FileMode fileMode)
        {
            return fileMode switch
            {
                FileMode.Create => true,
                FileMode.CreateNew => true,
                FileMode.OpenOrCreate => true,
                _ => false
            };
        }

        public Stream Open(ResourcePath path, FileMode fileMode, FileAccess access, FileShare share)
        {
            if (!_tempDir.Exists(path) && _committedDir.Exists(path))
            {
                Logger.DebugS("save.writer", $"Copying file as uncommitted: {path}");
                CopyBetweenDirWriters(_committedDir, _tempDir, fileMode, path);
                _pendingDeletions.Remove(path);
            }

            return _tempDir.Open(path, fileMode, access, share);
        }

        private void CopyBetweenDirWriters(IWritableDirProvider from, IWritableDirProvider to, FileMode fileMode, ResourcePath path)
        {
            var dir = path.Directory;
            if (dir.IsRooted)
                to.CreateDirectory(dir);

            using var reader = from.OpenRead(path);
            using var writer = to.Open(path, fileMode, FileAccess.ReadWrite, FileShare.None);
            reader.CopyTo(writer);
        }

        public void Rename(ResourcePath oldPath, ResourcePath newPath)
        {
            if (_tempDir.Exists(oldPath))
            {
                _tempDir.Rename(oldPath, newPath);
            }
            else
            {
                if (_committedDir.IsDirectory(oldPath))
                {
                    _tempDir.CreateDirectory(newPath);
                }
                else if (_committedDir.Exists(oldPath))
                {
                    _tempDir.CreateDirectory(newPath.Directory);
                }
                else
                {
                    throw new FileNotFoundException($"File {oldPath} does not exist in the save's directory. ({_committedDir.RootDir})");
                }

                _pendingDeletions.Add(oldPath);
                _pendingDeletions.Remove(newPath);
            }
        }

        public void ClearTemp()
        {
            Logger.DebugS("save.writer", "Clearing temporary directory");

            var (files, dirs) = _tempDir.Find("*", recursive: false);
        
            foreach (var file in files)
            {
                _tempDir.Delete(file);
            }

            foreach (var dir in dirs)
            {
                _tempDir.Delete(dir);
            }

            _pendingDeletions.Clear();
        }

        /// <inheritdoc/>
        public void Commit()
        {
            var (files, dirs) = _tempDir.Find("*", recursive: true);

            foreach (var file in files)
            {
                CopyBetweenDirWriters(_tempDir, _committedDir, FileMode.OpenOrCreate, file.ToRootedPath());
            }

            foreach (var dir in dirs)
            {
                _committedDir.CreateDirectory(dir);
            }

            foreach (var deletion in _pendingDeletions)
            {
                _committedDir.Delete(deletion);
            }

            ClearTemp();
        }
    }
}