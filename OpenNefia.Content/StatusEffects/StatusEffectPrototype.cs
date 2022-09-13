using OpenNefia.Content.Resists;
using OpenNefia.Core;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Content.StatusEffects
{
    [Prototype("Elona.StatusEffect")]
    public sealed class StatusEffectPrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField]
        public bool StopsActivity { get; } = false;

        /// <summary>
        /// If true, this effect will be healed when the turn starts.
        /// False for sickness and choking on mochi.
        /// </summary>
        [DataField]
        public bool AutoHeal { get; } = false;

        [DataField]
        public string? EmotionIconId { get; }

        [DataField]
        public PrototypeId<ElementPrototype>? RelatedElement { get; }

        [DataField("indicators")]
        private List<int> _indicators { get; } = new();

        public IReadOnlyList<int> Indicators => _indicators;

        [DataField]
        public bool RemoveOnSleep { get; set; } = false;

        [DataField]
        public Color Color { get; set; } = Color.Black;

        [DataField]
        public ComponentRegistry Components { get; } = new();
    }
}