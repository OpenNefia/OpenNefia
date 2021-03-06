using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomGen
{
    [RegisterComponent]
    public class RandomGenComponent : Component
    {
        public override string Name => "RandomGen";

        [DataField]
        public Dictionary<string, RandomGenTable> Tables = new();
    }

    public static class RandomGenTables
    {
        public const string Item = "item";
        public const string Chara = "chara";
    }

    [DataDefinition]
    public class RandomGenTable
    {
        [DataField]
        public int Rarity { get; set; } = 100000;

        [DataField]
        public int Coefficient { get; set; } = 400;

        [DataField("fltselect")]
        public string? FltSelect { get; set; } = null;
    }

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
