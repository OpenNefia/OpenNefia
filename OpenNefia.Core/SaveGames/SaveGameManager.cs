using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Profiles;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Utility;
using System.Diagnostics;
using System.Reflection;
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
        ISaveGameHandle? CurrentSave { get; set; }
        IEnumerable<ISaveGameHandle> AllSaves { get; }

        ISaveGameHandle CreateSave(ResourcePath saveDirectory, SaveGameHeader header);
        bool ContainsSave(ISaveGameHandle save);
        void DeleteSave(ISaveGameHandle save);
    }

    internal interface ISaveGameManagerInternal : ISaveGameManager
    {
        void Initialize();
        void RescanSaves();
    }

    public interface ISaveGameHandle
    {
        int SaveFormatVersion { get; }
        ResourcePath SaveDirectory { get; }
        DateTime LastWriteTime { get; set; }
        SaveGameHeader Header { get; }
        ISaveGameDirProvider Files { get; }
    }

    internal class SaveGameHandle : ISaveGameHandle
    {
        public int SaveFormatVersion => SaveGameManager.SaveFormatVersion;
        public ResourcePath SaveDirectory { get; }
        public DateTime LastWriteTime { get; set; }
        public SaveGameHeader Header { get; }
        public ISaveGameDirProvider Files { get; }

        internal SaveGameHandle(IWritableDirProvider tempDir, IWritableDirProvider saveDir, ResourcePath savePath, SaveGameHeader header)
        {
            SaveDirectory = savePath;
            Header = header;

            Files = new SaveGameDirProvider(tempDir, saveDir);
        }

        public override string ToString()
        {
            return $"Name={Header.Name}, SaveDirectory={SaveDirectory}";
        }
    }

    [DataDefinition]
    public sealed class AssemblyMetaData
    {
        /// <summary>
        /// Full name the assembly from <see cref="AssemblyName.FullName"/>.
        /// </summary>
        [DataField]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Version of the assembly from <see cref="AssemblyName.Version"/>.
        /// </summary>
        [DataField]
        public Version Version { get; set; } = new();

        /// <summary>
        /// Assembly informational version, which would include the Git commit hash.
        /// </summary>
        [DataField]
        public string? InformationalVersion { get; set; } = null;

        public static AssemblyMetaData FromAssembly(Assembly assembly)
        {
            var name = assembly.GetName();

            if (name == null)
                throw new InvalidOperationException($"Assembly {assembly} lacks a name!");

            if (name.Version == null)
                throw new InvalidOperationException($"Assembly {assembly} lacks a version!");

            var infoVersion = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            return new()
            {
                FullName = name.FullName,
                Version = name.Version,
                InformationalVersion = infoVersion
            };
        }
    }

    [DataDefinition]
    public sealed class SaveGameHeader
    {
        /// <summary>
        /// Name of this save.
        /// </summary>
        [DataField(required: true)]
        public string Name { get; } = default!;

        [DataField("assemblyMetaData", required: true)]
        private readonly List<AssemblyMetaData> _assemblyMetaData = new();

        /// <summary>
        /// Versions of loaded content assemblies. 
        /// </summary>
        public IReadOnlyList<AssemblyMetaData> AssemblyMetaData => _assemblyMetaData;

        public SaveGameHeader()
        {
        }

        public SaveGameHeader(string name)
        {
            Name = name;
        }

        public SaveGameHeader(string name, List<AssemblyMetaData> assemblyVersions)
        {
            Name = name;
            _assemblyMetaData = assemblyVersions;
        }
    }

    public class SaveGameManager : ISaveGameManagerInternal
    {
        [Dependency] private readonly ISerializationManager _serializationManager = default!;
        [Dependency] private readonly IProfileManager _profileManager = default!;

        public const int SaveFormatVersion = 1;

        public const string SawmillName = "save";

        public const string SavesPath = "/Saves";
        public const string TempPath = "/Temp";

        private IWritableDirProvider SavesRootDir { get; set; } = default!;
        private IWritableDirProvider TempRootDir { get; set; } = default!;

        private List<ISaveGameHandle> _saves = new();

        public IEnumerable<ISaveGameHandle> AllSaves => _saves;

        public ISaveGameHandle? CurrentSave { get; set; }

        public void Initialize()
        {
            SavesRootDir = _profileManager.CurrentProfile.GetChild(new ResourcePath(SavesPath));

            var tempResPath = new ResourcePath(TempPath);
            TempRootDir = _profileManager.CurrentProfile.GetChild(tempResPath);

            try
            {
                _profileManager.CurrentProfile.Delete(tempResPath);
            }
            catch (Exception ex)
            {
                Logger.ErrorS("save", ex, $"Failed to clear save temporary directory: {ex}");
            }

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

            _saves = _saves.OrderBy(save => save.LastWriteTime).ToList();
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
            var headerPath = new ResourcePath("/header.yml");
            var header = saveDirReader.ReadSerializedData<SaveGameHeader>(headerPath, _serializationManager, skipHook: true)!;

            var tempDirReader = TempRootDir.GetChild(saveDirectory);

            var save = new SaveGameHandle(tempDirReader, saveDirReader, saveDirectory, header);
            save.LastWriteTime = saveDirReader.GetLastWriteTime(headerPath);
            _saves.Add(save);

            return save;
        }

        public bool ContainsSave(ISaveGameHandle save)
        {
            return _saves.Contains(save);
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
