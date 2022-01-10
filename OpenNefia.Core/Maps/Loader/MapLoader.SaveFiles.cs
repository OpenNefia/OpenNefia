using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Log;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Maps
{
    public sealed partial class MapLoader
    {
        public event MapDeletedDelegate? OnMapDeleted;
        
        /// <inheritdoc/>
        public bool MapExistsInSave(MapId id, ISaveGameHandle save)
        {
            var mapFile = GetMapFilePath(id);

            return save.Files.Exists(mapFile);
        }

        /// <inheritdoc/>
        public bool TryLoadMap(MapId mapId, ISaveGameHandle save, [NotNullWhen(true)] out IMap? map)
        {
            var mapFile = GetMapFilePath(mapId);

            if (!save.Files.Exists(mapFile))
            {
                Logger.ErrorS("map.load", $"Map file '{mapFile}' does not exist in the save.");
                map = null;
                return false;
            }

            map = LoadMap(mapId, save);
            return true;
        }

        /// <inheritdoc/>
        public void SaveMap(MapId mapId, ISaveGameHandle save)
        {
            var filepath = GetMapFilePath(mapId);

            Logger.InfoS(SawmillName, $"Saving map {mapId} to {filepath}...");

            save.Files.CreateDirectory(filepath.Directory);

            using (var writer = save.Files.OpenWriteTextCompressed(filepath))
            {
                DoMapSave(mapId, writer, MapSerializeMode.Full);
            }
        }

        /// <inheritdoc/>
        public IMap LoadMap(MapId mapId, ISaveGameHandle save)
        {
            var filepath = GetMapFilePath(mapId);

            Logger.InfoS(SawmillName, $"Loading map {mapId} from {filepath}...");

            using (var reader = save.Files.OpenTextCompressed(filepath))
            {
                return DoMapLoad(mapId, reader, MapSerializeMode.Full);
            }
        }

        /// <inheritdoc/>
        public void DeleteMap(MapId mapId, ISaveGameHandle save)
        {
            if (_mapManager.MapIsLoaded(mapId))
            {
                throw new InvalidOperationException($"Attempted to delete loaded map {mapId}");
            }
            if (!MapExistsInSave(mapId, save))
            {
                throw new FileNotFoundException($"Map {mapId} does not exist in the save.");
            }

            var filepath = GetMapFilePath(mapId);

            Logger.InfoS(SawmillName, $"Deleting map {mapId} from {filepath}...");

            save.Files.Delete(filepath);
            OnMapDeleted?.Invoke(mapId);
        }

        /// <summary>
        /// Returns the resource path for a saved map, scoped to an individual save folder.
        /// </summary>
        public static ResourcePath GetMapFilePath(MapId mapId)
        {
            return new ResourcePath($"/Maps/{mapId}.yml.gz");
        }
    }
}
