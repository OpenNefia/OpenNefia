using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNefia.Content.Effects;

namespace OpenNefia.Content.Actions
{
    /// <summary>
    /// An action is a skill associated with an effect, and
    /// can appear in the Skill menu.
    /// </summary>
    [Prototype("Elona.Action")]
    public class ActionPrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        /// <summary>
        /// Associated skill. If the level is non-zero, the entity knows this action. 
        /// Note that the skill level is not used for power calculation; it is only used
        /// to check the existence of the skill.
        /// </summary>
        [DataField(required: true)]
        public PrototypeId<SkillPrototype> SkillID { get; }

        /// <summary>
        /// Effect to invoke when the action is used.
        /// </summary>
        [DataField(required: true)]
        public PrototypeId<EntityPrototype> EffectID { get; }

        [DataField]
        public int Difficulty { get; }

        /// <summary>
        /// Stamina cost of invoking the action.
        /// </summary>
        [DataField]
        public int StaminaCost { get; set; } = 0;

        /// <summary>
        /// Maximum range of the action in tiles.
        /// </summary>
        [DataField]
        public int MaxRange { get; set; } = 1;
    }
}