using JetBrains.Annotations;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    /// Class for loading and saving maps, whether from a blueprint or a saved game.
    /// </summary>
    /// <seealso cref="SerialMapLoader"/>
    public sealed partial class MapLoader : IMapLoader
    {
        public const string SawmillName = "map.load";

        [Dependency] private readonly IResourceManager _resourceManager = default!;

        public event BlueprintEntityStartupDelegate? OnBlueprintEntityStartup;

        /// <inheritdoc />
        public void SaveBlueprint(MapId mapId, ResourcePath resPath)
        {
            Logger.InfoS(SawmillName, $"Saving map blueprint {mapId} to {resPath}...");

            _resourceManager.UserData.CreateDirectory(resPath.Directory);

            using (var writer = _resourceManager.UserData.OpenWriteText(resPath))
            {
                using var profiler = new ProfilerLogger(LogLevel.Debug, SawmillName, $"Map blueprint save: {resPath}");
                SaveBlueprint(mapId, writer);
            }
        }

        /// <inheritdoc />
        public void SaveBlueprint(MapId mapId, TextWriter writer)
        {
            DoMapSave(mapId, writer, MapSerializeMode.Blueprint);
        }

        private static void DoMapSave(MapId mapId, TextWriter writer, MapSerializeMode mode)
        {
            var context = new MapSerializer(mapId, mode);
            var root = context.Serialize();
            var document = new YamlDocument(root);

            var stream = new YamlStream();
            stream.Add(document);
            stream.Save(new YamlMappingFix(new Emitter(writer)), false);
        }

        /// <inheritdoc />
        public IMap LoadBlueprint(MapId mapId, ResourcePath yamlPath)
        {
            TextReader reader;

            // try user
            if (!_resourceManager.UserData.Exists(yamlPath))
            {
                Logger.DebugS(SawmillName, $"No user blueprint path: {yamlPath}");

                // fallback to content
                if (_resourceManager.TryContentFileRead(yamlPath, out var contentReader))
                {
                    reader = new StreamReader(contentReader);
                }
                else
                {
                    throw new ArgumentException($"No blueprint found: {yamlPath}", nameof(yamlPath));
                }
            }
            else
            {
                reader = _resourceManager.UserData.OpenText(yamlPath);
            }

            using (reader)
            {
                using var profiler = new ProfilerLogger(LogLevel.Debug, SawmillName, $"Map blueprint load: {yamlPath}");
                Logger.InfoS(SawmillName, $"Loading map blueprint: {yamlPath}");
                return LoadBlueprint(mapId, reader);
            }
        }

        /// <inheritdoc />
        public IMap LoadBlueprint(MapId mapId, TextReader reader)
        {
            return DoMapLoad(mapId, reader, MapSerializeMode.Blueprint);
        }

        private IMap DoMapLoad(MapId mapId, TextReader reader, MapSerializeMode mode)
        {
            var data = new MapData(reader);

            var deserializer = new MapDeserializer(mapId, mode,
                (YamlMappingNode)data.RootNode, OnBlueprintEntityStartup);
            deserializer.Deserialize();
            var grid = deserializer.MapGrid!;

            foreach (var entityUid in deserializer.Entities)
            {
                MapInitExt.RunMapInit(entityUid);
            }

            return grid;
        }

        /// <inheritdoc/>
        public void SaveMap(MapId mapId, ISaveGameHandle save)
        {
            var filepath = GetMapFilePath(mapId);

            Logger.InfoS(SawmillName, $"Saving map {mapId} to {filepath}...");

            save.Files.CreateDirectory(filepath.Directory);

            using (var writer = save.Files.OpenWriteText(filepath))
            {
                DoMapSave(mapId, writer, MapSerializeMode.Full);
            }
        }

        /// <inheritdoc/>
        public IMap LoadMap(MapId mapId, ISaveGameHandle save)
        {
            var filepath = GetMapFilePath(mapId);

            Logger.InfoS(SawmillName, $"Loading map {mapId} from {filepath}...");

            using (var reader = save.Files.OpenText(filepath))
            {
                return DoMapLoad(mapId, reader, MapSerializeMode.Full);
            }
        }

        /// <summary>
        ///     Does basic pre-deserialization checks on map file load.
        /// </summary>
        private class MapData
        {
            public YamlStream Stream { get; }

            public YamlNode RootNode => Stream.Documents[0].RootNode;

            public MapData(TextReader reader)
            {
                var stream = new YamlStream();
                stream.Load(reader);

                if (stream.Documents.Count < 1)
                {
                    throw new InvalidDataException("Stream has no YAML documents.");
                }

                // Kinda wanted to just make this print a warning and pick [0] but screw that.
                // What is this, a hug box?
                if (stream.Documents.Count > 1)
                {
                    throw new InvalidDataException("Stream too many YAML documents. Map blueprints store exactly one.");
                }

                Stream = stream;
            }
        }
    }
}
