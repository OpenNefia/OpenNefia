using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Effects.New.Unique
{
    /// <summary>
    /// Spawns a new character and recruits them as an ally.
    /// </summary>
    [RegisterComponent]
    public sealed class EffectGainAllyComponent : Component
    {
        [DataField]
        public CharaFilter CharaFilter { get; set; } = new();

        [DataField]
        public LocaleKey? MessageKey { get; set; }
    }
}