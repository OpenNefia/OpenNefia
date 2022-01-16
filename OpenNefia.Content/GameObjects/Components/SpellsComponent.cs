using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Stats;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Holds spell level/stock data.
    /// </summary>
    [RegisterComponent]
    public class SpellsComponent : Component
    {
        public override string Name => "Spells";

        /// <summary>
        /// Level, potential, experience and spell stock for spells.
        /// </summary>
        [DataField]
        public Dictionary<PrototypeId<SkillPrototype>, LevelPotentialAndStock> Spells { get; } = new();

        public bool TryGetKnown(PrototypeId<SkillPrototype> protoId, [NotNullWhen(true)] out LevelPotentialAndStock? level)
        {
            if (!Spells.TryGetValue(protoId, out level))
            {
                return false;
            }

            if (level.Stats.Level.Base <= 0)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// The data associated with a spell.
    /// </summary>
    [DataDefinition]
    public class LevelPotentialAndStock : IEquatable<LevelPotentialAndStock>
    {
        /// <summary>
        /// The level, potential and experience associated with this spell.
        /// </summary>
        [DataField]
        public LevelAndPotential Stats { get; } = new();

        /// <summary>
        /// Current spell stock.
        /// </summary>
        [DataField]
        public int SpellStock { get; set; } = 0;

        public bool Equals(LevelPotentialAndStock? other)
        {
            if (other == null)
                return false;

            return Stats.Equals(other.Stats) && SpellStock == other.SpellStock;
        }

        public override string ToString()
        {
            return $"(Level={Stats.Level} Potential={Stats.Potential}% EXP={Stats.Experience} Stock={SpellStock})";
        }
    }

    [TypeSerializer]
    public class LevelPotentialAndStockSerializer : ITypeSerializer<LevelPotentialAndStock, ValueDataNode>
    {
        public DeserializationResult Read(
            ISerializationManager serializationManager,
            ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook,
            ISerializationContext? context = null)
        {
            var result = new LevelPotentialAndStock();
            result.Stats.Level = new(int.Parse(node.Value));

            return new DeserializedValue<LevelPotentialAndStock>(result);
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            return int.TryParse(node.Value, out _)
                ? new ValidatedValueNode(node)
                : new ErrorNode(node, $"Failed parsing {nameof(LevelAndPotential)}");
        }

        public DataNode Write(ISerializationManager serializationManager, LevelPotentialAndStock value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return serializationManager.GetDefinition<LevelPotentialAndStock>()!
                .Serialize(value, serializationManager, context, alwaysWrite);
        }

        public LevelPotentialAndStock Copy(ISerializationManager serializationManager, LevelPotentialAndStock source, LevelPotentialAndStock target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            serializationManager.Copy(source.Stats, target.Stats, context, skipHook);
            target.SpellStock = source.SpellStock;
            return target;
        }

        public bool Compare(ISerializationManager serializationManager, LevelPotentialAndStock left, LevelPotentialAndStock right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left.Equals(right);
        }
    }
}
