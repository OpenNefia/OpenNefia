using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Material;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
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

        [DataField]
        public PrototypeId<MaterialSpotPrototype>? MaterialSpotType { get; set; } = null;
    }
}
