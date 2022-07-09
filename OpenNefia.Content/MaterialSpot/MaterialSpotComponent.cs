using OpenNefia.Content.Activity;
using OpenNefia.Content.Audio;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.MaterialSpot
{
    // TODO: use [Localize]
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MaterialSpotComponent : Component
    {
        public override string Name => "MaterialSpot";

        /// <summary>
        /// Must inherit from <see cref="BaseAutoTurnAnim"/>.
        /// </summary>
        [DataField]
        public Type AutoTurnAnim { get; set; } = typeof(SearchingAutoTurnAnim);

        [DataField]
        public PrototypeId<MaterialSpotPrototype>? MaterialSpotType { get; set; } = null;

        [DataField]
        public LocaleKey ActivityName { get; set; } = "OpenNefia.Prototypes.Activity.Elona.Searching.Verb";

        [DataField]
        public int ActivityDefaultTurns { get; set; } = 20;

        [DataField]
        public int ActivityAnimationWait { get; set; } = 15;

        [DataField]
        public LocaleKey SteppedOnText { get; set; } = "Elona.MaterialSpot.Spot.Description";

        [DataField]
        public LocaleKey StartGatherText { get; set; } = "Elona.MaterialSpot.Spot.Start";

        [DataField]
        public SoundSpecifier? StartGatherSound { get; set; }

        [DataField]
        public LocaleKey? GatherSoundText { get; set; }

        [DataField]
        public LocaleKey GatherNoMoreText { get; set; } = "Elona.MaterialSpot.Spot.NoMore";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MaterialSpotRemainsComponent : Component
    {
        public override string Name => "MaterialSpotRemains";
    }
}