using OpenNefia.Core.GameObjects;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Maps
{
    public delegate void BlueprintEntityStartupDelegate(EntityUid entity);

    public interface IMapLoader
    {
        public event BlueprintEntityStartupDelegate OnBlueprintEntityStartup;

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
        IMap LoadBlueprint(MapId mapId, ResourcePath filepath);

        /// <summary>
        /// Loads a blueprint from a YAML stream.
        /// </summary>
        /// <param name="mapId">ID of the map to create and load into.</param>
        /// <param name="yamlStream">Text stream containing YAML content.</param>
        /// <returns></returns>
        IMap LoadBlueprint(MapId mapId, TextReader yamlStream);

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

        bool TryLoadMap(MapId mapId, ISaveGameHandle save, [NotNullWhen(true)] out IMap? map);

        /// <summary>
        /// Returns true if the map with this ID exists in the save file.
        /// </summary>
        bool MapExistsInSave(MapId id, ISaveGameHandle save);
    }
}
