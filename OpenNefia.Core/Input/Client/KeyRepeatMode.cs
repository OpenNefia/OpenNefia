namespace OpenNefia.Core.Input
{
    /// <summary>
    /// When using Elona's key repeat emulation, what type of key repeat to utilize.
    /// </summary>
    public enum KeyRepeatMode
    {
        /// <summary>
        /// Normal key binds.
        /// </summary>
        None,

        /// <summary>
        /// Delay used for movement, which have slightly different repeat delays.
        /// </summary>
        Movement,

        /// <summary>
        /// Delay used for user interfaces.
        /// </summary>
        UserInterface
    }
}
