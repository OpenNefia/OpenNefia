namespace OpenNefia.Core.Graphics
{
    /// <summary>
    /// <para>Types of fullscreen modes.</para>
    /// </summary>
    /// <remarks>
    /// Save as <see cref="Love.FullscreenType"/>.
    /// </remarks>
    public enum FullscreenType
    {
        /// <summary>
        /// Standard exclusive-fullscreen mode. Changes the display mode (actual resolution) of the monitor.
        /// </summary>
        Exclusive,

        /// <summary>
        /// Sometimes known as borderless fullscreen windowed mode. A borderless screen-sized window is created which sits on top of all desktop UI elements. The window is automatically resized to match the dimensions of the desktop, and its size cannot be changed.
        /// </summary>
        Desktop,
    }
}
