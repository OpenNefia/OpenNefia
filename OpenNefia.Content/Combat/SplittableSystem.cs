﻿using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Combat
{
    public interface ISplittableSystem : IEntitySystem
    {
        void Split(EntityUid uid);
    }

    public sealed class SplittableSystem : EntitySystem, ISplittableSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IMapPlacement _placement = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;

        public override void Initialize()
        {
            SubscribeComponent<SplittableComponent, EntityRefreshEvent>(HandleRefresh, priority: EventPriorities.Highest);
            SubscribeComponent<SplittableComponent, EntityWoundedEvent>(HandleWounded, priority: EventPriorities.VeryHigh + 40000);
        }

        private void HandleRefresh(EntityUid uid, SplittableComponent component, ref EntityRefreshEvent args)
        {
            // TODO separate these event handlers that just reset stats from EntityRefreshEvent entirely
            // This is to not have to worry about priorities being set incorrectly, and it's a very common pattern.
            component.SplitsOnHighDamage.Reset();
            component.SplitOnHighDamageThreshold.Reset();
            component.SplitOnHighDamageChance.Reset();
            component.SplitsRandomlyWhenAttacked.Reset();
            component.SplitRandomlyWhenAttackedChance.Reset();
        }

        private readonly PrototypeId<StatusEffectPrototype>[] SplitBlockingEffects = new[]
        {
            Protos.StatusEffect.Confusion,
            Protos.StatusEffect.Dimming,
            Protos.StatusEffect.Poison,
            Protos.StatusEffect.Paralysis,
            Protos.StatusEffect.Blindness,
        };

        private void HandleWounded(EntityUid uid, SplittableComponent split, ref EntityWoundedEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1572 		if cBit(cSplit,tc):if dmgType!dmgSub:if (dmg>cMH ...
            if (split.SplitsOnHighDamage.Buffed && TryComp<SkillsComponent>(uid, out var skills))
            {
                var threshold = split.SplitOnHighDamageThreshold.Buffed;
                var chance = split.SplitOnHighDamageChance.Buffed;
                if (args.FinalDamage > skills.MaxHP / threshold || _rand.Prob(chance))
                {
                    Split(uid);
                }
            }
            // <<<<<<<< shade2/chara_func.hsp:1574 		} ..

            // >>>>>>>> shade2/chara_func.hsp:1575 		if cBit(cSplit2,tc):if dmgType!dmgSub:if rnd(3)= ...
            if (split.SplitsRandomlyWhenAttacked.Buffed)
            {
                if (_rand.Prob(split.SplitRandomlyWhenAttackedChance.Buffed))
                {
                    if (SplitBlockingEffects.All(e => !_effects.HasEffect(uid, e)))
                    {
                        Split(uid);
                    }
                }
            }
            // <<<<<<<< shade2/chara_func.hsp:1577 		} ..
        }

        public void Split(EntityUid uid)
        {
            if (TryMap(uid, out var map) && !HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
            {
                var freePos = _placement.FindFreePositionForChara(Spatial(uid).MapPosition);
                if (freePos != null && _stacks.Clone(uid, freePos.Value) != null)
                {
                    _mes.Display(Loc.GetString("Elona.Splittable.Splits", ("entity", uid)));
                }
            }
        }
    }
}