using System;

namespace OpenNefia.Core.Rendering
{
    internal class ChipBatchEntry : IComparable<ChipBatchEntry>
    {
        public MapObjectMemory Memory;
        public AtlasTile AtlasTile;
        public int RowIndex;
        public int ScrollXOffset = 0;
        public int ScrollYOffset = 0;

        public ChipBatchEntry(AtlasTile atlasTile, MapObjectMemory memory)
        {
            Memory = memory;
            AtlasTile = atlasTile;
            RowIndex = memory.TileY;
        }

        public int CompareTo(ChipBatchEntry? other)
        {
            if (this.Memory.ZOrder == other?.Memory.ZOrder)
                return this.Memory.Index.CompareTo(other?.Memory.Index);

            return this.Memory.ZOrder.CompareTo(other?.Memory.ZOrder);
        }
    }
}
