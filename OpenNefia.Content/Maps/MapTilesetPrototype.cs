using OpenNefia.Content.PCCs;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.Maps
{
    [Prototype("Elona.MapTileset")]
    public class MapTilesetPrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField("doorChips")]
        public DoorChips DoorChips { get; } = new();

        [DataField("tiles")]
        private readonly Dictionary<PrototypeId<TilePrototype>, ITilePicker> _tiles = new();

        public IReadOnlyDictionary<PrototypeId<TilePrototype>, ITilePicker> Tiles => _tiles;

        [DataField("fogTile")]
        public ITilePicker? FogTile { get; } = null;
    }

    [DataDefinition]
    public sealed class DoorChips
    {
        [DataField("openChip")]
        public PrototypeId<ChipPrototype>? ChipOpen { get; } = null;

        [DataField("closedChip")]
        public PrototypeId<ChipPrototype>? ChipClosed { get; } = null;
    }

    [ImplicitDataDefinitionForInheritors]
    public interface ITilePicker
    {
        public PrototypeId<TilePrototype> GetTile();
    }

    public sealed class TileSingle : ITilePicker
    {
        [DataField("tile", required: true)]
        public PrototypeId<TilePrototype> Tile { get; } = default!;

        public PrototypeId<TilePrototype> GetTile() => Tile;
    }

    public sealed class TileRandom : ITilePicker
    {
        [DataField("tiles", required: true)]
        private readonly List<PrototypeId<TilePrototype>> _tiles = new();

        public IReadOnlyList<PrototypeId<TilePrototype>> Tiles => _tiles;

        [DataField("pickChance")]
        public float pickChance = 0.5f;

        public PrototypeId<TilePrototype> GetTile()
        {
            var _rand = IoCManager.Resolve<IRandom>();
            if (_rand.Prob(pickChance))
            {
                return _rand.Pick(Tiles);
            }
            else
            {
                return Tiles.First();
            }
        }
    }
}