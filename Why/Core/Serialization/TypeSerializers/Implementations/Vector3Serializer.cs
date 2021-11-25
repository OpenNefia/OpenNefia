using Love;
using System.Globalization;
using Why.Core.IoC;
using Why.Core.Maths;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;
using Why.Core.Utility;

namespace Why.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class Vector3Serializer : ITypeSerializer<Vector3, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (!VectorSerializerUtility.TryParseArgs(node.Value, 3, out var args))
            {
                throw new InvalidMappingException($"Could not parse {nameof(Vector3)}: '{node.Value}'");
            }

            var x = float.Parse(args[0], CultureInfo.InvariantCulture);
            var y = float.Parse(args[1], CultureInfo.InvariantCulture);
            var z = float.Parse(args[2], CultureInfo.InvariantCulture);
            var vector = new Vector3(x, y, z);

            return new DeserializedValue<Vector3>(vector);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            if (!VectorSerializerUtility.TryParseArgs(node.Value, 3, out var args))
            {
                throw new InvalidMappingException($"Could not parse {nameof(Vector3)}: '{node.Value}'");
            }

            return float.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                   float.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                   float.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, "Failed parsing values for Vector3.");
        }

        public DataNode Write(ISerializationManager serializationManager, Vector3 value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode($"{value.X.ToString(CultureInfo.InvariantCulture)}," +
                                     $"{value.Y.ToString(CultureInfo.InvariantCulture)}," +
                                     $"{value.Z.ToString(CultureInfo.InvariantCulture)}");
        }

        public Vector3 Copy(ISerializationManager serializationManager, Vector3 source, Vector3 target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.X, source.Y, source.Z);
        }
    }
}
