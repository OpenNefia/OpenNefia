using OpenNefia.Core.Maths;
using System;

namespace OpenNefia.Core.Rendering
{
    internal class ChipBatchEntry : IComparable<ChipBatchEntry>
    {
        public MapObjectMemory Memory;
        public AtlasTile AtlasTile;
        public int RowIndex;
        public Vector2i ScrollOffset;

        public ChipBatchEntry(AtlasTile atlasTile, MapObjectMemory memory)
        {
            Memory = memory;
            AtlasTile = atlasTile;
            RowIndex = memory.Coords.Y;
        }

        public int CompareTo(ChipBatchEntry? other)
        {
            if (this.Memory.ZOrder == other?.Memory.ZOrder)
                return this.Memory.Index.CompareTo(other?.Memory.Index);

            return this.Memory.ZOrder.CompareTo(other?.Memory.ZOrder);
        }
    }
}
