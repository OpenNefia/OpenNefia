using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
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
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Content.Activity
{
    [Prototype("Elona.Activity")]
    public class ActivityPrototype : IPrototype, IHspIds<int>
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField]
        public int DefaultTurns { get; set; } = 10;

        [DataField]
        public int AnimationWait { get; set; }

        [DataField]
        public ActivityInterruptAction OnInterrupt { get; set; }

        [DataField]
        public bool InterruptOnDisplace { get; set; } = false;

        [DataField]
        public ComponentRegistry Components { get; } = new();
    }

    public enum ActivityInterruptAction
    {
        Ignore,
        Prompt,
        Stop
    }
}