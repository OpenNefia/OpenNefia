using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Effects.New.Unique
{
    /// <summary>
    /// Inflicts weakness.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTouchOfWeaknessComponent : Component
    {
    }

    /// <summary>
    /// Inflicts hunger.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTouchOfHungerComponent : Component
    {
    }

    /// <summary>
    /// Cuts HP.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectManisDisassemblyComponent : Component
    {
    }
}
