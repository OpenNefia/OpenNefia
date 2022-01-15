using PortraitPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Charas.PortraitPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Portrait
        {
            #pragma warning disable format

            public static readonly PortraitPrototypeId Default = new($"{nameof(Default)}");

            #pragma warning restore format
        }
    }
}
