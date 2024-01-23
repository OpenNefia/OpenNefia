using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Core.Formulae;
using OpenNefia.Content.BaseAnim;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Book;

namespace OpenNefia.Content.Chargeable
{
    public interface IChargeableSystem : IEntitySystem
    {
        void SetCharges(EntityUid uid, int charges, ChargeableComponent? chargeable = null);
        void ModifyCharges(EntityUid uid, int delta, ChargeableComponent? chargeable = null);
        bool HasChargesRemaining(EntityUid uid, ChargeableComponent? chargeable = null);
        int CalcAbsorbableCharges(EntityUid owner, ChargeableComponent? chargeable = null);
        bool TryRecharge(EntityUid source, EntityUid item, int addedCharges, int baseCost, int power, ChargeableComponent? chargeable = null);
        void DrawCharge(EntityUid source, EntityUid item, ChargeableComponent? chargeable = null);
    }

    public sealed class ChargeableSystem : EntitySystem, IChargeableSystem
    {
        [Dependency] private readonly IFormulaEngine _formulas = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;

        public override void Initialize()
        {
            SubscribeComponent<ChargeableComponent, ComponentInit>(InitCharges);
            SubscribeComponent<SpellbookComponent, CalcRechargeAddedChargesEvent>(CalcAddedCharges_Spellbook);
        }

        private void InitCharges(EntityUid uid, ChargeableComponent component, ComponentInit args)
        {
            var vars = new Dictionary<string, double>();
            vars["maxCharges"] = component.MaxCharges;
            component.Charges = (int)_formulas.Calculate(component.InitialCharges, vars, component.MaxCharges);
        }

        public void SetCharges(EntityUid uid, int charges, ChargeableComponent? charged = null)
        {
            if (!Resolve(uid, ref charged))
                return;

            charged.Charges = Math.Clamp(charges, 0, charged.MaxCharges);
        }

        public void ModifyCharges(EntityUid uid, int delta, ChargeableComponent? charged = null)
        {
            if (!Resolve(uid, ref charged))
                return;

            SetCharges(uid, charged.Charges + delta, charged);
        }

        public bool HasChargesRemaining(EntityUid uid, ChargeableComponent? charged = null)
        {
            if (!Resolve(uid, ref charged))
                return false;

            return charged.Charges > 0;
        }

        public void DrawCharge(EntityUid source, EntityUid item, ChargeableComponent? chargeable = null)
        {
            if (!Resolve(item, ref chargeable))
                return;

            var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSmoke);
            _mapDrawables.Enqueue(anim, chargeable.Owner);

            var chargesAbsorbed = CalcAbsorbableCharges(chargeable.Owner, chargeable);

            var absorbed = EnsureComp<AbsorbedChargesComponent>(source);
            absorbed.RechargePower += chargesAbsorbed;

            _mes.Display(Loc.GetString("Elona.Chargeable.DrawCharge.Extract",
                ("source", source),
                ("item", item),
                ("chargesAbsorbed", chargesAbsorbed),
                ("totalCharges", absorbed.RechargePower)));

            EntityManager.DeleteEntity(item);
        }

        public int CalcAbsorbableCharges(EntityUid item, ChargeableComponent? chargeable = null)
        {
            if (!Resolve(item, ref chargeable))
                return 0;

            // >>>>>>>> elona122/shade2/proc.hsp:3151 		repeat 1 ...
            var maxCharges = chargeable.MaxCharges;
            var absorbedPerCharge = maxCharges switch
            {
                _ when maxCharges == 1 => 100,
                _ when maxCharges == 2 => 25,
                _ when maxCharges <= 4 => 5,
                _ when maxCharges <= 6 => 3,
                _ => 1
            };

            return chargeable.Charges * absorbedPerCharge * _stacks.GetCount(item);
            // <<<<<<<< elona122/shade2/proc.hsp:3157 		loop ...
        }

        private bool ProcRechargeSuccess(ChargeableComponent chargeable, int power)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:3127 		if rnd(efP/25+1)=0:f=false ...
            if (power <= 0)
                return false;

