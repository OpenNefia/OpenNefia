using OpenNefia.Core.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class PickableComponent : Component, IFromHspItem
    {
        public override string Name => "Pickable";

        [DataField]
        public OwnState OwnState { get; set; }

        public void FromHspItem(OwnState ownState)
        {
            OwnState = ownState;
        }
    }
}
