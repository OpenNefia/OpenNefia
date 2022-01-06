using BodyPartPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Equipment.BodyPartPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class BodyPart
        {
            public static readonly BodyPartPrototypeId Head = new($"Elona.{nameof(Head)}");
            public static readonly BodyPartPrototypeId Neck = new($"Elona.{nameof(Neck)}");
            public static readonly BodyPartPrototypeId Back = new($"Elona.{nameof(Back)}");
            public static readonly BodyPartPrototypeId Body = new($"Elona.{nameof(Body)}");
            public static readonly BodyPartPrototypeId Hand = new($"Elona.{nameof(Hand)}");
            public static readonly BodyPartPrototypeId Ring = new($"Elona.{nameof(Ring)}");
            public static readonly BodyPartPrototypeId Arm = new($"Elona.{nameof(Arm)}");
            public static readonly BodyPartPrototypeId Waist = new($"Elona.{nameof(Waist)}");
            public static readonly BodyPartPrototypeId Leg = new($"Elona.{nameof(Leg)}");
            public static readonly BodyPartPrototypeId Ranged = new($"Elona.{nameof(Ranged)}");
            public static readonly BodyPartPrototypeId Ammo = new($"Elona.{nameof(Ammo)}");
        }
    }
}
