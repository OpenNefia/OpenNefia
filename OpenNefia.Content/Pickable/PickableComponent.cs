using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Pickable
{
    [RegisterComponent]
    public class PickableComponent : Component, IFromHspItem
    {
        public override string Name => "Pickable";

        /// <summary>
        /// Ownership state of this entity. Determines if it's possible
        /// for the player to pick it up.
        /// </summary>
        [DataField]
        public OwnState OwnState { get; set; }

        /// <summary>
        /// If true, the inventory UI should prevent the player from dropping
        /// this entity.
        /// </summary>
        [DataField]
        public bool IsNoDrop { get; set; }

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
