using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.World;
using OpenNefia.Content.MaterialSpot;
using OpenNefia.Core.Maths;
using YamlDotNet.Core;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Holds some common data for all maps.
    /// This can be broken up later if desirable.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapCommonComponent : Component, IHspIds<int>
    {
        /// <inheritdoc/>
        [DataField]
        public HspIds<int>? HspIds { get; }

        [DataField]
        public PrototypeId<MusicPrototype>? Music { get; set; }

        [DataField]
        public int? FloorNumber { get; set; } = null;

        /// <summary>
        /// If true, this map is treated as an indoor map. Weather and day-night cycle effects will not render if the map is indoors.
        /// </summary>
        [DataField]
        public bool IsIndoors { get; set; } = false;

        /// <summary>
        /// If true, this map will be deleted after it has been moved out of using the <see cref="IMapTransferSystem"/>.
        /// If false, the map will be automatically saved to the current save game when it is left.
        /// /// </summary>
        [DataField]
        public bool IsTemporary { get; set; } = false;

        [DataField]
        public bool IsRenewable { get; set; } = true;

        [DataField]
        public PrototypeId<MapTilesetPrototype> Tileset { get; set; } = Protos.MapTileset.Default;

        [DataField]
        public PrototypeId<TilePrototype> FogTile { get; set; } = Protos.Tile.WallDirtFog;

        [DataField]
        public PrototypeId<MaterialSpotPrototype>? MaterialSpotType { get; set; } = null;

        [DataField]
        public GameDateTime RenewMajorDate { get; set; } = new();

        [DataField]
        public GameDateTime RenewMinorDate { get; set; } = new();

        /// <summary>
        /// If true, the player can gain traveling experience by entering this map.
        /// </summary>
        [DataField]
        public bool IsTravelDestination { get; set; } = false;

        /// <summary>
        /// If true, all tiles in the map will be memorized each time the map is entered.
        /// </summary>
        [DataField]
        public bool? RevealsFog { get; set; }

        [DataField]
        public int? ExperienceDivisor { get; set; }

        [DataField]
        public int? MaxItemsOnGround { get; set; }

        [DataField]
        public bool PreventsTeleport { get; set; }
    }
}
