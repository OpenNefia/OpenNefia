using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.GameObjects
{
    public static partial class Protos
    {
        public static class Mef
        {
            public static readonly EntityPrototypeId Web = new($"Elona.MefWeb");
            public static readonly EntityPrototypeId Mist = new($"Elona.MefMist");
            public static readonly EntityPrototypeId Acid = new($"Elona.MefAcid");
            public static readonly EntityPrototypeId Ether = new($"Elona.MefEther");
            public static readonly EntityPrototypeId Fire = new($"Elona.MefFire");
            public static readonly EntityPrototypeId Potion = new($"Elona.MefPotion");
            public static readonly EntityPrototypeId Nuke = new($"Elona.MefNuke");
        }
    }
}
