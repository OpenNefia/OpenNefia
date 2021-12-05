using OdinSerializer;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
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
        [Dependency] private readonly IEntityManagerInternal _entityManager = default!;
        [Dependency] private readonly IResourceManager _resourceManager = default!;
        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly IInstancedSerializer _serializer = default!;

        public void LoadMap(MapId mapId, ResourcePath filepath)
        {
            using (var stream = _resourceManager.UserData.OpenRead(filepath))
            {
                using (var zipStream = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var context = new MapContext(_mapManager, _entityManager, _tileDefinitionManager, _serializer, mapId, zipStream);
                    context.Deserialize();
                }
            }
        }

        public void SaveMap(MapId mapId, ResourcePath filepath)
        {
            using (var stream = _resourceManager.UserData.OpenWrite(filepath))
            {
                using (var zipStream = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    var context = new MapContext(_mapManager, _entityManager, _tileDefinitionManager, _serializer, mapId, zipStream);
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
            public readonly List<Entity> Entities = new();

            private readonly IMapManager _mapManager;
            private readonly IEntityManagerInternal _entityManager;
            private readonly ITileDefinitionManager _tileDefinitionManager;
            private readonly IInstancedSerializer _serializer;
            private readonly MapId _mapId;
            private readonly ZipArchive _zipFile;

            private MapContextMeta _meta = new();
            private MapContextTilemap _tilemap = new();

            public MapContext(IMapManager mapManager,
                IEntityManagerInternal entityManager,
                ITileDefinitionManager tileDefinitionManager,
                IInstancedSerializer serializer,
                MapId mapId, ZipArchive zipFile)
            {
                _mapManager = mapManager;
                _entityManager = entityManager;
                _tileDefinitionManager = tileDefinitionManager;
                _serializer = serializer;
                _mapId = mapId;
                _zipFile = zipFile;
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
                AllocMap();
                AllocEntities();
                ReadTilemap();
            }

            private void ReadMetaSection()
            {
                _meta = ReadFromZip<MapContextMeta>(MapContextMeta.FileName);

                if (_meta.Version != MapFormatVersion)
                {
                    throw new InvalidDataException($"Cannot handle map file version {_meta.Version}. (expected: {MapFormatVersion})");
                }
            }

            private void AllocMap()
            {
                if (_mapManager.MapExists(_mapId))
                {
                    throw new InvalidOperationException($"Map is already loaded in slot {_mapId}.");
                }

                _mapManager.CreateMap(_mapId, _meta.Width, _meta.Height);
            }

            private void AllocEntities()
            {
                var uids = ReadFromZip<MapContextUids>(MapContextUids.FileName); 
                
                foreach (var uid in uids.Uids)
                {
                    var entity = _entityManager.AllocEntity(null, uid);
                    Entities.Add(entity);
                }
            }

            private void ReadTilemap()
            {
                _tilemap = ReadFromZip<MapContextTilemap>(MapContextTilemap.FileName);
            }

            public void Serialize()
            {
                WriteMetaSection();
                WriteEntityUidsSection();
                WriteTilemapSection();
            }

            private void WriteMetaSection()
            {
                var map = _mapManager.GetMap(_mapId);

                var meta = new MapContextMeta()
                {
                    Version = MapFormatVersion,
                    Width = map.Width,
                    Height = map.Height
                };

                WriteToZip(MapContextMeta.FileName, meta);
            }

            private void WriteEntityUidsSection()
            {
                var uids = new MapContextUids();

                foreach (var entity in _entityManager.GetEntities().Where(e => e.Spatial.MapPosition.MapId == _mapId))
                {
                    uids.Uids.Add(entity.Uid);
                }

                WriteToZip(MapContextUids.FileName, uids);
            }

            private void WriteTilemapSection()
            {
                var tilemap = new MapContextTilemap();

                foreach (var tileProto in _tileDefinitionManager)
                {
                    tilemap.Tilemap.Add(tileProto.TileIndex, tileProto.ID);
                }

                WriteToZip(MapContextTilemap.FileName, tilemap);
            }
        }

        [Serializable]
        private class MapContextMeta
        {
            public const string FileName = "meta";

            public int Version;
            public int Width;
            public int Height;
        }

        [Serializable]
        private class MapContextUids
        {
            public const string FileName = "uids";

            public List<EntityUid> Uids = new();
        }

        [Serializable]
        private class MapContextTilemap
        {
            public const string FileName = "tilemap";

            /// <summary>
            /// Mapping of tile index -> PrototypeId(TilePrototype)
            /// </summary>
            public Dictionary<int, string> Tilemap = new();
        }

        [Serializable]
        private class MapContextGrid
        {
            public const string FileName = "tilemap";

            /// <summary>
            /// Mapping of tile index -> PrototypeId(TilePrototype)
            /// </summary>
            public IMap? Grid;
        }
    }
}
