using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Markdown.Validation;
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
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;

        public event MapDeletedDelegate? OnMapDeleted;
        
        /// <inheritdoc/>
        public bool MapExistsInSave(MapId id, ISaveGameHandle save)
        {
            var mapFile = GetMapFilePath(id);

            return save.Files.Exists(mapFile);
        }


        public bool TryGetOrLoadMap(MapId mapId, [NotNullWhen(true)] out IMap? map)
        {
            if (_saveGameManager.CurrentSave == null)
            {
                Logger.WarningS("map.load", "No active save!");
                map = null;
                return false;
            }

            return TryGetOrLoadMap(mapId, _saveGameManager.CurrentSave, out map);
        }

        public bool TryGetOrLoadMap(MapId mapId, ISaveGameHandle save, [NotNullWhen(true)] out IMap? map)
        {
            // See if this map is still in memory and hasn't been flushed yet.
            if (_mapManager.TryGetMap(mapId, out map))
            {
                Logger.WarningS("map.load", $"Traveling to cached map {map.Id}");
                return true;
            }

            if (!TryLoadMap(mapId, save, out map))
            {
                Logger.ErrorS("map.load", $"Failed to load map {mapId} from disk!");
                return false;
            }

            Logger.InfoS("map.load", $"Loaded map {mapId} from disk.");
            return true;
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
                return;
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
