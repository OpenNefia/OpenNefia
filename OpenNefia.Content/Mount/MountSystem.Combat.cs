using OpenNefia.Content.Combat;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Weight;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.Mount
{
    public sealed partial class MountSystem : EntitySystem, IMountSystem
    {
        private void RefreshSpeed_MountRider(EntityUid rider, MountRiderComponent component, ref EntityRefreshSpeedEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:212 	if gRider!0{ ...
            if (!TryGetMount(rider, out var mount))
                return;

            var mountSPD = _skills.Level(mount.Owner, Protos.Skill.AttrSpeed);
            var mountSTR = _skills.Level(mount.Owner, Protos.Skill.AttrStrength);
            var riderRiding = _skills.Level(rider, Protos.Skill.Riding);
            var suitabilityBonus = mount.Suitability == MountSuitability.Good ? 50 : 0;

            args.OutSpeed = (mountSPD * 100) / int.Clamp(100 + mountSPD - (int)(mountSTR * 1.5) - riderRiding * 2 - suitabilityBonus, 100, 1000);
            if (mount.Suitability == MountSuitability.Bad)
                args.OutSpeed /= 10;
            // <<<<<<<< elona122/shade2/chara_func.hsp:216 		} ...
        }

        private void RefreshSpeed_Mount(EntityUid mount, MountComponent component, ref EntityRefreshSpeedEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:215 		if gRider=c:cSpeed(c)=limit(sSTR(c)+sRiding(pc), ...
            if (!TryGetRider(mount, out var rider))
                return;

            var mountSPD = _skills.Level(mount, Protos.Skill.AttrSpeed);
            var mountSTR = _skills.Level(mount, Protos.Skill.AttrStrength);
            var riderRiding = _skills.Level(rider.Owner, Protos.Skill.Riding);

            args.OutSpeed = int.Clamp(mountSTR + riderRiding, 10, Math.Max(10, mountSPD));
            // <<<<<<<< elona122/shade2/chara_func.hsp:215 		if gRider=c:cSpeed(c)=limit(sSTR(c)+sRiding(pc), ...
        }

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
