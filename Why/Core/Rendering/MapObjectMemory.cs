using OpenNefia.Core.Object;
using OpenNefia.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public enum ShadowType
    {
        None,
        Normal,
        DropShadow
    }

    internal enum MemoryState
    {
        Added,
        InUse,
        Removed
    }

    public class MapObjectMemory : IDataExposable
    {
        public ulong ObjectUid;
        public bool IsVisible;
        public string ChipIndex = string.Empty;
        public int ScreenXOffset;
        public int ScreenYOffset;
        public float Rotation;
        public Love.Color Color;
        public ShadowType ShadowType;

        internal int Index;
        internal int TileX;
        internal int TileY;
        internal int ZOrder;
        internal MemoryState State;
        internal Type ObjectType = typeof(MapObject);

        public void Expose(DataExposer data)
        {
            data.ExposeValue(ref ObjectUid, nameof(ObjectUid));
            data.ExposeValue(ref IsVisible, nameof(IsVisible));
            data.ExposeValue(ref ChipIndex!, nameof(ChipIndex));
            data.ExposeValue(ref ScreenXOffset, nameof(ScreenXOffset));
            data.ExposeValue(ref ScreenYOffset, nameof(ScreenYOffset));
            data.ExposeValue(ref Rotation, nameof(Rotation));
            data.ExposeValue(ref Color, nameof(Color));
            data.ExposeValue(ref ShadowType, nameof(ShadowType));

            data.ExposeValue(ref Index, nameof(Index));
            data.ExposeValue(ref TileX, nameof(TileX));
            data.ExposeValue(ref TileY, nameof(TileY));
            data.ExposeValue(ref ZOrder, nameof(ZOrder));
            data.ExposeValue(ref State, nameof(State));
            data.ExposeValue(ref ObjectType!, nameof(ObjectType));
        }
    }
}
