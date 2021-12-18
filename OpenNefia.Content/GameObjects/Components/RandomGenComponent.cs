using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects.Components
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

        [DataField]
        public FltSelect FltSelect { get; set; } = FltSelect.None;
    }

    public enum FltSelect : int
    {
        None = 0,
        Sp = 1,
        Unique = 2,
        SpUnique = 3,
        Friend = 4,
        Town = 5,
        SfTown = 6,
        Shop = 7,
        Snow = 8,
        TownSp = 9,
    }
}
