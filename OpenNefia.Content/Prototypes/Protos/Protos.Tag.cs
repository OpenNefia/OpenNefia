using TagPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.GameObjects.TagPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Tag
        {
            #pragma warning disable format

            public static readonly TagPrototypeId DungeonStairsDelving   = new($"Elona.{nameof(DungeonStairsDelving)}");
            public static readonly TagPrototypeId DungeonStairsSurfacing = new($"Elona.{nameof(DungeonStairsSurfacing)}");

            #pragma warning restore format
        }
    }
}
