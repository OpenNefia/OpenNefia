using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects.Components
{
    [RegisterComponent]
    public class EntitySourceComponent : Component
    {
        [DataField]
        public EntityUid Source { get; set; }
    }
}
