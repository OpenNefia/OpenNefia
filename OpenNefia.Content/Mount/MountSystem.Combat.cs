using OpenNefia.Content.Combat;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Weight;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.Mount
{
    public sealed partial class MountSystem : EntitySystem, IMountSystem
    {
        private void HandleCalcAccuracyRider(EntityUid uid, MountRiderComponent component, ref CalcPhysicalAttackAccuracyEvent args)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:195 		if cc=pc{ ...
            if (!IsMounting(uid))
                return;

            args.OutAccuracy = args.OutAccuracy * 100 / Math.Clamp(150 - _skills.Level(uid, Protos.Skill.Riding) / 2, 115, 150);
            if (EntityManager.IsAlive(args.Weapon) && !args.IsRanged
                && TryComp<WeightComponent>(args.Weapon.Value, out var weight) && weight.Weight.Buffed >= 400)
            {
                args.OutAccuracy -= (weight.Weight.Buffed - 4000 + 400) / (10 + _skills.Level(uid, Protos.Skill.Riding) / 5);
            }
            // <<<<<<<< elona122/shade2/calculation.hsp:198 			} ...
        }

        private void HandleCalcAccuracyMount(EntityUid uid, MountComponent component, ref CalcPhysicalAttackAccuracyEvent args)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:199 		if cc=gRider{ ...
            if (!IsBeingMounted(uid))
                return;

            args.OutAccuracy = args.OutAccuracy * 100 / Math.Clamp(150 - _skills.Level(uid, Protos.Skill.AttrStrength) / 2, 115, 150);
            if (EntityManager.IsAlive(args.Weapon) && !args.IsRanged
                && TryComp<WeightComponent>(args.Weapon.Value, out var weight) && weight.Weight.Buffed >= 400)
            {
                args.OutAccuracy -= (weight.Weight.Buffed - 4000 + 400) / (10 + _skills.Level(uid, Protos.Skill.AttrStrength) / 5);
            }
            // <<<<<<<< elona122/shade2/calculation.hsp:203 		} ...
        }
    }
}
