using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Game;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Enchantments;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.UI;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Hunger;
using OpenNefia.Core.Formulae;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Parties;
using OpenNefia.Content.BaseAnim;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Factions;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Chargeable;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.EtherDisease;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Currency;
using OpenNefia.Content.Sanity;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Items;
using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Core.Utility;
using OpenNefia.Content.InUse;

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
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IMapPlacement _mapPlacements = default!;
        [Dependency] private readonly IEquipmentGenSystem _equipmentGen = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IChargeableSystem _chargeables = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly INewEffectSystem _newEffects = default!;
        [Dependency] private readonly IFormulaEngine _formulas = default!;
        [Dependency] private readonly IVisibilitySystem _visibilities = default!;
        [Dependency] private readonly ICombatSystem _combat = default!;
        [Dependency] private readonly IEtherDiseaseSystem _etherDiseases = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly ISanitySystem _sanities = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IInUseSystem _inUses = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectTouchOfWeaknessComponent, ApplyEffectDamageEvent>(Apply_TouchOfWeakness);
            SubscribeComponent<EffectTouchOfHungerComponent, ApplyEffectDamageEvent>(Apply_TouchOfHunger);
            SubscribeComponent<EffectManisDisassemblyComponent, ApplyEffectDamageEvent>(Apply_ManisDisassembly);
            SubscribeComponent<EffectMirrorComponent, ApplyEffectDamageEvent>(Apply_Mirror);
            SubscribeComponent<EffectChangeComponent, ApplyEffectDamageEvent>(Apply_Change);
            SubscribeComponent<EffectDrawChargeComponent, ApplyEffectDamageEvent>(Apply_DrawCharge);
            SubscribeComponent<EffectRechargeComponent, ApplyEffectDamageEvent>(Apply_Recharge);
            SubscribeComponent<EffectMeleeAttackComponent, ApplyEffectDamageEvent>(Apply_MeleeAttack);
            SubscribeComponent<EffectEyeOfEtherComponent, ApplyEffectDamageEvent>(Apply_EyeOfEther);
            SubscribeComponent<EffectSuspiciousHandComponent, ApplyEffectDamageEvent>(Apply_SuspiciousHand, priority: EventPriorities.High - 1000); // placed before EffectDamageTeleport
            SubscribeComponent<EffectDamageSanityComponent, ApplyEffectDamageEvent>(Apply_DamageSanity);
            SubscribeComponent<EffectSuicideAttackComponent, ApplyEffectDamageEvent>(Apply_SuicideAttack);
            SubscribeComponent<EffectSuicideAttackComponent, ApplyEffectAreaEvent>(ApplyArea_SuicideAttack, priority: EventPriorities.VeryLow + 10000);
            SubscribeComponent<EffectInsultComponent, ApplyEffectDamageEvent>(Apply_Insult, priority: EventPriorities.High - 1000); // just applies EffectDamageStatusEffects afterwards
            SubscribeComponent<EffectDistantAttackComponent, ApplyEffectDamageEvent>(Apply_DistantAttack);
            SubscribeComponent<EffectScavengeComponent, ApplyEffectDamageEvent>(Apply_Scavenge);
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

        private void Apply_MeleeAttack(EntityUid uid, EffectMeleeAttackComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:3293 	call *anime,(animeId=aniAttack) ...
            if (args.Handled || !IsAlive(args.InnerTarget)
                || !_visibilities.HasLineOfSight(args.Source, args.InnerTarget.Value))
            {
                return;
            }

            var attackSkill = Protos.Skill.MartialArts;
            var meleeWeapons = _combat.GetMeleeWeapons(args.Source);
            if (meleeWeapons.Count > 0)
                attackSkill = Comp<WeaponComponent>(meleeWeapons[0]).WeaponSkill;

            var damagePercent = 0;
            if (TryComp<SkillsComponent>(args.InnerTarget.Value, out var skills))
                damagePercent = args.OutDamage / skills.MaxHP;

            var anim = _combat.GetMeleeAttackAnimation(args.InnerTarget.Value, attackSkill, damagePercent, isCritical: false);
            _mapDrawables.Enqueue(anim, args.InnerTarget.Value);

            _combat.MeleeAttack(args.Source, args.InnerTarget.Value);

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:3294 	call *act_melee ...
        }

        private void Apply_EyeOfEther(EntityUid uid, EffectEyeOfEtherComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:3376 	if tc!pc:swbreak ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            if (!_gameSession.IsPlayer(args.InnerTarget.Value) || !HasComp<EtherDiseaseComponent>(args.InnerTarget.Value))
                return;

            var etherDisease = EnsureComp<EtherDiseaseComponent>(args.InnerTarget.Value);
            _mes.Display(Loc.GetString("Elona.Effect.EyeOfEther.Apply", ("source", args.Source), ("target", args.InnerTarget.Value)), color: UiColors.MesPurple);

            var vars = _newEffects.GetEffectDamageFormulaArgs(uid, args);
            var corruption = (int)_formulas.Calculate(component.AddedCorruption, vars, 100);

            _etherDiseases.ModifyCorruption(args.InnerTarget.Value, corruption);

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:3378 	modCorrupt 100 ...
        }

        private void Apply_SuspiciousHand(EntityUid uid, EffectSuspiciousHandComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1929 	if efId=actSteal{  ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            // TODO this needs to cancel the subsequent EffectDamageTeleport handler
            // if it fails. However there is not yet a BeforeApplyEffectDamageEvent.
            // Move this code into its handler when that is created.
            if (args.Source == args.InnerTarget.Value)
            {
                _mes.Display(Loc.GetString("Elona.Effect.Teleport.Prevented"));
                args.Handle(TurnResult.Failed);
                return;
            }

            var goldStolen = args.OutDamage;

            if (_rand.Next(_skills.Level(args.InnerTarget.Value, Protos.Skill.AttrPerception)) > _rand.Next(_skills.Level(args.Source, Protos.Skill.AttrPerception) * 4)
                || (TryComp<CommonProtectionsComponent>(args.InnerTarget.Value, out var prot) && prot.IsProtectedFromTheft.Buffed))
            {
                goldStolen = 0;
            }

            if (goldStolen > 0 && TryComp<MoneyComponent>(args.InnerTarget.Value, out var money))
            {
                _audio.Play(Protos.Sound.Paygold1, args.InnerTarget.Value);
                money.Gold = int.Max(money.Gold - goldStolen, 0);
                _mes.Display(Loc.GetString("Elona.Effect.SuspiciousHand.Steals", ("source", args.Source), ("target", args.InnerTarget.Value)));
                if (TryComp<MoneyComponent>(args.Source, out var thiefMoney))
                    thiefMoney.Gold += goldStolen;
            }

            // TODO Don't handle yet (fix later), EffectDamageTeleport will be handled

            // <<<<<<<< elona122/shade2/proc.hsp:1945 		} ...
        }

        private void Apply_DamageSanity(EntityUid uid, EffectDamageSanityComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:2027 	case actGazeInsane ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            _sanities.DamageSanity(args.InnerTarget.Value, args.OutDamage);

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:2032 	swbreak ...
        }

        private void Apply_SuicideAttack(EntityUid uid, EffectSuicideAttackComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            if (TryComp<ExplosiveComponent>(args.InnerTarget.Value, out var explosive)
                && explosive.IsExplosive.Buffed)
                component.ChainBombTargets.Add(args.InnerTarget.Value);

            args.Handle(TurnResult.Succeeded);
        }

        private void ApplyArea_SuicideAttack(EntityUid effect, EffectSuicideAttackComponent component, ApplyEffectAreaEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1796 	if efId=actSuicide{ ...
            // XXX: no Handled checking here
            // maybe "has turn result set" should not be overloaded to also mean "skip remaining handlers"

            var targets = component.ChainBombTargets.ToList();
            component.ChainBombTargets.Clear();

            if (IsAlive(args.Source))
            {
                if (TryComp<SkillsComponent>(args.Source, out var skills))
                    _damages.DamageHP(args.Source, int.Max(99999, skills.MaxHP));
                else
                    EntityManager.DeleteEntity(args.Source);
            }

            foreach (var target in targets)
            {
                if (!IsAlive(target))
                    continue;

                // Explode again.
                _newEffects.Apply(target, target, null, effect, args: args.Args);
            }
            
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1806 		} ...
        }

        private void Apply_Insult(EntityUid uid, EffectInsultComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:3383 	if efId=actInsult{ ...
            if (args.Handled || !IsAlive(args.InnerTarget) || _visibilities.IsInWindowFov(args.InnerTarget.Value))
                return;

            _mes.Display(Loc.GetString("Elona.Effect.Insult.Apply", ("source", args.Source), ("target", args.InnerTarget.Value)));

            var localeArgs = new LocaleArg[]
            {
                ("source", args.Source),
                ("target", args.InnerTarget.Value)
            };

            var gender = CompOrNull<CharaComponent>(args.Source)?.Gender ?? Gender.Male;
            if (!Loc.TryGetString($"Elona.Effect.Insult.Insults.{gender}", out var insult, localeArgs))
                insult = Loc.GetString("Elona.Effect.Insult.Insults.Male", localeArgs);

            _mes.Display(insult, color: UiColors.MesSkyBlue, entity: args.Source);

            // TODO fallthrough
            // <<<<<<<< elona122/shade2/proc.hsp:3397 		} ...
        }

        private void Apply_DistantAttack(EntityUid uid, EffectDistantAttackComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1693 	case skAttack ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            var anim = new RangedAttackMapDrawable(args.SourceCoordsMap,
                Spatial(args.InnerTarget.Value).MapPosition, 
                Protos.Chip.ItemProjectileSpore,
                sound: Protos.Sound.Bow1);
            _mapDrawables.Enqueue(anim, args.SourceCoordsMap);

            _combat.MeleeAttack(args.Source, args.InnerTarget.Value);

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1696 	swbreak ...
        }

        private void Apply_Scavenge(EntityUid uid, EffectScavengeComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            _mes.Display(Loc.GetString("Elona.Effect.Scavenge.Apply", ("source", args.Source), ("target", args.InnerTarget.Value)));

            bool CanScavenge(EntityUid item)
            {
                if (!IsAlive(item))
                    return false;

                if (TryComp<ItemComponent>(item, out var itemComp) && itemComp.IsPrecious)
                    return false;

                return HasComp<FoodComponent>(item);
            }

            var candidates = _inv.EnumerateInventory(args.InnerTarget.Value).Where(CanScavenge).ToList();
            _rand.Shuffle(candidates);

            candidates.Sort((a, b) =>
            {
                var isFishA = Comp<FoodComponent>(a).FoodType == Protos.FoodType.Fish ? 1 : 0;
                var isFishB = Comp<FoodComponent>(b).FoodType == Protos.FoodType.Fish ? 1 : 0;

                return isFishB - isFishA;
            });

            if (!candidates.TryFirstOrNull(out var item))
            {
                args.Handle(TurnResult.Failed);
                return;
            }

            if (HasComp<LovePotionSpikedComponent>(item.Value))
            {
                _mes.Display(Loc.GetString("Elona.Effect.Scavenge.Spiked", ("source", args.Source), ("target", args.InnerTarget.Value), ("item", item.Value)));
                args.Handle(TurnResult.Failed);
                return;
            }

            _inUses.RemoveUserOfItem
        }
    }
}