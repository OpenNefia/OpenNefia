using OpenNefia.Content.Audio;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Resists
{
    [Prototype("Elona.Element")]
    public class ElementPrototype : IPrototype
    {
        /// <inheritdoc />
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField]
        public bool CanResist { get; } = false;

        [DataField]
        public Color Color { get; }

        [DataField("uiColor")]
        public Color UIColor { get; }

        [DataField]
        public SoundSpecifier? Sound { get; }

        [DataField]
        public PrototypeId<AssetPrototype>? DeathAnim { get; }

        [DataField]
        public int DeathAnimDy { get; } = -16;

        [DataField]
        public int Rarity { get; set; } = 1;

        [DataField]
        public bool PreservesSleep { get; set; } = false;
    }
}
