using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Home
{
    /// <summary>
    /// Sets up the tutorial in the new player's home.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class AreaHomeTutorialComponent : Component
    {
        public override string Name => "AreaHomeTutorial";
    }
}