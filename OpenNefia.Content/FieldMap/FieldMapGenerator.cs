using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Random;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;

namespace OpenNefia.Content.FieldMap
{
    [ImplicitDataDefinitionForInheritors]
    public interface IMapGenerator
    {
        IMap? Generate(MapGeneratorOptions opts);
    }

    public class MapGeneratorOptions
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    [Prototype("Elona.FieldType")]
    public class FieldTypePrototype : IPrototype
    {
        /// <inheritdoc />
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField]
        public PrototypeId<TilePrototype> DefaultTile { get; set; } = Protos.Tile.Grass;

        [DataField]
        public PrototypeId<TilePrototype> FogTile { get; set; } = Protos.Tile.WallForestFog;

        [DataField]
        public List<FieldMapTile> Tiles { get; set; } = new();
    }

    [DataDefinition]
    public class FieldMapTile
    {
        [DataField("id")]
        public PrototypeId<TilePrototype> ID { get; set; } = Protos.Tile.Grass;

        [DataField]
        public int Density { get; set; } = 1;
    }

    public class FieldMapGenerator : IMapGenerator
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;

        /// <summary>
        /// The type of field map to generate.
        /// </summary>
        [DataField]
        public PrototypeId<FieldTypePrototype> FieldMap { get; set; } = Protos.FieldMap.Plains;

        public IMap? Generate(MapGeneratorOptions opts)
        {
            var proto = _prototypeManager.Index(FieldMap);

            var map = _mapManager.CreateMap(opts.Width, opts.Height);
            map.Clear(proto.DefaultTile);

            foreach (var tile in proto.Tiles)
            {
                SprayTile(map, tile.ID, tile.Density, _random);
            }

            var metaData = _entMan.EnsureComponent<MetaDataComponent>(map.MapEntityUid);
            metaData.DisplayName = Loc.GetPrototypeStringOpt(FieldMap, "Name");

            // TODO create_junk_items()

            return map;
        }

        public static void SprayTile(IMap map, PrototypeId<TilePrototype> tile, int density,
            IRandom random)
        {
            var tileAmount = map.Width * map.Height * density / 100 + 1;

            for (int i = 0; i < tileAmount; i++)
            {
                var pos = random.NextVec2iInBounds(map.Bounds);
                map.SetTile(pos, tile);
            }
        }
    }
}

namespace OpenNefia.Content.Prototypes
{
    using FieldMapPrototypeId = PrototypeId<FieldMap.FieldTypePrototype>;

    public static partial class Protos
    {
        public static class FieldMap
        {
            public static readonly FieldMapPrototypeId Plains = new("Elona.Plains");
            public static readonly FieldMapPrototypeId Grassland = new("Elona.Grassland");
            public static readonly FieldMapPrototypeId Desert = new("Elona.Desert");
            public static readonly FieldMapPrototypeId Sea = new("Elona.Sea");
            public static readonly FieldMapPrototypeId Forest = new("Elona.Forest");
            public static readonly FieldMapPrototypeId SnowField = new("Elona.SnowField");
        }
    }
}
