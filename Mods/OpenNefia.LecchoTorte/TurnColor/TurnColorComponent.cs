using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Maths;

namespace OpenNefia.LecchoTorte.TurnColor
{
    [RegisterComponent]
    public class TurnColorComponent : Component
    {
        [DataField(required: true)]
        public Color Color { get; set; } = Color.White;
    }
}
