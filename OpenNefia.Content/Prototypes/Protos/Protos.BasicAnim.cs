using OpenNefia.Core.Prototypes;
using BasicAnimPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Rendering.BasicAnimPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class BasicAnim
        {
            #pragma warning disable format

            public static readonly BasicAnimPrototypeId AnimSmoke   = new($"Elona.{nameof(AnimSmoke)}");
            public static readonly BasicAnimPrototypeId AnimSparkle = new($"Elona.{nameof(AnimSparkle)}");
            public static readonly BasicAnimPrototypeId AnimBuff    = new($"Elona.{nameof(AnimBuff)}");
            public static readonly BasicAnimPrototypeId AnimCurse   = new($"Elona.{nameof(AnimCurse)}");
            public static readonly BasicAnimPrototypeId AnimElec    = new($"Elona.{nameof(AnimElec)}");

            #pragma warning restore format
        }
    }
}
