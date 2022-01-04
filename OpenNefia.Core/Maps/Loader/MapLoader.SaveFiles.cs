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

        /// <summary>
        /// Returns the resource path for a saved map, scoped to an individual save folder.
        /// </summary>
        public static ResourcePath GetMapFilePath(MapId mapId)
        {
            return new ResourcePath($"/Maps/{mapId}.yml");
        }
    }
}
