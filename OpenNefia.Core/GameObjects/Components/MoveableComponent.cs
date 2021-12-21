using System;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Indicates an entity that can move around and be bumped into.
    /// </summary>
    /// <remarks>
    /// This is meant to replicate some of the logic of characters.
    /// </remarks>
    [RegisterComponent]
    public class MoveableComponent : Component
    {
        public override string Name => "Moveable";
    }
}
