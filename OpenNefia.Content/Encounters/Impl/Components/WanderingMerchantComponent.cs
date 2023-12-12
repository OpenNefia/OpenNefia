using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Encounters
{
    /// <summary>
    /// Defines some specific behavior for wandering merchants (spawned from random encounters):
    /// - They are not fooled by incognito.
    /// - They drop a temporal shopkeeper's trunk on death.
    /// - They have an extra dialog option to attack their convoy.
    /// - When chatting with them, they end the encounter when the player leaves the chat.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class WanderingMerchantComponent : Component
    {
    }
}