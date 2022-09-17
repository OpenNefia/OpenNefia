using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Content.Skills
{
    [TypeSerializer]
    public class LevelAndPotentialSerializer : ITypeSerializer<LevelAndPotential, ValueDataNode>
    {
        public LevelAndPotential Read(
            ISerializationManager serializationManager,
            ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook,
            ISerializationContext? context = null,
            LevelAndPotential? rawValue = null)
        {
            return new LevelAndPotential()
            {
                Level = new(int.Parse(node.Value))
            };
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return int.TryParse(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Failed parsing {nameof(LevelAndPotential)}");
        }

        public DataNode Write(ISerializationManager serializationManager, LevelAndPotential value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return serializationManager.GetDefinition<LevelAndPotential>()!.Serialize(value, serializationManager, alwaysWrite, context);
        }

        public LevelAndPotential Copy(ISerializationManager serializationManager, LevelAndPotential source, LevelAndPotential target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            target.Level = source.Level;
            target.Potential = source.Potential;
            target.Experience = source.Experience;
            return target;
        }

        public bool Compare(ISerializationManager serializationManager, LevelAndPotential left, LevelAndPotential right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left.Equals(right);
        }
    }
}
