using System.Globalization;
using OpenNefia.Core.IoC;
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
    public class Vector4Serializer : ITypeSerializer<Vector4, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (!VectorSerializerUtility.TryParseArgs(node.Value, 4, out var args))
            {
                throw new InvalidMappingException($"Could not parse {nameof(Vector4)}: '{node.Value}'");
            }

            var x = float.Parse(args[0], CultureInfo.InvariantCulture);
            var y = float.Parse(args[1], CultureInfo.InvariantCulture);
            var z = float.Parse(args[2], CultureInfo.InvariantCulture);
            var w = float.Parse(args[3], CultureInfo.InvariantCulture);
            var vector = new Vector4(x, y, z, w);

            return new DeserializedValue<Vector4>(vector);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            if (!VectorSerializerUtility.TryParseArgs(node.Value, 4, out var args))
            {
                throw new InvalidMappingException($"Could not parse {nameof(Vector4)}: '{node.Value}'");
            }

            return float.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                   float.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                   float.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                   float.TryParse(args[3], NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, "Failed parsing values for Vector4.");
        }

        public DataNode Write(ISerializationManager serializationManager, Vector4 value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode($"{value.X.ToString(CultureInfo.InvariantCulture)}," +
                                     $"{value.Y.ToString(CultureInfo.InvariantCulture)}," +
                                     $"{value.Z.ToString(CultureInfo.InvariantCulture)}," +
                                     $"{value.W.ToString(CultureInfo.InvariantCulture)}");
        }

        public Vector4 Copy(ISerializationManager serializationManager, Vector4 source, Vector4 target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.X, source.Y, source.Z, source.W);
        }
    }
}
