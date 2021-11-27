namespace OpenNefia.Core.Rendering
{
    public enum ShadowTile : short
    {
        /// <summary>
        /// If set, this tile is treated as a shadow/not visible.
        /// If unset, there can still be shadow corner pieces to draw on the tile.
        /// </summary>
        IsShadow      = 0b100000000,

        None          = 0b000000000,
        Northeast     = 0b010000000,
        Southwest     = 0b001000000,
        Southeast     = 0b000100000,
        Northwest     = 0b000010000,
        East          = 0b000001000,
        South         = 0b000000100,
        North         = 0b000000010,
        West          = 0b000000001,

        Intercardinal = 0b011110000,
        Cardinal      = 0b000001111,
    }
}