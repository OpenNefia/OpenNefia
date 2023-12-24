using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Mefs
{
    /// <summary>
    /// Prevents movement if a character is caught in it.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MefWebComponent : Component
    {
    }

    /// <summary>
    /// Sometimes nullifies melee attacks
    /// if the attacker is standing on top of one.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MefMistOfDarknessComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MefAcidGroundComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MefEtherGroundComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MefFireComponent : Component
    {
    }

    /// <summary>
    /// *ゴゴゴゴゴゴ*
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MefNuclearBombComponent : Component
    {
    }
}