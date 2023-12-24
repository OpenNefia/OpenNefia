using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using OpenNefia.Content.Effects.New;

namespace OpenNefia.Content.Mefs
{
    /// <summary>
    /// Represents "map effects". These are special effects on the map
    /// that can be stepped on and have a turn limit for validity.
    /// Additionally, only a single mef can be active on a tile at a time.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MefComponent : Component
    {
        /// <summary>
        /// Time remaining for the mef.
        /// In Elona, a "turn" is a single in-game minute.
        /// </summary>
        [DataField]
        public GameTimeSpan TimeRemaining { get; set; } = GameTimeSpan.FromMinutes(10);

        /// <summary>
        /// Entity responsible for spawning this mef.
        /// Takes the blame for the mef indirectly killing citizens, causing karma loss,
        /// and so on.
        /// </summary>
        [DataField]
        public EntityUid? SpawnedBy { get; set; }

        /// <summary>
        /// General-purpose power value of the mef.
        /// Usage can vary depending on the type of mef.
        /// </summary>
        /// <remarks>
        /// The reason this exists instead of giving each mef component
        /// its own power is because the power can scale when the mef
        /// is spawned using an <see cref="EffectDamageMefComponent"/>.
        /// </remarks>
        [DataField]
        public int Power { get; set; } = 0;

        [DataField]
        public float DecaySpeedModifier { get; set; } = 1f;

        /// <summary>
        /// How this mef should be updated in the world.
        /// </summary>
        [DataField]
        public MefUpdateType UpdateType { get; set; } = MefUpdateType.MapTurnsPassed;
    }

    public enum MefUpdateType
    {
        /// <summary>
        /// The mef will never decay.
        /// </summary>
        LastForever,

        /// <summary>
        /// The mef's <see cref="MefComponent.TimeRemaining"/> will be decremented
        /// by one minute for every turn that passes in the map (per turn ordering,
        /// not in-game time).
        /// The mef will update consistently no matter the player's speed.
        /// </summary>
        MapTurnsPassed,

        /// <summary>
        /// The amount of in-game time that passes will be decremented from
        /// the mef's <see cref="MefComponent.TimeRemaining"/>.
        /// The mef will update faster if the player's speed is high.
        /// </summary>
        TimePassed,
    }
}