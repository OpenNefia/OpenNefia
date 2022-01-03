using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Profiles;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Utility;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Core.SaveGames
{
    /// <summary>
    /// Manages reading and writing data to the user's save game folders. This
    /// interface is solely concerned with file management, and has no logic for the
    /// actual game save format.
    /// </summary>
    public interface ISaveGameManager
    {
        ISaveGameHandle? CurrentSave { get; }
        IEnumerable<ISaveGameHandle> AllSaves { get; }

        ISaveGameHandle CreateSave(ResourcePath saveDirectory, SaveGameHeader header);
        bool ContainsSave(ISaveGameHandle save);
        void DeleteSave(ISaveGameHandle save);
        void SetCurrentSave(ISaveGameHandle save);
    }

    internal interface ISaveGameManagerInternal : ISaveGameManager
    {
        void Initialize();
        void RescanSaves();
    }

    public interface ISaveGameHandle
    {
        ResourcePath SaveDirectory { get; }
        DateTime LastSaveDate { get; }
        SaveGameHeader Header { get; }
        ISaveGameDirProvider Files { get; }
    }

    public class SaveGameHandle : ISaveGameHandle
    {
        public ResourcePath SaveDirectory { get; }
        public DateTime LastSaveDate { get; internal set; }
        public SaveGameHeader Header { get; }
        public ISaveGameDirProvider Files { get; }

        internal SaveGameHandle(IWritableDirProvider tempDir, IWritableDirProvider saveDir, ResourcePath savePath, SaveGameHeader header)
        {
            SaveDirectory = savePath;
            Header = header;

            Files = new SaveGameDirProvider(tempDir, saveDir);
        }
    }

    [DataDefinition]
    public class SaveGameHeader
    {
        /// <summary>
        /// Name of this save.
        /// </summary>
        [DataField(required: true)]
        public string Name { get; } = default!;

        /// <summary>
        /// Assembly version of the engine.
        /// </summary>
        [DataField(required: true)]
        public Version EngineVersion { get; } = new();

        /// <summary>
        /// Full Git commit hash of the engine.
        /// </summary>
        [DataField(required: true)]
        public string EngineCommitHash { get; } = default!;

        [DataField("assemblyVersions", required: true)]
        private readonly Dictionary<string, Version> _assemblyVersions = new();

        /// <summary>
        /// Versions of loaded content assemblies. 
        /// This is a mapping from { strongAssemblyName -> version }.
        /// </summary>
        public IReadOnlyDictionary<string, Version> AssemblyVersions => _assemblyVersions;

        public SaveGameHeader()
        {
        }

        public SaveGameHeader(string name)
        {
            Name = name;
        }

        public SaveGameHeader(string name, Version engineVersion, string engineCommitHash, Dictionary<string, Version> assemblyVersions)
        {
            Name = name;
            EngineVersion = engineVersion;
            EngineCommitHash = engineCommitHash;
            _assemblyVersions = assemblyVersions;
        }
    }

    public class SaveGameManager : ISaveGameManagerInternal
    {
        [Dependency] private readonly ISerializationManager _serializationManager = default!;
        [Dependency] private readonly IProfileManager _profileManager = default!;

        public const string SawmillName = "save";

        public const string SavesPath = "/Saves";
        public const string TempPath = "/Temp";

        private IWritableDirProvider SavesRootDir { get; set; } = default!;
        private IWritableDirProvider TempRootDir { get; set; } = default!;

        private List<ISaveGameHandle> _saves = new();

        public IEnumerable<ISaveGameHandle> AllSaves => _saves;

        public ISaveGameHandle? CurrentSave { get; private set; }

        public void Initialize()
        {
            SavesRootDir = _profileManager.CurrentProfile.GetChild(new ResourcePath(SavesPath));

            var tempResPath = new ResourcePath(TempPath);
            _profileManager.CurrentProfile.Delete(tempResPath);
            TempRootDir = _profileManager.CurrentProfile.GetChild(tempResPath);

            RescanSaves();
        }

        public void RescanSaves()
        {
            CurrentSave = null;
            _saves.Clear();

            foreach (var dir in SavesRootDir.Find("*", recursive: false).directories)
            {
                TryRegisterSave(dir.ChangeSeparator("/"));
            }

            _saves = _saves.OrderBy(save => save.LastSaveDate).ToList();
        }

        private void TryRegisterSave(ResourcePath saveDirectory)
        {
            var saveDirectoryReader = SavesRootDir.GetChild(saveDirectory);

            if (!saveDirectoryReader.Exists(new ResourcePath("/header.yml")))
            {
                Logger.WarningS(SawmillName, $"Missing header.yml in save folder: {saveDirectory}");
                return;
            }

            try
            {
                RegisterSave(saveDirectoryReader, saveDirectory);
            }
            catch (Exception ex)
            {
                Logger.ErrorS(SawmillName, ex, $"Failed to read save at {saveDirectory}");
                return;
            }
        }

        private ISaveGameHandle RegisterSave(IWritableDirProvider saveDirReader, ResourcePath saveDirectory)
        {
            var header = saveDirReader.ReadSerializedData<SaveGameHeader>(
                new ResourcePath("/header.yml"), _serializationManager, skipHook: true)!;

            var tempDirReader = TempRootDir.GetChild(saveDirectory);

            var save = new SaveGameHandle(tempDirReader, saveDirReader, saveDirectory, header);
            _saves.Add(save);

            return save;
        }

        public bool ContainsSave(ISaveGameHandle save)
        {
            return _saves.Contains(save);
        }

        public void SetCurrentSave(ISaveGameHandle save)
        {
            if (!ContainsSave(save))
            {
                throw new ArgumentException($"Save is not registered: {save.Files.RootDir}");
            }

            CurrentSave = save;
            save.Files.ClearTemp();
        }

        public ISaveGameHandle CreateSave(ResourcePath saveDirectory, SaveGameHeader header)
        {
            if (!ResourceManager.IsPathValid(saveDirectory))
            {
                throw new ArgumentException($"Save path contains invalid characters/filenames: {saveDirectory}", nameof(saveDirectory));
            }
            if (saveDirectory.EnumerateSegments().Count() != 1)
            {
                throw new ArgumentException($"Save path must consist of a single directory.", nameof(saveDirectory));
            }
            if (SavesRootDir.Exists(saveDirectory))
            {
                throw new InvalidOperationException($"Save path already exists: {saveDirectory}");
            }

            var saveDirectoryReader = SavesRootDir.GetChild(saveDirectory);

            saveDirectoryReader.WriteSerializedData(
                new ResourcePath("/header.yml"), header, _serializationManager, alwaysWrite: true);

            Logger.InfoS(SawmillName, $"Creating save '{header.Name}' at {saveDirectoryReader.RootDir}.");

            return RegisterSave(saveDirectoryReader, saveDirectory);
        }

        public void DeleteSave(ISaveGameHandle save)
        {
            if (!ContainsSave(save) )
            {
                throw new ArgumentException($"Save is not registered: {save.Files.RootDir}");
            }
            if (CurrentSave == save)
            {
                throw new ArgumentException($"Cannot delete the active save.", nameof(save));
            }

            if (save.Files.RootDir != null)
            {
                SavesRootDir.Delete(save.SaveDirectory);
                TempRootDir.Delete(save.SaveDirectory);
            }

            _saves.Remove(save);
        }
    }
}
