using OpenNefia.Content.Combat;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Weight;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Mount
{
    public interface IMountSystem : IEntitySystem
    {
        bool HasMount(EntityUid uid, MountRiderComponent? mountRider = null);
        bool IsBeingMounted(EntityUid uid, MountComponent? mount = null);
    }

    public sealed class MountSystem : EntitySystem, IMountSystem
    {
        [Dependency] private readonly ISkillsSystem _skills = default!;

        public override void Initialize()
        {
            SubscribeComponent<MountRiderComponent, CalcPhysicalAttackAccuracyEvent>(HandleCalcAccuracyRider);
            SubscribeComponent<MountComponent, CalcPhysicalAttackAccuracyEvent>(HandleCalcAccuracyMount);
        }

        private void HandleCalcAccuracyRider(EntityUid uid, MountRiderComponent component, ref CalcPhysicalAttackAccuracyEvent args)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:195 		if cc=pc{ ...
            if (!HasMount(uid))
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

        public bool HasMount(EntityUid uid, MountRiderComponent? mountRider = null)
        {
            if (!Resolve(uid, ref mountRider, logMissing: false))
                return false;

            return EntityManager.IsAlive(mountRider.Mount)
                && TryComp<MountComponent>(mountRider.Mount, out var mount)
                && mount.Rider == uid
                && Spatial(mountRider.Mount).ParentUid == uid;
        }

        public bool IsBeingMounted(EntityUid uid, MountComponent? mount = null)
        {
            if (!Resolve(uid, ref mount, logMissing: false))
                return false;

            return EntityManager.IsAlive(mount.Rider)
                && TryComp<MountRiderComponent>(mount.Rider, out var mountRider)
                && mountRider.Mount == uid
                && Spatial(uid).ParentUid == mount.Rider;
        }
    }
}