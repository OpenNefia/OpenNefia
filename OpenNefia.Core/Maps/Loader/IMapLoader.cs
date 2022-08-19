using OpenNefia.Core.GameObjects;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Maps
{
    public delegate void BlueprintEntityStartupDelegate(EntityUid entity);
    public delegate void MapDeletedDelegate(MapId mapId);

    public interface IMapLoader
    {
        public event BlueprintEntityStartupDelegate OnBlueprintEntityStartup;

        event MapDeletedDelegate? OnMapDeleted;

        /// <summary>
        /// Saves a blueprint to the user data folder.
        /// </summary>
        /// <param name="mapId">ID of the map to convert to a blueprint.</param>
        /// <param name="filepath">Path of the blueprint YAML file under /UserData.</param>
        void SaveBlueprint(MapId mapId, ResourcePath filepath);

        /// <summary>
        /// Saves a blueprint to a YAML stream.
        /// </summary>
        /// <param name="mapId">ID of the map to create and load into.</param>
        /// <param name="yamlStream">Text stream to write YAML content to.</param>
        /// <returns></returns>
        void SaveBlueprint(MapId mapId, TextWriter yamlStream);

        /// <summary>
        /// Loads a blueprint from the user data folder or from within a resource root.
        /// </summary>
        /// <param name="mapId">ID of the map to create and load into.</param>
        /// <param name="filepath">Path of the blueprint YAML file under /UserData or a resource root.</param>
        /// <returns></returns>
        IMap LoadBlueprint(ResourcePath filepath);

        /// <summary>
        /// Loads a blueprint from a YAML stream.
        /// </summary>
        /// <param name="mapId">ID of the map to create and load into.</param>
        /// <param name="yamlStream">Text stream containing YAML content.</param>
        /// <returns></returns>
        IMap LoadBlueprint(TextReader yamlStream);

        /// <summary>
        /// Saves a map to a save file.
        /// </summary>
        /// <param name="mapId">ID of the map to save.</param>
        /// <param name="save">The save file to save to.</param>
        void SaveMap(MapId mapId, ISaveGameHandle save);

        /// <summary>
        /// Loads a map from a save file.
        /// </summary>
        /// <param name="mapId">ID of the map to load.</param>
        /// <param name="save">The save file to load from.</param>
        /// <exception cref="FileNotFoundException">If the map does not exist in the save.</exception>
        IMap LoadMap(MapId mapId, ISaveGameHandle save);

        /// <summary>
        /// Tries to load this map from the save file.
        /// </summary>
        /// <param name="mapId">ID of the map to load.</param>
        /// <param name="save">The save file to load from.</param>
        /// <param name="map">The map, if it exists.</param>
        /// <returns>True if the map was loaded.</returns>
        bool TryLoadMap(MapId mapId, ISaveGameHandle save, [NotNullWhen(true)] out IMap? map);

        /// <summary>
        /// Deletes a map in the save. The map must be unloaded first.
        /// </summary>
        /// <param name="mapId">ID of the map to delete.</param>
        /// <param name="save">The save file to delete from.</param>
        /// <exception cref="InvalidOperationException">If the map is still loaded.</exception>
        /// <exception cref="FileNotFoundException">If the map does not exist in the save.</exception>
        void DeleteMap(MapId mapId, ISaveGameHandle save);

        /// <summary>
        /// Returns true if the map with this ID exists in the save file.
        /// </summary>
        bool MapExistsInSave(MapId id, ISaveGameHandle save);

        Dictionary<string, HashSet<ErrorNode>> ValidateDirectory(ResourcePath path);
    }
}
