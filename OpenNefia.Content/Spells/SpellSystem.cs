using OpenNefia.Content.Activity;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Effects.New;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Spells;
using OpenNefia.Content.UI;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.Mount;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Parties;
using OpenNefia.Core.Configuration;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Actions;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Book;
using OpenNefia.Core;
using OpenNefia.Content.Buffs;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.Factions;

namespace OpenNefia.Content.Spells
{
    // TODO Spell -> Spells?
    public interface ISpellSystem : IEntitySystem
    {
        #region Querying

        int Level(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null);
        int Level(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null);
        int BaseLevel(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null);
        int BaseLevel(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null);
        int Potential(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null);
        int Potential(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null);
        int SpellStock(EntityUid uid, PrototypeId<SpellPrototype> spellId, SpellsComponent? spells = null);
        int SpellStock(EntityUid uid, SpellPrototype spellProto, SpellsComponent? spells = null);

        bool TryGetKnown(EntityUid uid, PrototypeId<SpellPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level, [NotNullWhen(true)] out SpellState? spellState, SpellsComponent? spells = null);

        bool HasSpell(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null);
        bool HasSpell(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null);

        #endregion

        #region Leveling

        void ModifyPotential(EntityUid uid, PrototypeId<SpellPrototype> spellID, int delta, SpellsComponent? spells = null);
        void ModifyPotential(EntityUid uid, SpellPrototype skillProto, int delta, SpellsComponent? spells = null);
        void GainFixedSpellExp(EntityUid uid, PrototypeId<SpellPrototype> spellID, int expGained, SpellsComponent? spells = null);
        void GainSpellExp(EntityUid uid, PrototypeId<SpellPrototype> spellID, int baseExpGained, int relatedSkillExpDivisor = 0, int levelExpDivisor = 0, SpellsComponent? spells = null);
        void GainSpellExp(EntityUid uid, SpellPrototype skill, int baseExpGained, int relatedSkillExpDivisor = 0, int levelExpDivisor = 0, SpellsComponent? spells = null);
        void GainSpell(EntityUid uid, PrototypeId<SpellPrototype> spellID, int spellStock = 0, LevelAndPotential? initialValues = null, SpellsComponent? spells = null);

        #endregion

        #region Casting

        /// <summary>
        /// Makes this entity cast a spell. You shouldn't use this if you're
        /// trying to apply a spell's effect from an item; instead use
        /// <see cref="INewEffectSystem"/>.
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="spell"></param>
        /// <param name="alwaysUseMP">
        /// Normally the AI will never expend MP when casting spells. Set this flag to true
        /// to override this.
        /// </param>
        /// <returns></returns>
        TurnResult NewCast(EntityUid caster, PrototypeId<SpellPrototype> spell, bool alwaysUseMP = false, SpellsComponent? spells = null);

        TurnResult Cast(PrototypeId<SpellPrototype> spellID, EntityUid target, int power = 0,
            EntityUid? source = null, EntityUid? item = null,
            EntityCoordinates? coords = null, CurseState? curseState = null,
            string effectSource = EffectSources.Default, EffectArgSet? args = null);

        string LocalizeSpellDescription(SpellPrototype proto, EntityUid caster, EntityUid effect);
        float CalcSpellSuccessRate(SpellPrototype proto, EntityUid caster, EntityUid effect);
        int CalcBaseSpellMPCost(SpellPrototype proto, EntityUid caster, EntityUid effect);
        int CalcBaseSpellStockCost(SpellPrototype proto, EntityUid caster, EntityUid effect);
        int CalcRandomizedSpellStockCost(SpellPrototype proto, EntityUid caster, EntityUid effect);
        int CalcCastSpellPower(SpellPrototype spell, EntityUid caster);

        #endregion
    }

    public sealed partial class SpellSystem : EntitySystem, ISpellSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly INewEffectSystem _newEffects = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IMountSystem _mounts = default!;
        [Dependency] private readonly ICombatSystem _combat = default!;
        [Dependency] private readonly IEquipmentSystem _equip = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IConfigurationManager _configuration = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ISpellbookSystem _spellbooks = default!;
        [Dependency] private readonly IBuffSystem _buffs = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly IEnchantmentSystem _enchantments = default!;
        [Dependency] private readonly ITargetingSystem _targetings = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;

        public override void Initialize()
        {
            SubscribeEntity<BeforeSpellEffectInvokedEvent>(BeforeSpellEffect_ProcVanillaCastingEvents);
        }

        #region Querying

