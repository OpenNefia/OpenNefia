using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects.Components
{
    [RegisterComponent]
    public class EntitySourceComponent : Component
    {
        public override string Name => "EntitySource";

        [DataField]
        public EntityUid Source { get; set; }
    }
}
