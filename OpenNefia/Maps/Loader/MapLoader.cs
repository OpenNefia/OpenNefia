using OdinSerializer;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Instanced;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Maps
{
    public sealed class MapLoader : IMapLoader
    {
        private const int MapFormatVersion = 1;

        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IResourceManager _resourceManager = default!;
        [Dependency] private readonly IInstancedSerializer _serializer = default!;

        public void LoadMap(MapId mapId, ResourcePath filepath)
        {
            using (var stream = _resourceManager.UserData.OpenRead(filepath))
            {
                using (var zipStream = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var context = new MapContext(_serializer, zipStream);
                    context.Deserialize();
                }
            }
        }

        public void SaveMap(MapId mapId, ResourcePath filepath)
        {
            // var map = _mapManager.GetMap(mapId);

            using (var stream = _resourceManager.UserData.OpenWrite(filepath))
            {
                using (var zipStream = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    var context = new MapContext(_serializer, zipStream);
                    context.Serialize();
                }
            }
        }

        /// <summary>
        /// Represents a serialized map file.
        /// </summary>
        /// <remarks>
        /// The saved map format is a zip with these three files. The basic format
        /// is similar to Robust's.
        /// 
        /// - uids: List of valid uids in this map. Used to preallocate entities.
        /// - grid: Size/tiles/flags/memory of the map.
        /// - entities: Entity data for the map.
        /// </remarks>
        private class MapContext
        {
            public List<int> Uids = new();
            public IMap? Map;

            private readonly IInstancedSerializer _serializer;
            private readonly ZipArchive _zipFile;

            public MapContext(IInstancedSerializer serializer, ZipArchive zipFile)
            {
                _zipFile = zipFile;
                _serializer = serializer;
            }

            private T ReadFromZip<T>(string filename)
            {
                var entry = _zipFile.GetEntry(filename)!;

                using (var stream = entry.Open())
                {
                    // The DeflateStream returned by ZipArchiveEntry.Open() doesn't
                    // support Stream.Length, but Odin uses it internally.
                    var content = stream.CopyToArray();

                    return _serializer.DeserializeValue<T>(content);
                }
            }

            private void WriteToZip<T>(string filename, T value)
            {
                var entry = _zipFile.CreateEntry(filename);

                using (var stream = entry.Open())
                {
                    _serializer.SerializeValue(value, stream);
                }
            }

            public void Deserialize()
            {
                ReadMetaSection();
            }

            private void ReadMetaSection()
            {
                var meta = ReadFromZip<MapContextMeta>(MapContextMeta.FileName);

                if (meta.Version != MapFormatVersion)
                {
                    throw new InvalidDataException($"Cannot handle map file version {meta.Version}. (expected: {MapFormatVersion})");
                }
            }

            public void Serialize()
            {
                WriteMetaSection();
            }

            private void WriteMetaSection()
            {
                var meta = new MapContextMeta()
                {
                    Version = MapFormatVersion
                };

                WriteToZip(MapContextMeta.FileName, meta);
            }
        }

        [Serializable]
        private class MapContextMeta
        {
            public const string FileName = "meta";

            [OdinSerialize]
            public int Version { get; set; }
        }
    }
}