        public bool TryGetKnown(EntityUid uid, PrototypeId<SpellPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level, [NotNullWhen(true)] out SpellState? spellState, SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells)
                || !_protos.TryIndex(protoId, out var spell)
                || !_skills.TryGetKnown(uid, spell.SkillID, out level))
            {
                level = null;
                spellState = null;
                return false;
            }

            spellState = spells.Ensure(protoId);
            return true;
        }

        public int Level(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null)
            => Level(uid, proto.GetStrongID(), spells);

        public int Level(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null)
        {
            if (!TryGetKnown(uid, protoId, out var level, out _))
                return 0;

            return level.Level.Buffed;
        }

        public int BaseLevel(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null)
            => BaseLevel(uid, proto.GetStrongID(), spells);

        public int BaseLevel(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null)
        {
            if (!TryGetKnown(uid, protoId, out var level, out _))
                return 0;

            return level.Level.Base;
        }

        public int Potential(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null)
            => Potential(uid, proto.GetStrongID(), spells);

        public int Potential(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null)
        {
            if (!TryGetKnown(uid, protoId, out var level, out _))
                return 0;

            return level.Potential;
        }

        public bool HasSpell(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null)
            => HasSpell(uid, proto.GetStrongID(), spells);

        public bool HasSpell(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null)
            => TryGetKnown(uid, protoId, out _, out _, spells);

        public int SpellStock(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null)
            => SpellStock(uid, proto.GetStrongID(), spells);
        public int SpellStock(EntityUid uid, PrototypeId<SpellPrototype> spellID, SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells) || !HasSpell(uid, spellID))
                return 0;

            return spells.Spells.GetOrInsertNew(spellID).SpellStock;
        }

        #endregion

        #region Leveling

        public void ModifyPotential(EntityUid uid, PrototypeId<SpellPrototype> spellID, int delta, SpellsComponent? spells = null)
        {
            if (delta == 0 || !Resolve(uid, ref spells)
                || !HasSpell(uid, spellID)
                || !_protos.TryIndex(spellID, out var spell))
                return;

            _skills.ModifyPotential(uid, spell.SkillID, delta);
        }

        public void ModifyPotential(EntityUid uid, SpellPrototype skillProto, int delta, SpellsComponent? spells = null)
            => ModifyPotential(uid, skillProto.GetStrongID(), delta, spells);

        public void GainFixedSpellExp(EntityUid uid, PrototypeId<SpellPrototype> spellID, int expGained,
            SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells) || !HasSpell(uid, spellID)
                || !_protos.TryIndex(spellID, out var spellProto))
                return;

            _skills.GainFixedSkillExp(uid, spellProto.SkillID, expGained);
        }

        public void GainSpellExp(EntityUid uid, PrototypeId<SpellPrototype> spellID,
            int baseExpGained,
            int relatedSkillExpDivisor,
            int levelExpDivisor = 0,
            SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells) || !HasSpell(uid, spellID))
                return;

            if (!_protos.TryIndex(spellID, out var spellProto))
            {
                Logger.WarningS("spell", $"No spell with ID {spellID} found.");
                return;
            }

            _skills.GainSkillExp(uid, spellProto.SkillID, baseExpGained, relatedSkillExpDivisor, levelExpDivisor);
        }

        public void GainSpellExp(EntityUid uid, SpellPrototype spell,
            int baseExpGained,
            int relatedSkillExpDivisor,
            int levelExpDivisor = 0,
            SpellsComponent? spells = null)
            => GainSpellExp(uid, spell.GetStrongID(), baseExpGained, relatedSkillExpDivisor, levelExpDivisor, spells);

        public const int DefaultSpellPotential = 200;

        public void GainSpell(EntityUid uid, PrototypeId<SpellPrototype> spellID, int spellStock = 0, LevelAndPotential? initialValues = null, SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells) || !_protos.TryIndex(spellID, out var spellProto)
                || !TryComp<SkillsComponent>(uid, out var skills))
                return;

            if (TryGetKnown(uid, spellID, out _, out var spellState))
            {
                if (_gameSession.IsPlayer(uid))
                {
                    spellState.SpellStock += spellStock;
                    ModifyPotential(uid, spellID, 1);
                }
                return;
            }

            var newSkill = new LevelAndPotential()
            {
                Level = initialValues?.Level ?? new(1),
                Potential = initialValues?.Potential ?? 0,
                Experience = initialValues?.Experience ?? 0,
            };
            newSkill.Level.Base = Math.Max(newSkill.Level.Base, 1);

            skills.Skills[spellProto.SkillID] = newSkill;
            spells.Spells[spellID] = new SpellState() { SpellStock = spellStock };

            if (newSkill.Potential <= 0)
            {
                ModifyPotential(uid, spellID, DefaultSpellPotential);
            }

            _refresh.Refresh(uid);
        }

        /// <inheritdoc/>
        public TurnResult NewCast(EntityUid caster, PrototypeId<SpellPrototype> spellID, bool alwaysUseMP = false, SpellsComponent? spells = null)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1272 *cast ...
            var result = DoCastSpell(caster, spellID, alwaysUseMP);
            if (result == TurnResult.Succeeded)
            {
                GainSpellAndCastingExp(caster, spellID);
            }
            return result;
            // <<<<<<<< elona122/shade2/proc.hsp:1280 	return false	 ...
        }

        private void GainSpellAndCastingExp(EntityUid caster, PrototypeId<SpellPrototype> spellID)
        {
            var spell = _protos.Index(spellID);

            if (_gameSession.IsPlayer(caster))
            {
                _skills.GainSkillExp(caster, spell.SkillID, spell.MPCost * 4 + 20, 4, 5);
            }

            _skills.GainSkillExp(caster, Protos.Skill.Casting, spell.MPCost + 10, 5);
        }

        private TurnResult DoCastSpell(EntityUid caster, PrototypeId<SpellPrototype> spellID, bool alwaysUseMP = false, SpellsComponent? spells = null)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1282 *cast_proc ..
            if (!_protos.TryIndex(spellID, out var spell)
                || !TryComp<SkillsComponent>(caster, out var skills)
                || !Resolve(caster, ref spells))
                return TurnResult.Aborted;

            if (!_newEffects.TrySpawnEffect(spell.EffectID, out var effect))
                return TurnResult.Aborted;

            var isPlayer = _gameSession.IsPlayer(caster);
            if (isPlayer)
            {
                var estMPCost = CalcBaseSpellMPCost(spell, caster, effect.Value);
                if (estMPCost > skills.MP && _configuration.GetCVar(CCVars.GameWarnOnSpellOvercast))
                {
                    if (!_playerQuery.YesOrNo(Loc.GetString("Elona.Spells.Prompt.OvercastWarning")))
                        return TurnResult.Aborted;
                }
            }

            var power = CalcCastSpellPower(spell, caster);

            var commonArgs = new EffectCommonArgs()
            {
                EffectSource = EffectSources.Spell,
                Power = power,
                CurseState = CurseState.Normal,
                SkillLevel = _skills.Level(caster, spell.SkillID),
                TileRange = spell.MaxRange
            };
            var args = EffectArgSet.Make(commonArgs);

            if (!_newEffects.TryGetEffectTarget(caster, effect.Value, args, out var targetPair))
                return TurnResult.Aborted;

            var useMP = isPlayer || alwaysUseMP;
            if (useMP)
            {
                // NOTE: Spell stock only applies to the player.
                if (isPlayer)
                {
                    var stock = spells.Spells.GetOrInsertNew(spellID);
                    stock.SpellStock = int.Max(stock.SpellStock - CalcRandomizedSpellStockCost(spell, caster, effect.Value), 0);
                }

                var mpUsed = CalcBaseSpellMPCost(spell, caster, effect.Value);
                _damages.DamageMP(caster, mpUsed, showMessage: false);

                // Check magic reaction damage.
                if (!IsAlive(caster))
                    return TurnResult.Failed;
            }

            var ev = new BeforeSpellEffectInvokedEvent(caster, targetPair.Target, targetPair.Coords, spell, args);
            RaiseEvent(effect.Value, ev);
            if (ev.Handled)
                return ev.TurnResult;

            var rapidMagicShots = 1;
            if (spells.CanCastRapidMagic.Buffed && spell.IsRapidCastable)
            {
                // TODO reduce spell power!
                // if rapidMagic : efP=efP/2+1:dice1=dice1/2+1:dice2=dice2/2+1:bonus=bonus/2+1
                rapidMagicShots = 1 + (_rand.OneIn(3) ? 1 : 0) + (_rand.OneIn(2) ? 1 : 0);
            }

            TurnResult result = TurnResult.Failed;
            if (rapidMagicShots > 1)
            {
                // TODO combine turn results
                for (var i = 0; i < rapidMagicShots; i++)
                {
                    result = _newEffects.Apply(caster, targetPair.Target, targetPair.Coords, effect.Value, args);
                    if (!IsAlive(targetPair.Target))
                    {
                        if(_targetings.TrySearchForTarget(caster, out var newTarget) && _factions.GetRelationTowards(caster, newTarget.Value) <= Relation.Enemy)
                        {
                            targetPair = new(newTarget.Value, targetPair.Coords);
                        }
                        else
                        {
                            break;
                        }
                    }
                }   
            }
            else
            {
                result = _newEffects.Apply(caster, targetPair.Target, targetPair.Coords, effect.Value, args);
            }

            if (IsAlive(effect))
                EntityManager.DeleteEntity(effect.Value);

            return result;
            // <<<<<<<< elona122/shade2/proc.hsp:1350 	return true ..
        }

        private void BeforeSpellEffect_ProcVanillaCastingEvents(EntityUid effect, BeforeSpellEffectInvokedEvent args)
        {
            if (args.Handled)
                return;

            if (_statusEffects.HasEffect(args.Caster, Protos.StatusEffect.Confusion)
                || _statusEffects.HasEffect(args.Caster, Protos.StatusEffect.Dimming))
            {
                _mes.Display(Loc.GetString("Elona.Spells.Cast.Confused", ("caster", args.Caster)), entity: args.Caster);
                if (!_spellbooks.TryToReadSpellbook(args.Caster, args.Spell.Difficulty, Level(args.Caster, args.Spell)))
                {
                    args.Handle(TurnResult.Failed);
                    return;
                }
            }
            else
            {
                var spells = Comp<SpellsComponent>(args.Caster);
                LocaleKey castingStyle;
                if (spells.CastingStyle != null)
                {
                    var key = new LocaleKey($"Elona.Spells.CastingStyle").With(spells.CastingStyle.Value);
                    if (Loc.KeyExists(key))
                        castingStyle = key;
                    else
                        castingStyle = "Elona.Spells.CastingStyle.Default";
                }
                else
                {
                    castingStyle = "Elona.Spells.CastingStyle.Default";
                }

                if (_gameSession.IsPlayer(args.Caster))
                {
                    var skillName = Loc.GetPrototypeString(args.Spell.SkillID, "Name");
                    _mes.Display(Loc.GetString(castingStyle.With("WithSkillName"), ("caster", args.Caster), ("target", args.Target), ("skillName", skillName)), entity: args.Caster);
                }
                else
                {
                    _mes.Display(Loc.GetString(castingStyle.With("Generic"), ("caster", args.Caster), ("target", args.Target)), entity: args.Caster);
                }
            }

            if (_buffs.HasBuff<BuffMistOfSilenceComponent>(args.Caster))
            {
                _mes.Display(Loc.GetString("Elona.Spells.Cast.Silenced", ("caster", args.Caster)), entity: args.Caster);
                args.Handle(TurnResult.Failed);
                return;
            }

            if (!_rand.Prob(CalcSpellSuccessRate(args.Spell, args.Caster, effect)))
            {
                if (_vis.IsInWindowFov(args.Caster))
                {
                    _mes.Display(Loc.GetString("Elona.Spells.Cast.Fail", ("caster", args.Caster)), entity: args.Caster);
                    var anim = new SpellCastFailureMapDrawable();
                    _mapDrawables.Enqueue(anim, args.Caster);
                }
                args.Handle(TurnResult.Failed);
                return;
            }

            var encPower = _enchantments.GetTotalEquippedEnchantmentPower<EncEnhanceSpellsComponent>(args.Caster);
            if (encPower > 0)
            {
                args.Args.Power = (int)(args.Args.Power * (100f + encPower / 10f) / 100f);
            }
        }

        public string LocalizeSpellDescription(SpellPrototype proto, EntityUid caster, EntityUid effect)
        {
            // TODO format power
            var description = string.Empty;
            if (Loc.TryGetPrototypeString(proto.SkillID, "Description", out var desc))
                description = desc;
            return description;
        }

        public int CalcCastSpellPower(SpellPrototype spell, EntityUid caster)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:868 	if c=pc:return	sdata@(id,c)*10+50 ...
            if (_gameSession.IsPlayer(caster))
            {
                return Level(caster, spell) * 10 + 50;
            }

            // TODO customize AI spell power
            if (_skills.HasSkill(caster, Protos.Skill.Casting) && !_parties.IsInPlayerParty(caster))
            {
                return _levels.GetLevel(caster) * 6 + 10;
            }

            return _skills.Level(caster, Protos.Skill.Casting) * 6 + 10;
            // <<<<<<<< elona122/shade2/calculation.hsp:870 	return sCasting(c)*6+10 ...
        }

        public float CalcSpellSuccessRate(SpellPrototype proto, EntityUid caster, EntityUid effect)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:874 #defcfunc calcSpellFail int id,int c ...
            int intRate;
            if (!_gameSession.IsPlayer(caster))
            {
                if (_mounts.TryGetRider(caster, out var rider))
                {
                    intRate = 95 - int.Clamp(30 - _skills.Level(rider.Owner, Protos.Skill.Riding) / 2, 0, 30);
                    return float.Clamp(intRate / 100f, 0, 1);
                }
                else
                {
                    return 0.95f;
                }
            }

            var armorClass = _equip.GetArmorClass(caster);
            var factor = 4;
            if (armorClass == Protos.Skill.HeavyArmor)
                factor = 17 - _skills.Level(caster, armorClass) / 5;
            else if (armorClass == Protos.Skill.MediumArmor)
                factor = 12 - _skills.Level(caster, armorClass) / 5;

            factor = int.Max(factor, 4);

            if (_mounts.IsMounting(caster))
                factor += 4;

            var skillLevel = _skills.Level(caster, proto.SkillID);

            // TODO generalize
            if (proto.SkillID == Protos.Skill.SpellWish)
                factor += skillLevel;
            else if (proto.SkillID == Protos.Skill.SpellWizardsHarvest)
                factor += skillLevel;

            intRate = 90 + skillLevel
                - (proto.Difficulty * factor / (5 + _skills.Level(caster, Protos.Skill.Casting) * 4));

            if (armorClass == Protos.Skill.HeavyArmor)
            {
                intRate = int.Min(intRate, 80);
            }
            else if (armorClass == Protos.Skill.MediumArmor)
            {
                intRate = int.Min(intRate, 92);
            }
            else
            {
                intRate = int.Min(intRate, 100);
            }

            var equipState = _combat.GetEquipState(caster);
            if (equipState.IsDualWielding)
                intRate -= 6;
            if (equipState.IsWieldingShield)
                intRate -= 12;

            return float.Clamp(intRate / 100f, 0f, 1f);
            // <<<<<<<< elona122/shade2/calculation.hsp:902 	return p ...
        }

        public int CalcBaseSpellMPCost(SpellPrototype proto, EntityUid caster, EntityUid effect)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:906 #defcfunc calcSpellCostMp int id ,int c ...
            if (_gameSession.IsPlayer(caster))
            {
                if (proto.NoMPCostScaling)
                    return proto.MPCost;

                return proto.MPCost * (100 + _skills.Level(caster, proto.SkillID) * 3) / 100 + _skills.Level(caster, proto.SkillID) / 8;
            }
            else
            {
                return proto.MPCost * (50 + _levels.GetLevel(caster) * 3) / 100;
            }
            // <<<<<<<< elona122/shade2/calculation.hsp:913 	return cost ...
        }

        public int CalcBaseSpellStockCost(SpellPrototype proto, EntityUid caster, EntityUid effect)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:917 #defcfunc calcSpellCostStock int id ,int c ...
            return int.Max(proto.MPCost * 200 / (_skills.Level(caster, proto.SkillID) * 3 + 100), proto.MPCost / 5);
            // <<<<<<<< elona122/shade2/calculation.hsp:922 	return cost ...
        }

        public int CalcRandomizedSpellStockCost(SpellPrototype proto, EntityUid caster, EntityUid effect)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:917 #defcfunc calcSpellCostStock int id ,int c ...
            var cost = CalcBaseSpellStockCost(proto, caster, effect);
            cost = _rand.Next(cost / 2 + 1) + cost / 2;
            return int.Max(cost, 1);
            // <<<<<<<< elona122/shade2/calculation.hsp:922 	return cost ...
        }

        #endregion
    }

    [EventUsage(EventTarget.Effect)]
    public sealed class BeforeSpellEffectInvokedEvent : TurnResultEntityEventArgs
    {
        public BeforeSpellEffectInvokedEvent(EntityUid caster, EntityUid? target, EntityCoordinates? coords, SpellPrototype spell, EffectArgSet args)
        {
            Caster = caster;
            Target = target;
            Coords = coords;
            Spell = spell;
            Args = args;
        }

        public EntityUid Caster { get; }
        public EntityUid? Target { get; }
        public EntityCoordinates? Coords { get; }
        public SpellPrototype Spell { get; }
        public EffectArgSet Args { get; }
    }
}