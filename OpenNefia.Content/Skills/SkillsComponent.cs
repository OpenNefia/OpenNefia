using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Stats;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.TurnOrder;

namespace OpenNefia.Content.Skills
{
    /// <summary>
    /// Holds HP/MP/stamina and skill data. (Skills and stats are treated the same.)
    /// </summary>
    /// <remarks>
    /// Since max HP/MP/stamina are calculated from stats, I can't really see the benefit of separating the two.
    /// </remarks>
    [RegisterComponent]
    public sealed class SkillsComponent : Component
    {
        public override string Name => "Skills";

        /// <summary>
        /// Current hitpoints.
        /// </summary>
        [DataField]
        public int HP { get; set; }

        /// <summary>
        /// Maximum hitpoints.
        /// </summary>
        [DataField]
        public int MaxHP { get; set; }

        /// <summary>
        /// Current magic points.
        /// </summary>
        [DataField]
        public int MP { get; set; }

        /// <summary>
        /// Maximum magic points.
        /// </summary>
        [DataField]
        public int MaxMP { get; set; }

        /// <summary>
        /// Current stamina.
        /// </summary>
        [DataField]
        public int Stamina { get; set; }

        /// <summary>
        /// Maximum stamina.
        /// </summary>
        [DataField]
        public int MaxStamina { get; set; }

        /// <summary>
        /// Current Defense Value.
        /// </summary>
        [DataField]
        public Stat<int> DV { get; set; } = new(0);

        /// <summary>
        /// Current Protection Value.
        /// </summary>
        [DataField]
        public Stat<int> PV { get; set; } = new(0);

        /// <summary>
        /// Current hit bonus.
        /// </summary>
        [DataField]
        public Stat<int> HitBonus { get; set; } = new(0);

        /// <summary>
        /// Current damage bonus.
        /// </summary>
        [DataField]
        public Stat<int> DamageBonus { get; set; } = new(0);

        /// <summary>
        /// Level, potential and experience for skills and stats.
        /// </summary>
        [DataField]
        public Dictionary<PrototypeId<SkillPrototype>, LevelAndPotential> Skills { get; } = new();

        /// <summary>
        /// Number of spendable skill bonus points.
        /// </summary>
        [DataField]
        public int BonusPoints { get; set; } = 0;

        /// <summary>
        /// Adjustment to <see cref="Content.Prototypes.Protos.Skill.AttrSpeed"/>.
        /// Affected by the number of body parts.
        /// </summary>
        [DataField]
        public int SpeedCorrection { get; set; } = 0;

        /// <summary>
        /// Whether or not the entity regains HP/MP/stamina at the end of this turn.
        /// The idea is that this is set to <code>true</code> on the <see cref="EntityTurnStartingEventArgs"/>
        /// and modified during the <see cref="EntityTurnEndingEventArgs"/>.
        /// </summary>
        [DataField]
        public bool CanRegenerateThisTurn { get; set; } = true;

        /// <summary>
        /// Total number of skill bonus points this character has 
        /// gained in their lifetime.
        /// </summary>
        [DataField]
        public int TotalBonusPointsEarned { get; set; } = 0;

        public LevelAndPotential Ensure(PrototypeId<SkillPrototype> protoId)
        {
            if (Skills.TryGetValue(protoId, out var level))
                return level;

            return new LevelAndPotential()
            {
                Level = new(0)
            };
        }

        public LevelAndPotential Ensure(SkillPrototype proto) => Ensure(proto.GetStrongID());

        public bool TryGetKnown(SkillPrototype proto, [NotNullWhen(true)] out LevelAndPotential? level)
            => TryGetKnown(proto.GetStrongID(), out level);
        public bool TryGetKnown(PrototypeId<SkillPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level)
        {
            if (!Skills.TryGetValue(protoId, out level))
            {
                return false;
            }

            if (level.Level.Base <= 0)
            {
                return false;
            }

            return true;
        }

        public bool HasSkill(SkillPrototype proto)
            => HasSkill(proto.GetStrongID());
        public bool HasSkill(PrototypeId<SkillPrototype> id)
        {
            return TryGetKnown(id, out _);
        }

        public int Level(SkillPrototype proto) => Level(proto.GetStrongID());
        public int Level(PrototypeId<SkillPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Level.Buffed;
        }

        public int BaseLevel(SkillPrototype proto) => BaseLevel(proto.GetStrongID());
        public int BaseLevel(PrototypeId<SkillPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Level.Base;
        }

        public int Potential(SkillPrototype proto) => Potential(proto.GetStrongID());
        public int Potential(PrototypeId<SkillPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Potential;
        }
    }
}
