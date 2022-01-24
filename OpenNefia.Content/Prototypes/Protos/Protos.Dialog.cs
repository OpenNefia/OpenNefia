using DialogPrototypeId     = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Dialog.DialogPrototype>;
using DialogNodePrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Dialog.DialogNodePrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Dialog
        {
            #pragma warning disable format

            public static readonly DialogPrototypeId DialogDefault      = new($"Elona.{nameof(DialogDefault)}");
            public static readonly DialogPrototypeId LomiasGameBegin    = new($"Elona.{nameof(LomiasGameBegin)}");
            
            public static readonly DialogNodePrototypeId TalkDefault    = new($"Elona.{nameof(TalkDefault)}");
            public static readonly DialogNodePrototypeId TalkBye        = new($"Elona.{nameof(TalkBye)}");
            public static readonly DialogNodePrototypeId TalkMore       = new($"Elona.{nameof(TalkMore)}");

#pragma warning restore format
        }
    }
}
