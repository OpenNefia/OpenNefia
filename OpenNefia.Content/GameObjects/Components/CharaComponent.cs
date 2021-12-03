using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class CharaComponent : Component
    {
        public override string Name => "Chara";

        [DataField]
        public string Title { get; set; } = string.Empty;

        [DataField]
        public bool HasFullName { get; set; } = false;

        [DataField(required: true)]
        public PrototypeId<ClassPrototype> Class { get; set; } = default;
    }
}