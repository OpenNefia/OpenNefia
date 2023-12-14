using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Charas
{
    /// <summary>
    /// Tags a character as belonging to a group of characters
    /// that can be used as part of a monster house.
    /// </summary>
    // TODO use TagComponent instead!
    [RegisterComponent]
    public class CreaturePackComponent : Component
    {
        [DataField("category", required: true)]
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Set of creature packs defined by content.
    /// </summary>
    public static class CreaturePacks
    {
        public const string None = $"Elona.{nameof(None)}";
        public const string Goblin = $"Elona.{nameof(Goblin)}";
        public const string Orc = $"Elona.{nameof(Orc)}";
        public const string Slime = $"Elona.{nameof(Slime)}";
        public const string Elea = $"Elona.{nameof(Elea)}";
        public const string Kobolt = $"Elona.{nameof(Kobolt)}";
        public const string Spider = $"Elona.{nameof(Spider)}";
        public const string Yeek = $"Elona.{nameof(Yeek)}";
        public const string Mercenary = $"Elona.{nameof(Mercenary)}";
        public const string Zombie = $"Elona.{nameof(Zombie)}";
        public const string Dog = $"Elona.{nameof(Dog)}";
        public const string Bear = $"Elona.{nameof(Bear)}";
        public const string Kamikaze = $"Elona.{nameof(Kamikaze)}";
        public const string Mummy = $"Elona.{nameof(Mummy)}";
        public const string HoundFire = $"Elona.{nameof(HoundFire)}";
        public const string HoundIce = $"Elona.{nameof(HoundIce)}";
        public const string HoundLightning = $"Elona.{nameof(HoundLightning)}";
        public const string HoundDarkness = $"Elona.{nameof(HoundDarkness)}";
        public const string HoundMind = $"Elona.{nameof(HoundMind)}";
        public const string HoundNerve = $"Elona.{nameof(HoundNerve)}";
        public const string HoundPoison = $"Elona.{nameof(HoundPoison)}";
        public const string HoundSound = $"Elona.{nameof(HoundSound)}";
        public const string HoundNether = $"Elona.{nameof(HoundNether)}";
        public const string HoundChaos = $"Elona.{nameof(HoundChaos)}";
    }
}