            if (HasComp<SpellbookComponent>(chargeable.Owner) && _rand.OneIn(4))
                return false;

            if (_rand.OneIn(chargeable.MaxCharges * chargeable.MaxCharges + 1))
                return false;

            return true;
            // <<<<<<<< elona122/shade2/proc.hsp:3129 		if rnd(iChargeLevel*iChargeLevel+1)=0:f=false ...
        }

        private void CalcAddedCharges_Spellbook(EntityUid uid, SpellbookComponent component, CalcRechargeAddedChargesEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:3132 			if iType(ci)=fltSpellBook:p=1 ...
            args.OutAddedCharges = 1;
            // <<<<<<<< elona122/shade2/proc.hsp:3132 			if iType(ci)=fltSpellBook:p=1 ...
        }

        public bool TryRecharge(EntityUid source, EntityUid item, int addedCharges, int baseCost, int power, ChargeableComponent? chargeable = null)
        {
            if (!Resolve(item, ref chargeable) || chargeable.Charges > chargeable.MaxCharges)
            {
                _mes.Display(Loc.GetString("Elona.Chargeable.Recharge.Errors.CannotRechargeAnymore",
                    ("source", source),
                    ("item", item)));
                return false;
            }

            if (!chargeable.CanBeRecharged)
            {
                _mes.Display(Loc.GetString("Elona.Effect.Recharge.Errors.CannotRecharge",
                    ("source", source),
                    ("item", chargeable.Owner)));
                return false;
            }

            var cost = baseCost + chargeable.RechargeCost;

            if (cost > 0)
            {
                // >>>>>>>> elona122/shade2/proc.hsp:3107 	if efId=actPutCharge{ ...
                if (!TryComp<AbsorbedChargesComponent>(source, out var absorbed)
                    || absorbed.RechargePower < cost)
                {
                    _mes.Display(Loc.GetString("Elona.Chargeable.Recharge.Errors.NotEnoughPower",
                        ("source", source),
                        ("item", chargeable.Owner),
                        ("chargesNeeded", cost),
                        ("totalCharges", absorbed?.RechargePower ?? 0)));
                    return false;
                }
                else
                {
                    absorbed.RechargePower -= cost;
                    _mes.Display(Loc.GetString("Elona.Chargeable.Recharge.SpendPower",
                        ("source", source),
                        ("item", chargeable.Owner),
                        ("chargesNeeded", cost),
                        ("totalCharges", absorbed?.RechargePower)));
                }
                // <<<<<<<< elona122/shade2/proc.hsp:3114 		} ...
            }

            var success = ProcRechargeSuccess(chargeable, power);

            if (success)
            {
                var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSmoke);
                _mapDrawables.Enqueue(anim, chargeable.Owner);

                var ev = new CalcRechargeAddedChargesEvent(addedCharges);
                RaiseEvent(chargeable.Owner, ev);

                addedCharges = int.Max(ev.OutAddedCharges, 1);

                _mes.Display(Loc.GetString("Elona.Chargeable.Recharge.Success",
                    ("source", source),
                    ("item", chargeable.Owner),
                    ("amount", addedCharges)));
                chargeable.Charges += addedCharges;

                return true;
            }
            else
            {
                if (_rand.OneIn(4))
                {
                    _mes.Display(Loc.GetString("Elona.Chargeable.Recharge.Failure.Explodes",
                        ("source", source),
                        ("item", chargeable.Owner)));

                    _stacks.Use(chargeable.Owner, 1);
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Chargeable.Recharge.Failure.FailToRecharge",
                        ("source", source),
                        ("item", chargeable.Owner)));
                }
                return false;
            }
        }
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class CalcRechargeAddedChargesEvent
    {
        public int OutAddedCharges { get; set; }

        public CalcRechargeAddedChargesEvent(int outAddedCharges)
        {
            OutAddedCharges = outAddedCharges;
        }
    }
}