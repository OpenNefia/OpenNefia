using System.Globalization;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class UIBox2iFromDimensionsSerializer : ITypeSerializer<UIBox2i, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            var args = node.Value.Split(',');

            if (args.Length != 4)
            {
                throw new InvalidMappingException($"Could not parse {nameof(UIBox2i)}: '{node.Value}'");
            }

            var l = int.Parse(args[0], CultureInfo.InvariantCulture);
            var t = int.Parse(args[1], CultureInfo.InvariantCulture);
            var w = int.Parse(args[2], CultureInfo.InvariantCulture);
            var h = int.Parse(args[3], CultureInfo.InvariantCulture);

            return new DeserializedValue<UIBox2i>(UIBox2i.FromDimensions(l, t, w, h));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var args = node.Value.Split(',');

            if (args.Length != 4)
            {
                return new ErrorNode(node, $"Invalid amount of args for {nameof(UIBox2i)}.");
            }

            return int.TryParse(args[0], out _) &&
                   int.TryParse(args[1], out _) &&
                   int.TryParse(args[2], out _) &&
                   int.TryParse(args[3], out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Failed parsing values of {nameof(UIBox2i)}.");
        }

        public DataNode Write(ISerializationManager serializationManager, UIBox2i value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var nodeValue =
                $"{value.Left.ToString(CultureInfo.InvariantCulture)}," +
                $"{value.Top.ToString(CultureInfo.InvariantCulture)}," +
                $"{value.Width.ToString(CultureInfo.InvariantCulture)}," +
                $"{value.Height.ToString(CultureInfo.InvariantCulture)}";

            return new ValueDataNode(nodeValue);
        }

        [MustUseReturnValue]
        public UIBox2i Copy(ISerializationManager serializationManager, UIBox2i source, UIBox2i target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.Left, source.Bottom, source.Right, source.Top);
        }
    }
}
