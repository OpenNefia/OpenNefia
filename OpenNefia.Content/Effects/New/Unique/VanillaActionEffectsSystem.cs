using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Spells;
using OpenNefia.Core;
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
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects.New;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Enchantments;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.UI;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.Formulae;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Parties;
using OpenNefia.Content.BaseAnim;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Mount;
using OpenNefia.Content.Factions;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Chargeable;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Book;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Effects.New.Unique
{
    public sealed class VanillaActionEffectsSystem : EntitySystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IVanillaEnchantmentsSystem _vanillaEnchantments = default!;
        [Dependency] private readonly IQualitySystem _qualities = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ISkillAdjustsSystem _skillAdjusts = default!;
        [Dependency] private readonly IRefreshSystem _refreshes = default!;
        [Dependency] private readonly IHungerSystem _hungers = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IRoleSystem _roles = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IMountSystem _mounts = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IMapPlacement _mapPlacements = default!;
        [Dependency] private readonly IEquipmentGenSystem _equipmentGen = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IChargeableSystem _chargeables = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly INewEffectSystem _newEffects = default!;
        [Dependency] private readonly IFormulaEngine _formulas = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectTouchOfWeaknessComponent, ApplyEffectDamageEvent>(Apply_TouchOfWeakness);
            SubscribeComponent<EffectTouchOfHungerComponent, ApplyEffectDamageEvent>(Apply_TouchOfHunger);
            SubscribeComponent<EffectManisDisassemblyComponent, ApplyEffectDamageEvent>(Apply_ManisDisassembly);
            SubscribeComponent<EffectMirrorComponent, ApplyEffectDamageEvent>(Apply_Mirror);
            SubscribeComponent<EffectChangeComponent, ApplyEffectDamageEvent>(Apply_Change);
            SubscribeComponent<EffectDrawChargeComponent, ApplyEffectDamageEvent>(Apply_DrawCharge);
            SubscribeComponent<EffectRechargeComponent, ApplyEffectDamageEvent>(Apply_Recharge);
        }

        private void Apply_TouchOfWeakness(EntityUid uid, EffectTouchOfWeaknessComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1865 	if efId=actTouchWeaken{ ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            var attrID = _skills.PickRandomAttribute().GetStrongID();

            var proceed = true;

            if (_vanillaEnchantments.HasSustainEnchantment(args.InnerTarget.Value, attrID))
                proceed = false;

            if (_qualities.GetQuality(args.InnerTarget.Value) >= Quality.Great && _rand.OneIn(4))
                proceed = false;

            if (proceed)
            {
                var adjustment = _skillAdjusts.GetSkillAdjust(args.InnerTarget.Value, attrID);
                var diff = _skills.BaseLevel(args.InnerTarget.Value, attrID) - adjustment;
                if (diff > 0)
                {
                    diff = diff * args.CommonArgs.Power / 2000 + 1;
                    _skillAdjusts.SetSkillAdjust(args.InnerTarget.Value, attrID, adjustment - diff);
                }
                _mes.Display(Loc.GetString("Elona.Effect.TouchOfWeakness.Apply", ("source", args.Source), ("target", args.InnerTarget.Value)), color: UiColors.MesPurple, entity: args.InnerTarget.Value);
                _refreshes.Refresh(args.InnerTarget.Value);
            }

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1877 		} ...
        }

        private void Apply_TouchOfHunger(EntityUid uid, EffectTouchOfHungerComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1859 	if efId=actTouchHunger{ ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            if (TryComp<HungerComponent>(args.InnerTarget.Value, out var hunger))
            {
                hunger.Nutrition -= HungerSystem.HungerDecrementAmount * 100;
                _mes.Display(Loc.GetString("Elona.Effect.TouchOfHunger.Apply", ("source", args.Source), ("target", args.InnerTarget.Value)), color: UiColors.MesPurple, entity: args.InnerTarget.Value);
                _hungers.MakeHungry(args.InnerTarget.Value, hunger);
            }

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1863 		} ...
        }

        private void Apply_ManisDisassembly(EntityUid uid, EffectManisDisassemblyComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1843 	if efId=actDisassemble{ ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            if (args.AffectedTileIndex == 0)
                _mes.Display(Loc.GetString("Elona.Effect.ManisDisassembly.Dialog", ("source", args.Source), ("target", args.InnerTarget.Value)), entity: args.Source);

            if (TryComp<SkillsComponent>(args.InnerTarget.Value, out var skills))
            {
                skills.HP = skills.MaxHP / 12 + 1;
            }

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1847 		} ...
        }

        private void Apply_Mirror(EntityUid uid, EffectMirrorComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:2121 	case actKnowSelf ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            _mes.Display(Loc.GetString("Elona.Effect.Mirror.Examine", ("source", args.Source), ("target", args.InnerTarget.Value)));

            var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSparkle);
            _mapDrawables.Enqueue(anim, args.InnerTarget.Value);

            _mes.Display("TODO", color: UiColors.MesYellow);

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:2125 	swbreak ...
        }

        private bool CanChangeCreature(EntityUid target)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:3168 	if tc=pc:txtNothingHappen:obvious=false:swbreak ...
            if (_gameSession.IsPlayer(target))
                return false;

            // I expect a large number of bugs surrounding these conditions.
            // Since the entity's components are wiped and replaced in-place, a
            // lot of temporary state gets reset.
            // So these checks are a matter of stopping short if some of that
            // missing state could cause problems (example: MountRider, Party)
            if (_qualities.GetQuality(target) >= Quality.Great
                || _roles.HasAnyRoles(target)
                || _parties.IsInSomeParty(target)
                || HasComp<TemporaryAllyComponent>(target)
                || HasComp<LivestockComponent>(target) // Elona+
                || (TryComp<CharaComponent>(target, out var chara) && chara.IsPrecious))
                return false;

            return true;
            // <<<<<<<< elona122/shade2/proc.hsp:3172 	if tc<maxSaveChara:f=false ...
        }

        private void Apply_Change(EntityUid uid, EffectChangeComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:3166 	case actChangeCreature ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            if (_gameSession.IsPlayer(args.InnerTarget.Value))
                return;

            if (!CanChangeCreature(args.InnerTarget.Value))
            {
                args.Handle(TurnResult.Failed);
                _mes.Display(Loc.GetString("Elona.Effect.Change.CannotBeChanged", ("source", args.Source), ("target", args.InnerTarget.Value)));
                return;
            }

            if (args.OutDamage < _levels.GetLevel(args.InnerTarget.Value))
            {
                args.Handle(TurnResult.Failed);
                _mes.Display(Loc.GetString("Elona.Effect.Common.Resists", ("target", args.InnerTarget.Value)));
                return;
            }

            var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSmoke);
            _mapDrawables.Enqueue(anim, args.InnerTarget.Value);

            _mes.Display(Loc.GetString("Elona.Effect.Change.Changes", ("source", args.Source), ("target", args.InnerTarget.Value)));

            // NOTE: This implementation isn't equivalent to vanilla's yet, but it should be just
            // enough for most people

            // TODO does not reuse the EntityUid like vanilla (creates a new entity instead)
            // TODO does not preserve original entity's inventory/equipment
            // TODO does not account for mounts/riders
            // TODO other systems may want to hook into this to preserve their own state

            var oldRelation = CompOrNull<FactionComponent>(args.InnerTarget.Value)?.RelationToPlayer;
            var oldTarget = CompOrNull<VanillaAIComponent>(args.InnerTarget.Value)?.CurrentTarget;
            var oldAggro = CompOrNull<VanillaAIComponent>(args.InnerTarget.Value)?.Aggro;
            var oldPosition = Spatial(args.InnerTarget.Value).MapPosition;

            var filter = new CharaFilter()
            {
                MinLevel = _randomGen.CalcObjectLevel(_levels.GetLevel(args.InnerTarget.Value) + 3),
                Quality = Quality.Normal
            };
            var newEntity = _charaGen.GenerateChara(MapCoordinates.Global, filter);

            if (!IsAlive(newEntity))
            {
                if (newEntity != null)
                    EntityManager.DeleteEntity(newEntity.Value);
                args.Handle(TurnResult.Failed);
                return;
            }

            EntityManager.DeleteEntity(args.InnerTarget.Value);

            if (!_mapPlacements.TryPlaceChara(newEntity.Value, oldPosition))
            {
                EntityManager.DeleteEntity(newEntity.Value);
                args.Handle(TurnResult.Failed);
                return;
            }

            // >>>>>>>> elona122/shade2/chara_func.hsp:350 	p1=cRelation(tc):p2=cAiAggro(tc):p3=cExist(tc):hp ...
            if (oldRelation != null && TryComp<FactionComponent>(newEntity.Value, out var faction))
                faction.RelationToPlayer = oldRelation.Value;
            if (oldTarget != null && TryComp<VanillaAIComponent>(newEntity.Value, out var ai))
            {
                ai.CurrentTarget = oldTarget;
                ai.Aggro = oldAggro ?? 0;
            }
            // <<<<<<<< elona122/shade2/chara_func.hsp:350 	p1=cRelation(tc):p2=cAiAggro(tc):p3=cExist(tc):hp ...

            // >>>>>>>> elona122/shade2/chara_func.hsp:409 	rc@=tc:gosub *chara_equipFull@ ...
            _equipmentGen.GenerateAndEquipEquipment(newEntity.Value);
            _refreshes.Refresh(newEntity.Value);
            // <<<<<<<< elona122/shade2/chara_func.hsp:411 	call *charaRefresh@,(r1@=tc) ...

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:3184 		} ...
        }

        private void Apply_DrawCharge(EntityUid uid, EffectDrawChargeComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:3145 	case actAbsorbCharge ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            if (args.AffectedTileIndex > 0)
                return;

            var context = new InventoryContext(args.InnerTarget.Value,
                new MatchAnyComponentInventoryBehavior(new Type[] { typeof(ChargeableComponent) }));
            var result = _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);

            if (!result.HasValue || !IsAlive(result.Value.SelectedItem))
            {
                args.Handle(TurnResult.Failed);
                return;
            }

            if (!_stacks.TrySplit(result.Value.SelectedItem.Value, 1, out var split)
                || !TryComp<ChargeableComponent>(split, out var chargeable))
            {
                return;
            }

            _chargeables.DrawCharge(args.Source, split, chargeable);

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:3164 	swbreak ...
        }

        private void Apply_Recharge(EntityUid uid, EffectRechargeComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:3104 	case actPutCharge ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            if (args.AffectedTileIndex > 0)
                return;

            var context = new InventoryContext(args.InnerTarget.Value,
                new MatchAnyComponentInventoryBehavior(new Type[] { typeof(ChargeableComponent) }));
            var result = _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);

            if (!result.HasValue || !IsAlive(result.Value.SelectedItem))
            {
                args.Handle(TurnResult.Failed);
                return;
            }

            if (!_stacks.TrySplit(result.Value.SelectedItem.Value, 1, out var split)
                || !TryComp<ChargeableComponent>(split, out var chargeable))
            {
                return;
            }

            var vars = _newEffects.GetEffectDamageFormulaArgs(uid, args);
            vars["charges"] = chargeable.Charges;
            vars["maxCharges"] = chargeable.MaxCharges;
            var addedCharges = (int)_formulas.Calculate(component.AddedCharges, vars, 1);

            if (_chargeables.TryRecharge(args.Source, chargeable.Owner, addedCharges, component.RechargePowerCost, args.OutDamage, chargeable))
            {
                args.Handle(TurnResult.Succeeded);
            }
            else
            {
                args.Handle(TurnResult.Failed);
            }
            // <<<<<<<< elona122/shade2/proc.hsp:3143 	swbreak ...
        }
    }
}