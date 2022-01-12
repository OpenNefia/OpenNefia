using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Prototypes
{
    public static partial class ProtoSets
    {
        public static class Tile
        {
            /// <summary>
            /// Map tiles that count as roads.
            /// </summary>
            public static readonly IReadOnlySet<PrototypeId<TilePrototype>> WorldMapRoadTiles = new HashSet<PrototypeId<TilePrototype>>()
            {
                Protos.Tile.WorldRoadNs,
                Protos.Tile.WorldRoadWe,
                Protos.Tile.WorldRoadSw,
                Protos.Tile.WorldRoadSe,
                Protos.Tile.WorldRoadNw,
                Protos.Tile.WorldRoadNe,
                Protos.Tile.WorldRoadSwe,
                Protos.Tile.WorldRoadNsw,
                Protos.Tile.WorldRoadNwe,
                Protos.Tile.WorldRoadNse,
            };
        }
    }
}
