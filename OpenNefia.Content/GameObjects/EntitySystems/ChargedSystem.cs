using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
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

namespace OpenNefia.Content.GameObjects
{
    public interface IChargedSystem : IEntitySystem
    {
        void ModifyCharges(EntityUid uid, int delta, ChargedComponent? charged = null);
        bool HasChargesRemaining(EntityUid uid, ChargedComponent? charged = null);
    }

    public sealed class ChargedSystem : EntitySystem, IChargedSystem
    {
        [Dependency] private readonly IRandom _rand = default!;

        public override void Initialize()
        {
            SubscribeComponent<ChargedComponent, ComponentInit>(InitCharges);
        }

        private void InitCharges(EntityUid uid, ChargedComponent component, ComponentInit args)
        {
            component.Charges = component.Charges - _rand.Next(component.Charges) + _rand.Next(component.Charges);
        }

        public void ModifyCharges(EntityUid uid, int delta, ChargedComponent? charged = null)
        {
            if (!Resolve(uid, ref charged))
                return;

            charged.Charges = Math.Clamp(charged.Charges + delta, 0, charged.MaxCharges);
        }

        public bool HasChargesRemaining(EntityUid uid, ChargedComponent? charged = null)
        {
            if (!Resolve(uid, ref charged))
                return false;

            return charged.Charges > 0;
        }
    }
}