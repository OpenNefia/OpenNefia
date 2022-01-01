using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Holds HP/MP/stamina and skill data. (Skills and stats are treated the same.)
    /// </summary>
    /// <remarks>
    /// Since max HP/MP/stamina are calculated from stats, I can't really see the benefit of separating the two.
    /// </remarks>
    [RegisterComponent]
    public class SkillsComponent : Component
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
        /// Level, potential and experience for skills and stats.
        /// </summary>
        [DataField]
        public SkillHolder Skills { get; } = new();

        public int Level(PrototypeId<SkillPrototype> id)
        {
            if (!Skills.TryGetKnown(id, out var level))
                return 0;

            return level.Level.Buffed;
        }
    }

    [DataDefinition]
    public class SkillHolder : Dictionary<PrototypeId<SkillPrototype>, LevelAndPotential>
    {
        public bool TryGetKnown(PrototypeId<SkillPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level)
        {
            if (!TryGetValue(protoId, out level))
            {
                return false;
            }

            if (level.Level.Base <= 0)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// The level, potential and experience associated with a skill.
    /// </summary>
    [DataDefinition]
    public class LevelAndPotential
    {
        public const int DEFAULT_POTENTIAL = 100;

        /// <summary>
        /// Level of the skill.
        /// </summary>
        [DataField]
        public Stat<int> Level { get; set; } = 1;

        /// <summary>
        /// Potential of the skill, specified as a percentage. 100 is the baseline.
        /// </summary>
        [DataField]
        public int Potential { get; set; } = DEFAULT_POTENTIAL;

        /// <summary>
        /// Current experience of the skill.
        /// </summary>
        /// <remarks>
        /// In Elona, a new skill level is gained per 1000 experience.
        /// </remarks>
        [DataField]
        public int Experience { get; set; } = 0;

        public override string ToString()
        {
            return $"(Level={Level} Potential={Potential}% EXP={Experience})";
        }
    }
}
