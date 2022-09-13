using OpenNefia.Core.Containers;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using System.Text.RegularExpressions;

namespace OpenNefia.Core.Areas
{
    [TypeSerializer]
    public class AreaFloorIdSerializer : ITypeSerializer<AreaFloorId, ValueDataNode>
    {
        private static readonly Regex AreaFloorIdRegex = new(@"^(.*):([0-9]+)$");

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            return AreaFloorIdRegex.IsMatch(node.Value)
                ? new ValidatedValueNode(node) :
                new ErrorNode(node, $"Could not parse {nameof(AreaFloorId)}: '{node.Value}'. Must be formatted like 'SomeId:123'.");
        }

        public AreaFloorId Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null,
            AreaFloorId rawValue = default)
        {
            var matches = AreaFloorIdRegex.Match(node.Value);
            if (!matches.Success)
                throw new InvalidMappingException($"Could not parse {nameof(AreaFloorId)}: '{node.Value}'. Must be formatted like 'SomeId:123'.");

            return new AreaFloorId(matches.Groups[1].Value, int.Parse(matches.Groups[2].Value));
        }

        public DataNode Write(ISerializationManager serializationManager, AreaFloorId value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode($"{value.ID}:{value.FloorNumber}");
        }

        public AreaFloorId Copy(ISerializationManager serializationManager, AreaFloorId source, AreaFloorId target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, AreaFloorId left, AreaFloorId right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
