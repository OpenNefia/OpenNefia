using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Maths;

namespace OpenNefia.LecchoTorte.TurnColor
{
    [RegisterComponent]
    public class TurnColorComponent : Component
    {
        public override string Name => "LecchoTorte.TurnColor";

        [DataField(required: true)]
        public Color Color { get; set; } = Color.White;
    }
}
