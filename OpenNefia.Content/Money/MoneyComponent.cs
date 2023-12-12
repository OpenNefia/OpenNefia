using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Currency
{
    /// <summary>
    /// Indicates something that can hold gold and platinum coins, like
    /// characters.
    /// </summary>
    /// <remarks>
    /// TODO: it might be nice to generalize "money" as:
    /// - a prototype that can be attached to a "gold piece/platinum coin"-type item
    /// - a quantity that a character can hold
    /// - an item that can be spawned by dropping X amount of it
    /// that way modders can add their own currency types.
    /// i think this is what SS14 does. 
    /// </remarks>
    [RegisterComponent]
    public sealed class MoneyComponent : Component
    {
        [DataField]
        public int Gold { get; set; }

        [DataField]
        public int Platinum { get; set; }

        [DataField]
        public IntRange? InitialGold { get; set; }

        [DataField]
        public IntRange? InitialPlatinum { get; set; }

        /// <summary>
        /// If <c>false</c>, gold will drop based on vanilla rules:
        /// - always, if the character's quality is better than <see cref="Qualities.Quality.Great"/>
        /// - otherwise, a 1/20 chance
        /// </summary>
        [DataField]
        public bool AlwaysDropsGoldOnDeath { get; set; }
    }
}
