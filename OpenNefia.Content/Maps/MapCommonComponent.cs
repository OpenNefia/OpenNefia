using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    public class MapCommonComponent : Component, IHspIds<int>
    {
        public override string Name => "MapCommon";

        /// <inheritdoc/>
        [DataField]
        public HspIds<int>? HspIds { get; }

        [DataField]
        public PrototypeId<MusicPrototype>? Music { get; set; }

        [DataField]
        public bool IsIndoors { get; set; } = false;

        [DataField]
        public PrototypeId<MapTilesetPrototype> Tileset { get; set; } = Protos.MapTileset.Default;

        [DataField]
        public PrototypeId<TilePrototype> FogTile { get; set; } = Protos.Tile.WallDirtFog;

        /// <summary>
        /// The maximum number of characters that should be active in this map.
        /// If there are not enough, more will be randomly generated over time.
        /// </summary>
        [DataField]
        public int MaxCrowdDensity { get; set; } = 0;
    }
}
