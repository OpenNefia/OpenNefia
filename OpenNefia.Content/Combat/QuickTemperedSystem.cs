using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.UI;
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

namespace OpenNefia.Content.Combat
{
    public interface IQuickTemperedSystem : IEntitySystem
    {
    }

    public sealed class QuickTemperedSystem : EntitySystem, IQuickTemperedSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;

        public override void Initialize()
        {
            SubscribeComponent<QuickTemperedComponent, EntityRefreshEvent>(HandleRefresh, priority: EventPriorities.Highest);
            SubscribeComponent<QuickTemperedComponent, EntityWoundedEvent>(HandleWounded, priority: EventPriorities.VeryHigh + 50000);
        }

        private void HandleRefresh(EntityUid uid, QuickTemperedComponent component, ref EntityRefreshEvent args)
        {
            component.IsQuickTempered.Reset();
        }

        private void HandleWounded(EntityUid uid, QuickTemperedComponent component, ref EntityWoundedEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1578 		if cBit(cTemper,tc):if dmgType!dmgSub:if cAngry( ...
            if (component.IsQuickTempered.Buffed)
            {
                if (!_effects.HasEffect(uid, Protos.StatusEffect.Fury))
                {
                    if (_rand.Prob(component.EnrageChance.Buffed))
                    {
                        _mes.Display(Loc.GetString("Elona.QuickTempered.IsEngulfedInFury", ("entity", uid)), UiColors.MesBlue);
                        _effects.SetTurns(uid, Protos.StatusEffect.Fury, _rand.Next(30) + 15);
                    }
                }
            } 
            // <<<<<<<< shade2/chara_func.hsp:1581 		} ..
        }
    }
}