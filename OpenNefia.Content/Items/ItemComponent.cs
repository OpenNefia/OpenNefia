using OpenNefia.Content.Materials;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Items
{
    [RegisterComponent]
    public class ItemComponent : Component, IComponentLocalizable
    {
        [DataField]
        public bool IsPrecious { get; set; } = false;

        [DataField]
        public bool CanWishFor { get; set; } = true;

        [DataField]
        public Formula? WishAmount { get; set; }

        /// <summary>
        /// A qualifier like "cargo", "dish", "potion", etc. for use in item names. Example: "a
        /// [potion] of teleportation", where the DisplayName in the <see cref="MetaDataComponent"/>
        /// is just "teleportation".
        /// </summary>
        /// <remarks>
        /// The purpose of this field, instead of just setting the name to "potion of
        /// teleportation", is to be able to display the item's name like "3 [pair]s of rubynus
        /// boots."
        /// </remarks>
        [Localize]
        public string? ItemTypeName { get; private set; }

        /// <summary>
        /// The "of" in "a potion of teleportation".
        /// </summary>
        [Localize]
        public string? ItemPreposition { get; private set; }

        [Localize]
        public string? KatakanaName { get; private set; }

        void IComponentLocalizable.LocalizeFromLua(NLua.LuaTable table)
        {
            ItemTypeName = table.GetStringOrNull(nameof(ItemTypeName));
            ItemPreposition = table.GetStringOrNull(nameof(ItemPreposition));
            KatakanaName = table.GetStringOrNull(nameof(KatakanaName));
        }
    }
}
