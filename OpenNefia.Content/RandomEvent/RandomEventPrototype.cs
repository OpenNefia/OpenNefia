using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNefia.Content.RandomEvent
{
    [Prototype("Elona.RandomEvent")]
    public class RandomEventPrototype : IPrototype, IHspIds<int>
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        /// <summary>
        /// Image displayed in the random event popup.
        /// </summary>
        [DataField]
        public PrototypeId<AssetPrototype> Image { get; set; }

        /// <summary>
        /// Number of choices for the random event popup.
        /// The corresponding localization should have as many strings as the number of choices.
        /// </summary>
        [DataField]
        public int ChoiceCount { get; set; }

        /// <summary>
        /// Skill roll based on luck the player needs to skip this event
        /// and turn it into "Avoiding Misfortune".
        /// </summary>
        [DataField]
        public int? LuckThresholdToSkip { get; set; } = null;
    }
}