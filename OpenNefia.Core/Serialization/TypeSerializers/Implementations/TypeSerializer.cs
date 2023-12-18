using System;
using System.Collections.Generic;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    // NOTE: This doesn't work fully due to this behavior:
    // https://stackoverflow.com/questions/47861723/comparing-typeoftype-to-system-runtimetype
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

        public Type Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null, Type? value = default)
        {
            if (Shortcuts.TryGetValue(node.Value, out var shortcutType))
                return shortcutType;

            var type = dependencies.Resolve<IReflectionManager>().GetType(node.Value);

            return type == null
                ? throw new InvalidMappingException($"Type '{node.Value}' not found.")
                : type;
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

        public bool Compare(ISerializationManager serializationManager, Type left, Type right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
