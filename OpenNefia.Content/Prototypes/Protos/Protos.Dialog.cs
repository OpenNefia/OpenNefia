using DialogPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Dialog.DialogPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Dialog
        {
            public static readonly DialogPrototypeId IgnoresYou = new($"Elona.{nameof(IgnoresYou)}");
            public static readonly DialogPrototypeId IsBusy = new($"Elona.{nameof(IsBusy)}");
            public static readonly DialogPrototypeId IsSleeping = new($"Elona.{nameof(IsSleeping)}");
            
            public static readonly DialogPrototypeId Default = new($"Elona.{nameof(Default)}");
            public static readonly DialogPrototypeId Ally = new($"Elona.{nameof(Ally)}");

            public static readonly DialogPrototypeId Trainer = new($"Elona.{nameof(Trainer)}");
            public static readonly DialogPrototypeId Prostitute = new($"Elona.{nameof(Prostitute)}");
            public static readonly DialogPrototypeId Innkeeper = new($"Elona.{nameof(Innkeeper)}");
            public static readonly DialogPrototypeId Shopkeeper = new($"Elona.{nameof(Shopkeeper)}");

            public static readonly DialogPrototypeId LomiasNewGame = new($"Elona.{nameof(LomiasNewGame)}");
        }
    }
}
