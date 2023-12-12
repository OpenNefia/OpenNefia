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
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Maps;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Levels;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Pickable;

namespace OpenNefia.Content.FieldMap
{
    [ImplicitDataDefinitionForInheritors]
    public interface IMapGenerator
    {
        IMap Generate(MapGeneratorOptions opts);
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
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        public HashSet<PrototypeId<TilePrototype>> WorldMapTiles { get; } = new();

        [DataField]
        public PrototypeId<TilePrototype> DefaultTile { get; set; } = Protos.Tile.Grass;

        [DataField]
        public PrototypeId<TilePrototype> FogTile { get; set; } = Protos.Tile.WallForestFog;

        [DataField]
        public List<FieldMapTile> Tiles { get; } = new();

        [DataField]
        public IFieldMapEvents? Generator { get; set; }
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IFieldMapEvents
    {
        void OnGenerate(IMap map);
    }

    public sealed class FieldMapGeneratorForest : IFieldMapEvents
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;

        public void OnGenerate(IMap map)
        {
            for (var i = 0; i < 20 + _rand.Next(20); i++)
            {
                var entity = _itemGen.GenerateItem(map, tags: new[] { Protos.Tag.ItemCatTree });
                if (entity != null && _entityMan.TryGetComponent<PickableComponent>(entity.Value, out var pickable))
                {
                    pickable.OwnState = OwnState.NPC;
                }
            }

            FieldMapGenerator.CreateJunkItems(map, _itemGen, _rand);
        }
    }

    public sealed class FieldMapGeneratorGrassland : IFieldMapEvents
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;

        public void OnGenerate(IMap map)
        {
            for (var i = 0; i < 10 + _rand.Next(10); i++)
            {
                var entity = _itemGen.GenerateItem(map, tags: new[] { Protos.Tag.ItemCatTree });
                if (entity != null && _entityMan.TryGetComponent<PickableComponent>(entity.Value, out var pickable))
                {
                    pickable.OwnState = OwnState.NPC;
                }
            }

            FieldMapGenerator.CreateJunkItems(map, _itemGen, _rand);
        }
    }

    public sealed class FieldMapGeneratorDesert : IFieldMapEvents
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;

        public void OnGenerate(IMap map)
        {
            for (var i = 0; i < 10 + _rand.Next(10); i++)
            {
                var entity = _itemGen.GenerateItem(map, id: Protos.Item.DeadTree);
                if (entity != null && _entityMan.TryGetComponent<PickableComponent>(entity.Value, out var pickable))
                {
                    pickable.OwnState = OwnState.NPC;
                }
            }

            FieldMapGenerator.CreateJunkItems(map, _itemGen, _rand);
        }
    }

    public sealed class FieldMapGeneratorSnowField : IFieldMapEvents
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;

        public void OnGenerate(IMap map)
        {
            for (var i = 0; i < 3 + _rand.Next(5); i++)
            {
                var entity = _itemGen.GenerateItem(map, tags: new[] { Protos.Tag.ItemCatTree }, fltselect: FltSelects.Snow);
                if (entity != null && _entityMan.TryGetComponent<PickableComponent>(entity.Value, out var pickable))
                {
                    pickable.OwnState = OwnState.NPC;
                }
            }

            FieldMapGenerator.CreateJunkItems(map, _itemGen, _rand);
        }
    }

    public sealed class FieldMapGeneratorPlains : IFieldMapEvents
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;

        public void OnGenerate(IMap map)
        {
            for (var i = 0; i < 5 + _rand.Next(5); i++)
            {
                var entity = _itemGen.GenerateItem(map, tags: new[] { Protos.Tag.ItemCatTree });
                if (entity != null && _entityMan.TryGetComponent<PickableComponent>(entity.Value, out var pickable))
                {
                    pickable.OwnState = OwnState.NPC;
                }
            }

            FieldMapGenerator.CreateJunkItems(map, _itemGen, _rand);
        }
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
        [Dependency] private readonly ICharaGen _charaGen = default!;

        /// <summary>
        /// The type of field map to generate.
        /// </summary>
        [DataField]
        public PrototypeId<FieldTypePrototype> FieldMap { get; set; } = Protos.FieldMap.Plains;

        public IMap Generate(MapGeneratorOptions opts)
        {
            var proto = _prototypeManager.Index(FieldMap);

            var map = _mapManager.CreateMap(opts.Width, opts.Height);
            map.Clear(proto.DefaultTile);

            foreach (var tile in proto.Tiles)
            {
                SprayTile(map, tile.ID, tile.Density, _random);
            }

            var metaData = _entMan.EnsureComponent<MetaDataComponent>(map.MapEntityUid);
            if (Loc.TryGetPrototypeString(FieldMap, "Name", out var name))
                metaData.DisplayName = name;

            if (proto.Generator != null)
            {
                EntitySystem.InjectDependencies(proto.Generator);
                proto.Generator.OnGenerate(map);
            }

            var charaGen = _entMan.EnsureComponent<MapCharaGenComponent>(map.MapEntityUid);
            charaGen.MaxCharaCount = 4;

            var turnOrder = _entMan.EnsureComponent<MapTurnOrderComponent>(map.MapEntityUid);
            turnOrder.TurnCost = 10000;

            var level = _entMan.EnsureComponent<LevelComponent>(map.MapEntityUid);
            level.Level = 1;

            var common = _entMan.GetComponent<MapCommonComponent>(map.MapEntityUid);
            common.IsIndoors = false;
            common.FogTile = proto.FogTile;
            common.IsTemporary = true;

            var mapAi = _entMan.EnsureComponent<MapVanillaAIComponent>(map.MapEntityUid);
            mapAi.DefaultCalmAction = VanillaAICalmAction.None;

            _entMan.EnsureComponent<MapTypeFieldComponent>(map.MapEntityUid);

            return map;
        }

        public IMap GenerateAndPopulate(MapGeneratorOptions opts)
        {
            var map = Generate(opts);

            for (var i = 0; i < _charaGen.GetMaxCrowdDensity(map); i++)
            {
                _charaGen.GenerateCharaFromMapFilter(map);
            }

            return map;
        }

        public static void CreateJunkItems(IMap map, IItemGen _itemGen, IRandom _random)
        {
            for (int i = 0; i < 4 + _random.Next(5); i++)
            {
                _itemGen.GenerateItem(map, tags: new[] { Protos.Tag.ItemCatJunkInField });
            }
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
