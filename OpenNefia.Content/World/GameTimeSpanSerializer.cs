using OpenNefia.Content.Skills;
using OpenNefia.Content.Spells;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using OpenNefia.Core.Serialization;

namespace OpenNefia.Content.World
{
    [TypeSerializer]
    public sealed class GameTimeSpanSerializer : ITypeSerializer<GameTimeSpan, ValueDataNode>
    {
        private static Regex GameTimeSpanRegex = new(
            @"(-)?(\d{2,}):(\d{2}):(\d{2})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public DeserializationResult Read(
            ISerializationManager serializationManager,
            ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook,
            ISerializationContext? context = null)
        {
            var match = GameTimeSpanRegex.Match(node.Value);
            if (!match.Success)
                throw new InvalidMappingException($"Could not parse {nameof(GameTimeSpan)}: '{node.Value}'");

            var isNeg = match.Groups[1].Value == "-";
            var result = new GameTimeSpan(
                (isNeg ? -1 : 1) * int.Parse(match.Groups[2].Value),
                (isNeg ? -1 : 1) * int.Parse(match.Groups[3].Value),
                (isNeg ? -1 : 1) * int.Parse(match.Groups[4].Value));

            return new DeserializedValue<GameTimeSpan>(result);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return GameTimeSpanRegex.IsMatch(node.Value)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Failed parsing {nameof(GameTimeSpan)}");
        }

        public DataNode Write(ISerializationManager serializationManager, GameTimeSpan value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var neg = value.TotalSeconds < 0 ? "-" : "";
            var hours = value.Hour
                + value.Day * GameTimeSpan.SecondsPerDay / GameTimeSpan.SecondsPerHour
                + value.Month * GameTimeSpan.SecondsPerMonth / GameTimeSpan.SecondsPerHour
                + value.Year * GameTimeSpan.SecondsPerYear / GameTimeSpan.SecondsPerHour;

            return new ValueDataNode($"{neg}{Math.Abs(hours).ToString("D2")}:{Math.Abs(value.Minute).ToString("D2")}:{Math.Abs(value.Second).ToString("D2")}");
        }

        public GameTimeSpan Copy(ISerializationManager serializationManager, GameTimeSpan source, GameTimeSpan target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, GameTimeSpan left, GameTimeSpan right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left.Equals(right);
        }
    }
}
