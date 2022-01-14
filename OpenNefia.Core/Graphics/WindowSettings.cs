namespace OpenNefia.Core.Graphics
{
    /// <summary>
    /// Backend-agnostic version of <see cref="Love.WindowSettings"/>.
    /// </summary>
    public sealed class WindowSettings
    {
        /// <summary>
        /// Fullscreen (true), or windowed (false).
        /// </summary>
        public bool Fullscreen { get; set; }

        /// <summary>
        /// Choose between "DeskTop" fullscreen or "Exclusive" fullscreen mode 
        /// </summary>
        public FullscreenType FullscreenType { get; set; }

        /// <summary>
        /// True if LÖVE should wait for vsync, false otherwise.
        /// </summary>
        public bool VSync { get; set; }

        /// <summary>
        /// The number of antialiasing samples.
        /// </summary>
        public int MSAA { get; set; }

        /// <summary>
        /// Whether a stencil buffer should be allocated. If true, the stencil buffer will have 8 bits.
        /// </summary>
        public bool Stencil { get; set; }

        /// <summary>
        /// The number of bits in the depth buffer.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Let the window be user-resizable
        /// </summary>
        public bool Resizable { get; set; }

        /// <summary>
        /// Remove all border visuals from the window
        /// </summary>
        public bool Borderless { get; set; }

        /// <summary>
        /// True if the window should be centered.
        /// </summary>
        public bool Centered { get; set; }

        /// <summary>
        /// The index of the display to show the window in, if multiple monitors are available.
        /// </summary>
        public int Display { get; set; }

        /// <summary>
        /// True if high-dpi mode should be used on Retina displays in macOS and iOS. Does nothing on non-Retina displays. Added in 0.9.1.
        /// </summary>
        public bool HighDPI { get; set; }

        /// <summary>
        /// The minimum width of the window, if it's resizable. Cannot be less than 1.
        /// </summary>
        public int MinWidth { get; set; }

        /// <summary>
        /// The minimum height of the window, if it's resizable. Cannot be less than 1.
        /// </summary>
        public int MinHeight { get; set; }

        /// <summary>
        /// True if use the position params, false otherwise.
        /// </summary>
        public bool UsePosition { get; set; }

        /// <summary>
        /// The x-coordinate of the window's position in the specified display.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The x-coordinate of the window's position in the specified display.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// We don't explicitly set the refresh rate, it's "read-only".
        /// <para>The refresh rate of the screen's current display mode, in Hz. May be 0 if the value can't be determined. Added in 0.9.2.</para>
        /// </summary>
        public double RefreshRate { get; set; }
    }
}
