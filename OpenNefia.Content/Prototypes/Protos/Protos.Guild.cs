using GuildPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Guild.GuildPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Guild
        {
            #pragma warning disable format

            public static readonly GuildPrototypeId Mage    = new($"Elona.{nameof(Mage)}");
            public static readonly GuildPrototypeId Fighter = new($"Elona.{nameof(Fighter)}");
            public static readonly GuildPrototypeId Thief   = new($"Elona.{nameof(Thief)}");

            #pragma warning restore format
        }
    }
}
