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
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic
{
    [TypeSerializer]
    public class EnumRangeSerializer<T> : ITypeSerializer<EnumRange<T>, ValueDataNode>
        where T : struct, Enum
    {
        private static readonly Regex EnumRangeRegex = new Regex(@"^([\p{L}\p{Nl}_][\p{Cf}\p{L}\p{Mc}\p{Mn}\p{Nd}\p{Nl}\p{Pc}]+)~([\p{L}\p{Nl}_][\p{Cf}\p{L}\p{Mc}\p{Mn}\p{Nd}\p{Nl}\p{Pc}]+)$");

        public EnumRange<T> Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null,
            EnumRange<T> value = default)
        {
            var matches = EnumRangeRegex.Match(node.Value);
            if (!matches.Success)
            {
                throw new InvalidMappingException($"Could not parse {nameof(EnumRange<T>)}: '{node.Value}'. Must be formatted like 'Min~Max'.");
            }

            var min = matches.Groups[1].Value;
            var max = matches.Groups[2].Value;

            if (!Enum.TryParse<T>(min, out var enumMin))
            {
                var valid = EnumHelpers.EnumerateValues<T>().Select(T => T.ToString()).ToList();
                var validStr = string.Join(", ", valid);
                throw new InvalidMappingException($"Could not parse enum range min {min} as a member of enum {typeof(T)}. Valid values: {validStr}");
            }
            if (!Enum.TryParse<T>(max, out var enumMax))
            {
                var valid = EnumHelpers.EnumerateValues<T>().Select(T => T.ToString()).ToList();
                var validStr = string.Join(", ", valid);
                throw new InvalidMappingException($"Could not parse enum range max {max} as a member of enum {typeof(T)}. Valid values: {validStr}");
            }


            return new EnumRange<T>(enumMin, enumMax);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var matches = EnumRangeRegex.Match(node.Value);
            if (!matches.Success)
            {
                return new ErrorNode(node, $"Could not parse {nameof(EnumRange<T>)}: '{node.Value}'. Must be formatted like 'Min~Max'.");
            }

            var min = matches.Groups[1].Value;
            var max = matches.Groups[2].Value;

            if (!Enum.TryParse<T>(min, out _))
            {
                var valid = EnumHelpers.EnumerateValues<T>().Select(T => T.ToString()).ToList();
                var validStr = string.Join(", ", valid);
                return new ErrorNode(node, $"Could not parse enum range min {min} as a member of enum {typeof(T)}. Valid values: {validStr}");
            }
            if (!Enum.TryParse<T>(max, out _))
            {
                var valid = EnumHelpers.EnumerateValues<T>().Select(T => T.ToString()).ToList();
                var validStr = string.Join(", ", valid);
                return new ErrorNode(node, $"Could not parse enum range max {max} as a member of enum {typeof(T)}. Valid values: {validStr}");
            }

            return new ValidatedValueNode(node);
        }

        public DataNode Write(ISerializationManager serializationManager, EnumRange<T> value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode($"{value.Min}~{value.Max}");
        }

        [MustUseReturnValue]
        public EnumRange<T> Copy(ISerializationManager serializationManager, EnumRange<T> source, EnumRange<T> target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.Min, source.Max);
        }

        public bool Compare(ISerializationManager serializationManager, EnumRange<T> objA, EnumRange<T> objB, bool skipHook,
            ISerializationContext? context = null)
        {
            return objA == objB;
        }
    }
}
