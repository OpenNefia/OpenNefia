using OpenNefia.Content.Spells;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Spells
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
        public Dictionary<PrototypeId<SpellPrototype>, LevelPotentialAndStock> Spells { get; } = new();

        public LevelPotentialAndStock Ensure(PrototypeId<SpellPrototype> skillID)
        {
            if (!Spells.TryGetValue(skillID, out var spell))
            {
                spell = new LevelPotentialAndStock();
                Spells[skillID] = spell;
            }

            return spell;
        }

        public bool TryGetKnown(PrototypeId<SpellPrototype> protoId, [NotNullWhen(true)] out LevelPotentialAndStock? level)
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

        public bool HasSkill(SpellPrototype proto)
            => HasSkill(proto.GetStrongID());
        public bool HasSkill(PrototypeId<SpellPrototype> id)
        {
            return TryGetKnown(id, out _);
        }

        public int Level(SpellPrototype proto) => Level(proto.GetStrongID());
        public int Level(PrototypeId<SpellPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Level.Buffed;
        }

        public int BaseLevel(SpellPrototype proto) => BaseLevel(proto.GetStrongID());
        public int BaseLevel(PrototypeId<SpellPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Level.Base;
        }

        public int Potential(SpellPrototype proto) => Potential(proto.GetStrongID());
        public int Potential(PrototypeId<SpellPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Potential;
        }
    }
}
