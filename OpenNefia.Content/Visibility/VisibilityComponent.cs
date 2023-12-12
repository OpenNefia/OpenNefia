using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Visibility
{
    /// <summary>
    /// Various pieces of state related to entity visibility.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class VisibilityComponent : Component, IComponentRefreshable
    {
        /// <summary>
        /// If <c>true</c>, this entity is invisible to others.
        /// </summary>
        [DataField]
        public Stat<bool> IsInvisible { get; set; } = new(false);

        /// <summary>
        /// If <c>true</c>, this entity can see invisible entities.
        /// </summary>
        [DataField]
        public Stat<bool> CanSeeInvisible { get; set; } = new(false);

        /// <summary>
        /// Amount of noise the entity is making. Factors into the chance of
        /// the AI detecting the entity.
        /// </summary>
        [DataField]
        public int Noise { get; set; } = 0;

        /// <summary>
        /// How far this entity can see in tiles.
        /// </summary>
        /// <remarks>
        /// Only applies to the player (for now).
        /// </remarks>
        [DataField]
        public Stat<int> FieldOfViewRadius { get; set; } = new(14);

        public void Refresh()
        {
            IsInvisible.Reset();
            CanSeeInvisible.Reset();
            FieldOfViewRadius.Reset();
        }
    }
}