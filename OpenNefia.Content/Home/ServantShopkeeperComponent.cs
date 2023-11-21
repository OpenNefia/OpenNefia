using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.Home
{
    /// <summary>
    /// Replaces itself on <see cref="EntityBeingGeneratedEvent"/> with a <see cref="Shopkeeper.RoleShopkeeperComponent"/> that has a randomly chosen shopkeeper role based on engine variable <c>Elona.ServantShopkeeperChoices</c>.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ServantShopkeeperComponent : Component
    {
    }
}