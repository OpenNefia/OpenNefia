using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Hunger
{
    [RegisterComponent]
    public sealed class HungerComponent : Component
    {
        public override string Name => "Hunger";

        [DataField]
        public int Nutrition { get; set; } = 0;

        [DataField]
        public bool IsAnorexic { get; set; }

        [DataField]
        public int AnorexiaCounter { get; set; }
    }

    public static class HungerLevels
    {
        public const int VeryHungry = 1000;
        public const int Hungry = 2000;
        public const int Normal = 5000;
        public const int Ally = 6000;
        public const int Satisfied = 10000;
        public const int Bloated = 12000;
        public const int InnkeeperMeal = 15000;
        public const int Vomit = 35000;
    }
}
