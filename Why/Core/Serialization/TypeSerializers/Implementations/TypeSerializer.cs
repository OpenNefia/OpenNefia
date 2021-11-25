using System;
using System.Collections.Generic;
using Why.Core.IoC;
using Why.Core.Reflection;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class TypeSerializer : ITypeSerializer<Type, ValueDataNode>
    {
        private static readonly Dictionary<string, Type> Shortcuts = new ()
        {
            {"bool", typeof(bool)}
        };

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            if (Shortcuts.ContainsKey(node.Value))
                return new ValidatedValueNode(node);

            return dependencies.Resolve<IReflectionManager>().GetType(node.Value) == null
                ? new ErrorNode(node, $"Type '{node.Value}' not found.")
                : new ValidatedValueNode(node);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            if (Shortcuts.TryGetValue(node.Value, out var shortcutType))
                return new DeserializedValue<Type>(shortcutType);

            var type = dependencies.Resolve<IReflectionManager>().GetType(node.Value);

            return type == null
                ? throw new InvalidMappingException($"Type '{node.Value}' not found.")
                : new DeserializedValue<Type>(type);
        }

        public DataNode Write(ISerializationManager serializationManager, Type value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.FullName ?? value.Name);
        }

        public Type Copy(ISerializationManager serializationManager, Type source, Type target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }
    }
}
