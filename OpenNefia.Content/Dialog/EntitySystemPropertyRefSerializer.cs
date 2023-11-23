using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Serialization;
using System.Text.RegularExpressions;

namespace OpenNefia.Content.Dialog
{
    [TypeSerializer]
    public class EntitySystemPropertyRefSerializer : ITypeSerializer<EntitySystemPropertyRef, ValueDataNode>
    {
        private static readonly Regex QualifiedPropertyNameRegex = new(@"^EntitySystem@(.*):(.+)$");

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            var matches = QualifiedPropertyNameRegex.Match(node.Value); 
            if (!matches.Success)
                return new ErrorNode(node, $"Could not parse {nameof(EntitySystemPropertyRef)}: '{node.Value}'. Must be formatted like 'EntitySystem@Namespace.Of.Type:PropertyName'.", true);

            var typeName = matches.Groups[1].Value;
            var reflection = dependencies.Resolve<IReflectionManager>();

            if (!reflection.TryLooseGetType(typeName, out var type))
                return new ErrorNode(node, $"Type '{typeName}' not found.");
            if (!type.IsAssignableTo(typeof(IEntitySystem)))
                return new ErrorNode(node, $"Type '{typeName}' does not implement {nameof(IEntitySystem)}");

            return new ValidatedValueNode(node);
        }

        public EntitySystemPropertyRef Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null,
            EntitySystemPropertyRef rawValue = default)
        {
            var matches = QualifiedPropertyNameRegex.Match(node.Value);
            if (!matches.Success)
                throw new InvalidMappingException($"Could not parse {nameof(EntitySystemPropertyRef)}: '{node.Value}'. Must be formatted like 'EntiNamespace.Of.Type:PropertyName'.");

            var typeName = matches.Groups[1].Value;
            var reflection = dependencies.Resolve<IReflectionManager>();

            if (!reflection.TryLooseGetType(typeName, out var type))
                throw new InvalidMappingException($"Type '{typeName}' not found.");
            if (!type.IsAssignableTo(typeof(IEntitySystem)))
                throw new InvalidMappingException($"Type '{typeName}' does not implement {nameof(IEntitySystem)}");

            return new EntitySystemPropertyRef(type, matches.Groups[2].Value);
        }

        public DataNode Write(ISerializationManager serializationManager, EntitySystemPropertyRef value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToString());
        }

        public EntitySystemPropertyRef Copy(ISerializationManager serializationManager, EntitySystemPropertyRef source, EntitySystemPropertyRef target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, EntitySystemPropertyRef left, EntitySystemPropertyRef right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
