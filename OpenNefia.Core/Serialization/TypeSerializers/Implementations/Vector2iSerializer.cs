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
    public class Vector2iSerializer : ITypeSerializer<Vector2i, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            if (!VectorSerializerUtility.TryParseArgs(node.Value, 2, out var args))
            {
                throw new InvalidMappingException($"Could not parse {nameof(Vector2i)}: '{node.Value}'");
            }

            var x = int.Parse(args[0], CultureInfo.InvariantCulture);
            var y = int.Parse(args[1], CultureInfo.InvariantCulture);
            var vector = new Vector2i(x, y);

            return new DeserializedValue<Vector2i>(vector);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            if (!VectorSerializerUtility.TryParseArgs(node.Value, 2, out var args))
            {
                throw new InvalidMappingException($"Could not parse {nameof(Vector2i)}: '{node.Value}'");
            }

            return int.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                   int.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, "Failed parsing values for Vector2i.");
        }

        public DataNode Write(ISerializationManager serializationManager, Vector2i value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode($"{value.X.ToString(CultureInfo.InvariantCulture)}," +
                                     $"{value.Y.ToString(CultureInfo.InvariantCulture)}");
        }

        public Vector2i Copy(ISerializationManager serializationManager, Vector2i source, Vector2i target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.X, source.Y);
        }

        public bool Compare(ISerializationManager serializationManager, Vector2i left, Vector2i right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
