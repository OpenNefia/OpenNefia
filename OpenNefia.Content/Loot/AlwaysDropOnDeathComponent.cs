using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Loot
{
    /// <summary>
    /// When applied to an entity, it will always be dropped as loot if its owning chara is killed.
    /// Used by platinum coins and target items in "I Want It!" quests.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class AlwaysDropOnDeathComponent : Component
    {    }
}