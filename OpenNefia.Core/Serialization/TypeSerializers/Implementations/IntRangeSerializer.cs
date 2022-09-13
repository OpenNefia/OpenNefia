using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class IntRangeSerializer : ITypeSerializer<IntRange, ValueDataNode>
    {
        private static readonly Regex IntRangeRegex = new Regex(@"(\d+)~(\d+)");

        public IntRange Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null,
            IntRange value = default)
        {
            var matches = IntRangeRegex.Match(node.Value);
            if (!matches.Success)
                throw new InvalidMappingException($"Could not parse {nameof(IntRange)}: '{node.Value}'. Must be formatted like '(0, 100)'.");

            return new IntRange(int.Parse(matches.Groups[1].Value), int.Parse(matches.Groups[2].Value));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return IntRangeRegex.IsMatch(node.Value) 
                ? new ValidatedValueNode(node) :
                new ErrorNode(node, $"Could not parse {nameof(IntRange)}: '{node.Value}'. Must be formatted like '(0, 100)'.");
        }

        public DataNode Write(ISerializationManager serializationManager, IntRange value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode($"{value.Min}~{value.Max}");
        }

        [MustUseReturnValue]
        public IntRange Copy(ISerializationManager serializationManager, IntRange source, IntRange target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.Min, source.Max);
        }

        public bool Compare(ISerializationManager serializationManager, IntRange objA, IntRange objB, bool skipHook,
            ISerializationContext? context = null)
        {
            return objA == objB;
        }
    }
}
