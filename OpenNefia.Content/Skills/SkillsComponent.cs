﻿using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Skills
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
        public Dictionary<PrototypeId<SkillPrototype>, LevelAndPotential> Skills { get; } = new();

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

        public int Level(PrototypeId<SkillPrototype> id)
        {
            if (!TryGetKnown(id, out var level))
                return 0;

            return level.Level.Buffed;
        }
    }
}