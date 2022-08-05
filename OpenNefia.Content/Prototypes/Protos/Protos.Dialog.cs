using DialogPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Dialog.DialogPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Dialog
        {
            public static readonly DialogPrototypeId Villager = new($"Elona.{nameof(Villager)}");
            public static readonly DialogPrototypeId Trainer = new($"Elona.{nameof(Trainer)}");
            public static readonly DialogPrototypeId Prostitute = new($"Elona.{nameof(Prostitute)}");
        }
    }
}
