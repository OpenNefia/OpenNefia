using System;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects
{
    [RegisterComponent]
    public class MoveableComponent : Component
    {
        public override string Name => "Moveable";
    }
}
