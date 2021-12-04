using OpenNefia.Core.Prototypes;
using BasicAnimPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Rendering.BasicAnimPrototype>;

namespace OpenNefia.Core.Rendering
{
    [PrototypeOfEntries]
    public static class BasicAnimPrototypeOf
    {
        public static BasicAnimPrototypeId AnimSmoke = new(nameof(AnimSmoke));
        public static BasicAnimPrototypeId AnimSparkle = new(nameof(AnimSparkle));
        public static BasicAnimPrototypeId AnimBuff = new(nameof(AnimBuff));
        public static BasicAnimPrototypeId AnimCurse = new(nameof(AnimCurse));
        public static BasicAnimPrototypeId AnimElec = new(nameof(AnimElec));
    }
}
