using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic
{
    /// <summary>
    /// Allows you to grab a <see cref="CVarDef{T}"/> from the <see cref="IConfigurationManager"/>
    /// in serialized YAML by giving the CVar's name as a string.
    /// </summary>
    /// <typeparam name="T">Type of the CVar.</typeparam>
    [TypeSerializer]
    public sealed class CVarDefSerializer<T> :
        ITypeSerializer<CVarDef<T>, ValueDataNode>
        where T : notnull
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            var configManager = dependencies.Resolve<IConfigurationManager>();
            return configManager.TryGetCVarDef<T>(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Could not find {nameof(CVarDef<T>)} registered with name: {node.Value}");
        }

        public CVarDef<T> Read(ISerializationManager serializationManager, ValueDataNode node, 
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null,
            CVarDef<T>? value = null)
        {
            var configManager = dependencies.Resolve<IConfigurationManager>();

            configManager.TryGetCVarDef<T>(node.Value, out var cVarDef);
            return cVarDef!;
        }

        public DataNode Write(ISerializationManager serializationManager, CVarDef<T> value,
            bool alwaysWrite = false, ISerializationContext? context = null)
        {
            return new ValueDataNode(value.Name);
        }

        public CVarDef<T> Copy(ISerializationManager serializationManager, CVarDef<T> source, CVarDef<T> target, 
            bool skipHook, ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, CVarDef<T> left, CVarDef<T> right,
            bool skipHook, ISerializationContext? context = null)
        {
            return left.Name == right.Name;
        }

    }
    /// <summary>
    /// Allows you to grab a <see cref="CVarDef"/> from the <see cref="IConfigurationManager"/>
    /// in serialized YAML by giving the CVar's name as a string.
    /// </summary>
    [TypeSerializer]
    public sealed class CVarDefSerializer : ITypeSerializer<CVarDef, ValueDataNode>
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            var configManager = dependencies.Resolve<IConfigurationManager>();
            return configManager.TryGetCVarDef(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Could not find {nameof(CVarDef)} registered with name: {node.Value}");
        }

        public CVarDef Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null,
            CVarDef? value = null)
        {
            var configManager = dependencies.Resolve<IConfigurationManager>();

            configManager.TryGetCVarDef(node.Value, out var cVarDef);
            return cVarDef!;
        }

        public DataNode Write(ISerializationManager serializationManager, CVarDef value,
            bool alwaysWrite = false, ISerializationContext? context = null)
        {
            return new ValueDataNode(value.Name);
        }

        public CVarDef Copy(ISerializationManager serializationManager, CVarDef source, CVarDef target,
            bool skipHook, ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, CVarDef left, CVarDef right,
            bool skipHook, ISerializationContext? context = null)
        {
            return left.Name == right.Name;
        }
    }
}
