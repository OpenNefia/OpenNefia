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
    public class AngleSerializer : ITypeSerializer<Angle, ValueDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            var nodeContents = node.Value;

            var angle = nodeContents.EndsWith("rad")
                ? new Angle(double.Parse(nodeContents.Substring(0, nodeContents.Length - 3),
                    CultureInfo.InvariantCulture))
                : Angle.FromDegrees(double.Parse(nodeContents, CultureInfo.InvariantCulture));

            return new DeserializedValue<Angle>(angle);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var nodeValue = node.Value;
            var value = nodeValue.EndsWith("rad") ? nodeValue.Substring(0, nodeValue.Length - 3) : nodeValue;

            return double.TryParse(value, out _) ? new ValidatedValueNode(node) : new ErrorNode(node, "Failed parsing angle.");
        }

        public DataNode Write(ISerializationManager serializationManager, Angle value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode($"{value.Theta.ToString(CultureInfo.InvariantCulture)} rad");
        }

        [MustUseReturnValue]
        public Angle Copy(ISerializationManager serializationManager, Angle source, Angle target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source);
        }
    }
}
