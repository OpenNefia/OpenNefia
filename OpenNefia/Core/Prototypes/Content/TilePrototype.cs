using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Prototypes
{
    public enum TileKind
    {
        None = 0,
        Dryground = 1,
        Crop = 2,
        Water = 3,
        Snow = 4,
        MountainWater = 5,
        HardWall = 6,
        Sand = 7,
        SandHard = 8,
        Coast = 9,
        SandWater = 10
    }

    [Prototype("Tile")]
    public class TilePrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField(required: true)]
        public TileSpecifier Image = null!;

        [DataField]
        public int? ElonaAtlas = null;

        [DataField]
        public bool IsSolid = false;

        [DataField]
        public bool IsOpaque = false;

        [DataField]
        public TileSpecifier? WallImage = null;

        [DataField]
        public TileKind Kind = TileKind.None;

        [DataField]
        public TileKind Kind2 = TileKind.None;
    }
}
