using BookPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Book.BookPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Book
        {
            #pragma warning disable format

            public static readonly BookPrototypeId MyDiary           = new($"Elona.{nameof(MyDiary)}");
            public static readonly BookPrototypeId BeginnersGuide    = new($"Elona.{nameof(BeginnersGuide)}");
            public static readonly BookPrototypeId ItsABug           = new($"Elona.{nameof(ItsABug)}");
            public static readonly BookPrototypeId DontReadThis      = new($"Elona.{nameof(DontReadThis)}");
            public static readonly BookPrototypeId MuseumGuide       = new($"Elona.{nameof(MuseumGuide)}");
            public static readonly BookPrototypeId CrimberryAddict   = new($"Elona.{nameof(CrimberryAddict)}");
            public static readonly BookPrototypeId CatsCradle        = new($"Elona.{nameof(CatsCradle)}");
            public static readonly BookPrototypeId HerbEffect        = new($"Elona.{nameof(HerbEffect)}");
            public static readonly BookPrototypeId ShopkeeperGuide   = new($"Elona.{nameof(ShopkeeperGuide)}");
            public static readonly BookPrototypeId EasyGardening     = new($"Elona.{nameof(EasyGardening)}");
            public static readonly BookPrototypeId Water             = new($"Elona.{nameof(Water)}");
            public static readonly BookPrototypeId BreedersGuide     = new($"Elona.{nameof(BreedersGuide)}");
            public static readonly BookPrototypeId StrangeDiary      = new($"Elona.{nameof(StrangeDiary)}");
            public static readonly BookPrototypeId PyramidInvitation = new($"Elona.{nameof(PyramidInvitation)}");
            public static readonly BookPrototypeId CardGameManual    = new($"Elona.{nameof(CardGameManual)}");
            public static readonly BookPrototypeId DungeonGuide      = new($"Elona.{nameof(DungeonGuide)}");

            #pragma warning restore format
        }
    }
}
