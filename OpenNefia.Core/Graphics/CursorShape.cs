namespace OpenNefia.Core.Graphics
{
    /// <summary>
    ///     Default common cursor shapes available in the UI.
    ///     Synchronized with <see cref="Love.SystemCursor"/>.
    /// </summary>
    public enum CursorShape : int
    {
        /// <summary>
        /// An arrow pointer.
        /// </summary>
        Arrow,

        /// <summary>
        /// An I-beam, normally used when mousing over editable or selectable text.
        /// </summary>
        IBeam,

        /// <summary>
        /// Wait graphic.
        /// </summary>
        Wait,

        /// <summary>
        /// Crosshair symbol.
        /// </summary>
        Crosshair,

        /// <summary>
        /// Small wait cursor with an arrow pointer.
        /// </summary>
        WaitArrow,

        /// <summary>
        /// Double arrow pointing to the top-left and bottom-right.
        /// </summary>
        SizeNWSE,

        /// <summary>
        /// Double arrow pointing to the top-right and bottom-left.
        /// </summary>
        SizeNESW,

        /// <summary>
        /// Double arrow pointing left and right.
        /// </summary>
        SizeWE,

        /// <summary>
        /// Double arrow pointing up and down.
        /// </summary>
        SizeNS,

        /// <summary>
        /// Four-pointed arrow pointing up, down, left, and right.
        /// </summary>
        SizeAll,

        /// <summary>
        /// Slashed circle or crossbones.
        /// </summary>
        No,

        /// <summary>
        /// Hand symbol.
        /// </summary>
        Hand,
    }
}