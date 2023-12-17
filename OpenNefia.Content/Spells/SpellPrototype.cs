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

namespace OpenNefia.Content.Spells
{
    /// <summary>
    /// Represents a skill assocated with an effect that has a spell stock.
    /// </summary>
    [Prototype("Elona.Spell")]
    public class SpellPrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField(required: true)]
        public PrototypeId<SkillPrototype> SkillID { get; }

        [DataField(required: true)]
        public PrototypeId<EntityPrototype> EffectID { get; }

        [DataField]
        public int Difficulty { get; set; } = 0;

        [DataField]
        public int MPCost { get; set; } = 0;
    }
}