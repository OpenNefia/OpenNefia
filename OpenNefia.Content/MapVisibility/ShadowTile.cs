namespace OpenNefia.Content.MapVisibility
{
    public enum ShadowTile : short
    {
#pragma warning disable format
        None      = 0b000000000,

        /// <summary>
        /// If set, this tile is treated as a shadow/not visible.
        /// If unset, there can still be shadow corner pieces to draw on the tile.
        /// </summary>
        IsShadow  = 0b100000000,
        
        Northeast = 0b010000000,
        Southwest = 0b001000000,
        Southeast = 0b000100000,
        Northwest = 0b000010000,
        East      = 0b000001000,
        South     = 0b000000100,
        North     = 0b000000010,
        West      = 0b000000001,

        Intercardinal = Northeast | Southwest | Southeast | Northwest,
        Cardinal      = 0b000001111,
#pragma warning restore format
    }
}