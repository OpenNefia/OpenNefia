using SidequestPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Sidequests.SidequestPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Sidequest
        {
            #pragma warning disable format

            public static readonly SidequestPrototypeId MainQuest = new($"Elona.{nameof(MainQuest)}");
            public static readonly SidequestPrototypeId Tutorial  = new($"Elona.{nameof(Tutorial)}");

            #pragma warning restore format
        }
    }
}
