using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameController
{
    public class GameControllerOptions
    {
        /// <summary>
        ///     Name the userdata directory will have.
        /// </summary>
        public string UserDataDirectoryName { get; init; } = "OpenNefia";

        /// <summary>
        ///     Name of the configuration file in the user data directory.
        /// </summary>
        public string ConfigFileName { get; init; } = "client_config.toml";

        // TODO: Define engine branding from json file in resources.
        /// <summary>
        ///     Default window title.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>OpenNefia</c> if unset.
        /// </remarks>
        public string DefaultWindowTitle { get; init; } = "OpenNefia";

        /// <summary>
        ///     Directory to load all assemblies from.
        /// </summary>
        public ResourcePath AssemblyDirectory { get; init; } = new(@"/Assemblies/");

        /// <summary>
        ///     Directory to load all prototypes from.
        /// </summary>
        public ResourcePath PrototypeDirectory { get; init; } = new(@"/Prototypes/");

        /// <summary>
        ///     Directory to load all themes from.
        /// </summary>
        public ResourcePath ThemeDirectory { get; init; } = new(@"/Themes/");

        /// <summary>
        ///     Whether to load config and user data.
        /// </summary>
        public bool LoadConfigAndUserData { get; init; } = true;
    }
}
