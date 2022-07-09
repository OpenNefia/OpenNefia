using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Food
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class RottenFoodProtectionComponent : Component
    {
        public override string Name => "RottenFoodProtection";

        [DataField]
        public bool IsProtectedFromRottenFood { get; set; }
    }
}