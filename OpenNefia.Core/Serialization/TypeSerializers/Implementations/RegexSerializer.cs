using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class RegexSerializer : ITypeSerializer<Regex, ValueDataNode>
    {
        public Regex Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null, Regex? value = default)
        {
            return new Regex(node.Value, RegexOptions.Compiled);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            try
            {
                _ = new Regex(node.Value);
            }
            catch (Exception)
            {
                return new ErrorNode(node, "Failed compiling regex.");
            }

            return new ValidatedValueNode(node);
        }

        public DataNode Write(ISerializationManager serializationManager, Regex value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.ToString());
        }

        [MustUseReturnValue]
        public Regex Copy(ISerializationManager serializationManager, Regex source, Regex target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.ToString(), source.Options, source.MatchTimeout);
        }

        public bool Compare(ISerializationManager serializationManager, Regex left, Regex right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
