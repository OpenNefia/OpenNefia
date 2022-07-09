using FoodTypePrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Food.FoodTypePrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class FoodType
        {
            #pragma warning disable format

            public static readonly FoodTypePrototypeId Meat      = new($"Elona.{nameof(Meat)}");
            public static readonly FoodTypePrototypeId Vegetable = new($"Elona.{nameof(Vegetable)}");
            public static readonly FoodTypePrototypeId Fruit     = new($"Elona.{nameof(Fruit)}");
            public static readonly FoodTypePrototypeId Sweet     = new($"Elona.{nameof(Sweet)}");
            public static readonly FoodTypePrototypeId Pasta     = new($"Elona.{nameof(Pasta)}");
            public static readonly FoodTypePrototypeId Fish      = new($"Elona.{nameof(Fish)}");
            public static readonly FoodTypePrototypeId Bread     = new($"Elona.{nameof(Bread)}");
            public static readonly FoodTypePrototypeId Egg       = new($"Elona.{nameof(Egg)}");

            #pragma warning restore format
        }
    }
}
