using System;

namespace OpenNefia.Core.Configuration
{
    /// <summary>
    /// Extra flags for changing the behavior of a config var.
    /// </summary>
    [Flags]
    public enum CVar : short
    {
        /// <summary>
        /// No special flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Debug vars that are considered 'cheating' to change.
        /// </summary>
        Cheat = 1,

        /// <summary>
        /// Non-default values are saved to the configuration file.
        /// </summary>
        Archive = 2,
    }
}
