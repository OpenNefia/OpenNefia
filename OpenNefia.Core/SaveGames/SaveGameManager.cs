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
    /// Manages reading and writing data to the user's save game folders.
    /// </summary>
    public interface ISaveGameManager
    {
        ISaveGameHandle? CurrentSave { get; }
        IEnumerable<ISaveGameHandle> AllSaves { get; }

        void Initialize();

        ISaveGameHandle CreateSave(ResourcePath saveDirectory, SaveGameHeader header);
        bool ContainsSave(ISaveGameHandle save);
        void DeleteSave(ISaveGameHandle save);
        void SetCurrentSave(ISaveGameHandle save);
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

        internal SaveGameHandle(IWritableDirProvider saveDir, ResourcePath savePath, SaveGameHeader header)
        {
            SaveDirectory = savePath;
            Header = header;

            Files = new SaveGameDirProvider(saveDir, new VirtualWritableDirProvider());
        }
    }

    [DataDefinition]
    public class SaveGameHeader
    {
        [DataField(required: true)]
        public string Name { get; } = default!;

        public SaveGameHeader()
        {
        }
        
        public SaveGameHeader(string saveName)
        {
            Name = saveName;
        }
    }

    public class SaveGameManager : ISaveGameManager
    {
        [Dependency] private readonly ISerializationManager _serializationManager = default!;
        [Dependency] private readonly IProfileManager _profileManager = default!;

        public const string SawmillName = "save";

        private const string SavesPath = "/Save";

        private IWritableDirProvider SavesRootDir { get; set; } = default!;

        private List<ISaveGameHandle> _saves = new();

        public IEnumerable<ISaveGameHandle> AllSaves => _saves;

        public ISaveGameHandle? CurrentSave { get; private set; }

        public void Initialize()
        {
            SavesRootDir = _profileManager.CurrentProfile.GetChild(new ResourcePath(SavesPath));

            RescanSaves();
        }

        private void RescanSaves()
        {
            CurrentSave = null;
            _saves.Clear();

            foreach (var dir in SavesRootDir.Find("*", recursive: false).directories)
            {
                TryRegisterSave(dir);
            }

            _saves = _saves.OrderBy(save => save.LastSaveDate).ToList();
        }

        private void TryRegisterSave(ResourcePath saveDirectory)
        {
            var headerFile = saveDirectory / "header.yml";

            if (!SavesRootDir.Exists(headerFile))
            {
                Logger.WarningS(SawmillName, $"Missing header.yml in save folder: {saveDirectory}");
                return;
            }

            try
            {
                var saveDirectoryReader = SavesRootDir.GetChild(saveDirectory);
                RegisterSave(saveDirectoryReader, saveDirectory);
            }
            catch (Exception ex)
            {
                Logger.ErrorS(SawmillName, ex, $"Failed to read save at {saveDirectory}");
                return;
            }
        }

        private ISaveGameHandle RegisterSave(IWritableDirProvider saveDirectoryReader, ResourcePath saveDirectory)
        {
            var yaml = saveDirectoryReader.ReadAllYaml(new ResourcePath("/header.yml"));
            var node = yaml.Documents[0].RootNode;
            var header = _serializationManager.ReadValue<SaveGameHeader>(node.ToDataNode(), skipHook: true)!;

            var save = new SaveGameHandle(saveDirectoryReader, saveDirectory, header);
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

            var headerFile = new ResourcePath("/header.yml");
            var node = _serializationManager.WriteValue(header, true);
            saveDirectoryReader.WriteAllYaml(headerFile, node.ToYamlNode());

            Logger.InfoS(SawmillName, $"Created save '{header.Name}' at {saveDirectoryReader.RootDir}.");

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
            }

            _saves.Remove(save);
        }
    }
}
