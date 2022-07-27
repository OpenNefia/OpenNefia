using System.Globalization;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class MapCoordinatesSerializer : ITypeSerializer<MapCoordinates, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (!VectorSerializerUtility.TryParseArgs(node.Value, 3, out var args))
            {
                throw new InvalidMappingException($"Could not parse {nameof(MapCoordinates)}: '{node.Value}'");
            }

            var mapId = int.Parse(args[0], CultureInfo.InvariantCulture);
            var x = int.Parse(args[1], CultureInfo.InvariantCulture);
            var y = int.Parse(args[2], CultureInfo.InvariantCulture);
            var coords = new MapCoordinates(new MapId(mapId), x, y);

            return new DeserializedValue<MapCoordinates>(coords);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            if (!VectorSerializerUtility.TryParseArgs(node.Value, 3, out var args))
            {
                return new ErrorNode(node, $"Could not parse {nameof(MapCoordinates)}: '{node.Value}'");
            }

            return int.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                   int.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                   int.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, "Failed parsing values for MapCoordinates.");
        }

        public DataNode Write(ISerializationManager serializationManager, MapCoordinates value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode($"{value.MapId.Value.ToString(CultureInfo.InvariantCulture)}," +
                                     $"{value.X.ToString(CultureInfo.InvariantCulture)}," +
                                     $"{value.Y.ToString(CultureInfo.InvariantCulture)}");
        }

        public MapCoordinates Copy(ISerializationManager serializationManager, MapCoordinates source, MapCoordinates target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.MapId, source.X, source.Y);
        }

        public bool Compare(ISerializationManager serializationManager, MapCoordinates left, MapCoordinates right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
