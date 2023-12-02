using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomGen
{
    /// <summary>
    /// Controls the rarity of spawning this entity.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal | ComponentTarget.Enchantment | ComponentTarget.Quest)]
    public class RandomGenComponent : Component
    {
        /// <summary>
        /// Random generation tables for this entity. They are typically referred to by entity prototype components,
        /// not instanced entity components.
        /// By adding an entry for one of the standard tables, you permit that entity to be spawned through the
        /// use of random generation systems. For example, the <see cref="RandomGenTables.Chara"/> table
        /// controls which characters are spawned via <see cref="ICharaGen.GenerateChara"/>. If there
        /// is no entry for that table, the character will never be spawned via random generation.
        /// </summary>
        [DataField]
        public Dictionary<string, RandomGenTable> Tables = new();
    }

    /// <summary>
    /// Standard random generation tables.
    /// </summary>
    public static class RandomGenTables
    {
        /// <summary>
        /// Used by <see cref="ICharaGen.GenerateChara"/>.
        /// </summary>
        public const string Chara = "chara";

        /// <summary>
        /// Used by <see cref="IItemGen.GenerateItem"/>.
        /// </summary>
        public const string Item = "item";

        /// <summary>
        /// Used by <see cref="IQuestSystem"/>.
        /// </summary>
        public const string Quest = "quest";
    }

    /// <summary>
    /// Controls random generation parameters.
    /// </summary>
    [DataDefinition]
    public class RandomGenTable
    {
        public const int DefaultRarity = 1000000;

        /// <summary>
        /// Rarity of this entity.
        /// </summary>
        /// <remarks>
        /// This number can be interpreted in different ways depending on which
        /// random generation routine is to be used. For example, characters and items
        /// each follow different weighted formulae, while quests are picked from a
        /// "1 in N chance"-style pool.
        /// </remarks>
        [DataField]
        public int Rarity { get; set; } = DefaultRarity;

        /// <summary>
        /// Coefficient, used by character and item random generation.
        /// </summary>
        [DataField]
        public int Coefficient { get; set; } = 100;

        /// <summary>
        /// Filters generated entities by a special tag. This way, you can for example
        /// generate a set of trees in a winter region, but filter by only the trees with 
        /// a snowy theme.
        /// </summary>
        /// <remarks>
        /// TODO
        /// </remarks>
        [DataField("fltselect")]
        public string? FltSelect { get; set; } = null;
    }

    /// <summary>
    /// List of fltselects in vanilla.
    /// </summary>
    public static class FltSelects
    {
        public const string None = $"Elona.{nameof(None)}";
        public const string Sp = $"Elona.{nameof(Sp)}";
        public const string Unique = $"Elona.{nameof(Unique)}";
        public const string SpUnique = $"Elona.{nameof(SpUnique)}";
        public const string Friend = $"Elona.{nameof(Friend)}";
        public const string Town = $"Elona.{nameof(Town)}";
        public const string SfTown = $"Elona.{nameof(SfTown)}";
        public const string Shop = $"Elona.{nameof(Shop)}";
        public const string Snow = $"Elona.{nameof(Snow)}";
        public const string TownSp = $"Elona.{nameof(TownSp)}";
    }
}
