using CommandLine;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.CommandLine;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Content.CommandLine
{
    [Verb(name: "convertHspMap", HelpText = "Converts a .map/.idx/.obj trio into a map blueprint.")]
    public class ConvertHspMapCommand : ICommand
    {
        [Value(0, MetaName = "mapFilePath", Required = true, HelpText = "Path to .map file.")]
        public string MapFilePath { get; set; } = default!;

        [Value(1, MetaName = "outputDirectory", Required = true, HelpText = "Directory to hold the converted map blueprint.")]
        public string OutputDirectory { get; set; } = default!;

        [Option(longName: "mapName", HelpText = "Name of this map.")]
        public string? MapName { get; set; }

        [Option(longName: "mapAuthor", HelpText = "Author of this map.")]
        public string MapAuthor { get; set; } = "(unknown)";

        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;

        private readonly EntityUid MapEntityUid = new EntityUid(0);
        private EntityUid _maxEntityUid;

        // { EntityProtoId -> { CompTypeName -> CompYAML } }
        private Dictionary<PrototypeId<EntityPrototype>, Dictionary<string, MappingDataNode>> _prototypeCompCache = new();

        /// <summary>
        /// Maps from (hspType, hspIndex) tuples to <see cref="PrototypeId{T}"/>,
        /// and from cellObjIds to <see cref="PrototypeId{T}"/>.
        /// </summary>
        private class HspEntityPrototypeIndex
        {
            private readonly IPrototypeManager _prototypeManager;
            private readonly IReflectionManager _reflectionManager;

            private readonly Dictionary<string, Dictionary<int, PrototypeId<EntityPrototype>>> _idToProtoId = new();
            private readonly Dictionary<int, PrototypeId<EntityPrototype>> _cellObjIndexToProtoId = new();

            public HspEntityPrototypeIndex(IPrototypeManager prototypeManager, IReflectionManager reflectionManager)
            {
                _prototypeManager = prototypeManager;
                _reflectionManager = reflectionManager;

                Build();
            }

            private void Build()
            {
                var groups = _prototypeManager.EnumeratePrototypes<EntityPrototype>()
                    .Where(proto => proto.HspEntityType != null && proto.HspIds != null)
                    .GroupBy(proto => proto.HspEntityType!);

                foreach (var group in groups)
                {
                    var hspEntityType = group.Key;
                    var ids = new Dictionary<int, PrototypeId<EntityPrototype>>();

                    foreach (var proto in group)
                    {
                        var hspId = proto.HspIds!.GetCanonical();
                        if (!ids.ContainsKey(hspId))
                            ids.Add(hspId, proto.GetStrongID());

                        if (proto.HspCellObjIds.TryGetValue(proto.HspIds!.HspOrigin, out var cellObjIds))
                        {
                            foreach (var id in cellObjIds)
                                _cellObjIndexToProtoId.Add(id, proto.GetStrongID());
                        }
                    }

                    _idToProtoId.Add(hspEntityType, ids);
                }
            }

            public bool TryGetValue(string type, int index, [NotNullWhen(true)] out PrototypeId<EntityPrototype> id)
            {
                id = PrototypeId<EntityPrototype>.Invalid;

                if (!_idToProtoId.TryGetValue(type, out var ids))
                    return false;

                if (!ids.TryGetValue(index, out id))
                    return false;

                return true;
            }

            public PrototypeId<EntityPrototype> GetValueOrThrow(string type, int index)
            {
                if (!TryGetValue(type, index, out var id))
                    throw new KeyNotFoundException($"Entity prototype with type '{type}' and index {index} not found.");

                return id;
            }

            public PrototypeId<EntityPrototype> GetValueForCellObjOrThrow(int cellObjIndex)
            {
                if (!_cellObjIndexToProtoId.TryGetValue(cellObjIndex, out var id))
                    throw new KeyNotFoundException($"Entity prototype with cell object index {cellObjIndex} not found.");

                return id;
            }
        }

        private class HspMapIdx
        {
            public readonly int Width;
            public readonly int Height;
            public readonly int AtlasIndex;
            public readonly int Regen;
            public readonly int StairUpPos;

            public HspMapIdx(int width, int height, int atlasIndex, int regen, int stairsUp)
            {
                Width = width;
                Height = height;
                AtlasIndex = atlasIndex;
                Regen = regen;
                StairUpPos = stairsUp;
            }
        }

        private Dictionary<int, PrototypeId<TilePrototype>> BuildTileMap(int atlasNum)
        {
            var tileMap = new Dictionary<int, PrototypeId<TilePrototype>>();

            foreach (var tileProto in _tileDefinitionManager.Where(t => t.HspIds != null && t.HspIds.GetCanonical()!.X == 0))
            {
                var (_, hspIndex) = tileProto.HspIds!.GetCanonical();
                tileMap[hspIndex] = tileProto.GetStrongID();
            }

            if (atlasNum > 0)
            {
                foreach (var tileProto in _tileDefinitionManager.Where(t => t.HspIds != null && t.HspIds.GetCanonical()!.X == atlasNum))
                {
                    var (_, hspIndex) = tileProto.HspIds!.GetCanonical();
                    tileMap[hspIndex] = tileProto.GetStrongID();
                }
            }

            return tileMap;
        }

        private HspMapIdx ReadHspMapIdx(string idxFilePath)
        {
            int width, height, atlasNum, regen, stairUpPos;

            using (var reader = OpenCompressed(idxFilePath))
            {
                width = reader.ReadInt32();
                height = reader.ReadInt32();
                atlasNum = reader.ReadInt32();
                regen = reader.ReadInt32();
                stairUpPos = reader.ReadInt32();
            }

            return new HspMapIdx(width, height, atlasNum, regen, stairUpPos);
        }

        private (string, YamlMappingNode) ReadHspMapMap(string mapFilePath, HspMapIdx idx)
        {
            var map = new Map(idx.Width, idx.Height);
            var tileMap = BuildTileMap(idx.AtlasIndex);

            using (var reader = OpenCompressed(mapFilePath))
            {
                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        var elonaTileId = reader.ReadInt32();
                        var tileDef = tileMap[elonaTileId];
                        map.SetTile(new Vector2i(x, y), tileDef);
                    }
                }
            }

            var protoToRune = YamlGridSerializer.BuildProtoToRuneTileMap(map);

            var grid = YamlGridSerializer.SerializeGrid(map, protoToRune, _tileDefinitionManager);
            var runeToProtoTileMap = new YamlMappingNode();

            foreach (var (rune, tileId) in protoToRune.Invert())
            {
                runeToProtoTileMap.Add(rune, tileId.ToString());
            }

            return (grid, runeToProtoTileMap);
        }

        private ComponentRegistry MakeEntityComponentsBase(PrototypeId<EntityPrototype> protoId)
        {
            var proto = _prototypeManager.Index(protoId);
            var comps = new ComponentRegistry();

            return _serializationManager.Copy(proto.Components, comps)!;
        }

        private YamlMappingNode SerializeEntityNode(PrototypeId<EntityPrototype> protoId, ComponentRegistry comps, int x, int y)
        {
            var prototype = _prototypeManager.Index(protoId);

            var entityNode = new YamlMappingNode();
            entityNode.Add(MapBlueprintLoader.Keys.Entities_Uid, _maxEntityUid.ToString());
            entityNode.Add(MapBlueprintLoader.Keys.Entities_ProtoId, protoId.ToString());

            var entityComps = new YamlSequenceNode();
            entityNode.Add(MapBlueprintLoader.Keys.Entities_Components, entityComps);

            // Precompute YAML to diff against each entity instance.
            // The fields that are the same as those in the prototype are omitted.
            if (!_prototypeCompCache.ContainsKey(protoId))
            {
                _prototypeCompCache[protoId] = new Dictionary<string, MappingDataNode>();
                foreach (var (compType, comp) in prototype.Components)
                {
                    var protoMapping = _serializationManager.WriteValueAs<MappingDataNode>(comp.GetType(), comp);
                    _prototypeCompCache[protoId].Add(compType, protoMapping);
                }
            }

            foreach (var (compType, comp) in comps)
            {

                var compMapping = _serializationManager.WriteValueAs<MappingDataNode>(comp.GetType(), comp);

                // This is necessary since no entities/maps are tracked by the EntityManager during
                // this process, so it's not possible to set the local position by hand (it tries to
                // look up the parent in the EntityManager).
                if (comp is SpatialComponent)
                {
                    compMapping["pos"] = new ValueDataNode($"{x},{y}");
                    compMapping["parent"] = new ValueDataNode(MapEntityUid.ToString());
                }

                if (_prototypeCompCache[protoId].TryGetValue(compType, out var protoMapping))
                {
                    // Only serialize component fields that differ from the prototype.
                    compMapping = compMapping.Except(protoMapping);
                    if (compMapping == null) continue;
                }

                compMapping.Insert(0, MapBlueprintLoader.Keys.Entities_Components_Type, new ValueDataNode(comp.Name));
                entityComps.Add(compMapping.ToYamlNode());
            }

            _maxEntityUid = new EntityUid((int)_maxEntityUid + 1);

            return entityNode;
        }

        private YamlMappingNode ConvertItemObj(Obj obj, HspEntityPrototypeIndex index)
        {
            var ownState = obj.Param;

            var protoId = index.GetValueOrThrow(HspEntityTypes.Item, obj.Id);
            var comps = MakeEntityComponentsBase(protoId);

            foreach (var comp in comps.Values.WhereAssignable<IComponent, IFromHspItem>())
            {
                comp.FromHspItem(ownState);
            }

            return SerializeEntityNode(protoId, comps, obj.X, obj.Y);
        }

        private YamlMappingNode ConvertCharaObj(Obj obj, HspEntityPrototypeIndex index)
        {
            var protoId = index.GetValueOrThrow(HspEntityTypes.Chara, obj.Id);
            var comps = MakeEntityComponentsBase(protoId);

            return SerializeEntityNode(protoId, comps, obj.X, obj.Y);
        }

        private YamlMappingNode ConvertFeatObj(Obj obj, HspEntityPrototypeIndex index)
        {
            var cellObjId = obj.Id;
            var protoId = index.GetValueForCellObjOrThrow(cellObjId);

            var param1 = obj.Param % 1000;
            var param2 = obj.Param / 1000;

            var comps = MakeEntityComponentsBase(protoId);

            foreach (var comp in comps.Values.WhereAssignable<IComponent, IFromHspFeat>())
            {
                comp.FromHspFeat(cellObjId, param1, param2);
            }

            return SerializeEntityNode(protoId, comps, obj.X, obj.Y);
        }

        private YamlMappingNode MakeMapEntity()
        {
            var mapEntity = new YamlMappingNode();
            mapEntity.Add(MapBlueprintLoader.Keys.Entities_Uid, MapEntityUid.ToString());

            var mapEntityComps = new YamlSequenceNode();
            mapEntity.Add(MapBlueprintLoader.Keys.Entities_Components, mapEntityComps);

            var mapEntityMapComp = new YamlMappingNode();
            mapEntityComps.Add(mapEntityMapComp);
            mapEntityMapComp.Add(MapBlueprintLoader.Keys.Entities_Components_Type, "Map");

            return mapEntity;
        }

        private enum ObjType : int
        {
            Item = 0,
            Chara = 1,
            Feat = 2,
        }

        private class Obj
        {
            public readonly int Id;
            public readonly int X;
            public readonly int Y;
            public readonly int Param;
            public readonly ObjType Type;

            public Obj(int id, int x, int y, int param, ObjType type)
            {
                Id = id;
                X = x;
                Y = y;
                Param = param;
                Type = type;
            }
        }

        private Obj ReadObj(BinaryReader reader)
        {
            var id = reader.ReadInt32();
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            var param = reader.ReadInt32();
            var type = (ObjType)reader.ReadInt32();

            return new Obj(id, x, y, param, type);
        }

        private YamlSequenceNode ReadHspMapObj(string objFilePath, HspMapIdx idx)
        {
            var index = new HspEntityPrototypeIndex(_prototypeManager, _reflectionManager);

            var entities = new YamlSequenceNode();

            var mapEntity = MakeMapEntity();
            entities.Add(mapEntity);

            _maxEntityUid = new EntityUid(1);

            using (var reader = OpenCompressed(objFilePath))
            {
                for (var i = 0; i < 400; i++)
                {
                    var obj = ReadObj(reader);
                    
                    if (obj.Id == 0)
                        break;

                    switch (obj.Type)
                    {
                        case ObjType.Item:
                            entities.Add(ConvertItemObj(obj, index));
                            break;

                        case ObjType.Chara:
                            entities.Add(ConvertCharaObj(obj, index));
                            break;

                        case ObjType.Feat:
                            entities.Add(ConvertFeatObj(obj, index));
                            break;

                        default:
                            throw new IndexOutOfRangeException($"Cannot read HSP map object of type {obj.Type}");
                    }
                }
            }

            return entities;
        }

        private BinaryReader OpenCompressed(string objFilePath)
        {
            var fileStream = File.Open(objFilePath, FileMode.Open);
            var zlibStream = new GZipStream(fileStream, CompressionMode.Decompress);
            return new BinaryReader(zlibStream);
        }

        private YamlMappingNode BuildMapMetadata()
        {
            if (MapName == null)
            {
                MapName = Path.GetFileNameWithoutExtension(MapFilePath);
            }

            var meta = new YamlMappingNode();
            meta.Add(MapBlueprintLoader.Keys.Meta_Format, "1");
            meta.Add(MapBlueprintLoader.Keys.Meta_Name, MapName);
            meta.Add(MapBlueprintLoader.Keys.Meta_Author, MapAuthor);

            return meta;
        }

        public void Execute()
        {
            var dir = Path.GetDirectoryName(MapFilePath);
            var fileBaseName = Path.GetFileNameWithoutExtension(MapFilePath);
            var idxFilePath = Path.Join(dir, $"{fileBaseName}.idx");
            var objFilePath = Path.Join(dir, $"{fileBaseName}.obj");

            if (!File.Exists(idxFilePath))
            {
                throw new InvalidDataException($"1.22 .idx file {idxFilePath} does not exist.");
            }
            if (!File.Exists(MapFilePath))
            {
                throw new InvalidDataException($"1.22 .map file {MapFilePath} does not exist.");
            }
            if (!File.Exists(objFilePath))
            {
                throw new InvalidDataException($"1.22 .obj file {objFilePath} does not exist.");
            }

            var idx = ReadHspMapIdx(idxFilePath);
            var (grid, tileMap) = ReadHspMapMap(MapFilePath, idx);
            var entities = ReadHspMapObj(objFilePath, idx);
            var meta = BuildMapMetadata();

            var root = new YamlMappingNode();
            root.Add(MapBlueprintLoader.Keys.Meta, meta);
            root.Add(MapBlueprintLoader.Keys.Grid, new YamlScalarNode(grid) { Style = ScalarStyle.Literal });
            root.Add(MapBlueprintLoader.Keys.Tilemap, tileMap);
            root.Add(MapBlueprintLoader.Keys.Entities, entities);

            var document = new YamlDocument(root);
            var outputPath = Path.Join(OutputDirectory, $"{fileBaseName}.yml");
            
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);

            using (var fileStream = File.Open(outputPath, FileMode.Create, FileAccess.Write))
            {
                using (var writer = new StreamWriter(fileStream))
                {
                    var stream = new YamlStream();
                    stream.Add(document);
                    stream.Save(new YamlMappingFix(new Emitter(writer)), false);
                }
            }

            Console.WriteLine($"Wrote to {outputPath}.");
        }
    }
}
