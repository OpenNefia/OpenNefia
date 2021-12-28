using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects.Pickable
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

    /// <summary>
    /// Possible ownership states for entities with a <see cref="PickableComponent"/>.
    /// </summary>
    public enum OwnState : int
    {
        /// <summary>
        /// Nothing claims ownership of this item, and the player can pick it up.
        /// </summary>
        None = 0,

        /// <summary>
        /// This item cannot be picked up by the player. ("It's not yours.")
        /// </summary>
        NPC = 1,

        Shop = 2,
        Construct = 3,
        Quest = 4,
        Special = 5
    }
}
