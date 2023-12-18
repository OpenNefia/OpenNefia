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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Love.Misc.Moonshine;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Mount;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Parties;

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

        TurnResult NewCast(EntityUid caster, PrototypeId<SpellPrototype> spell);

        TurnResult Cast(PrototypeId<SpellPrototype> spellID, EntityUid target, int power = 0,
            EntityUid? source = null, EntityUid? item = null,
            EntityCoordinates? coords = null, CurseState? curseState = null,
            string effectSource = EffectSources.Default, EffectArgSet? args = null);

        string LocalizeSpellDescription(SpellPrototype proto, EntityUid caster, EntityUid effect);
        float CalcSpellSuccessRate(SpellPrototype proto, EntityUid caster, EntityUid effect);
        int CalcSpellMPCost(SpellPrototype proto, EntityUid caster, EntityUid effect);
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

        public TurnResult NewCast(EntityUid caster, PrototypeId<SpellPrototype> spellID)
        {
            var result = DoCastSpell(caster, spellID);
            if (result == TurnResult.Succeeded)
            {
                GainSpellAndCastingExp(caster, spellID);
            }
            return result;
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

        private TurnResult DoCastSpell(EntityUid caster, PrototypeId<SpellPrototype> spellID)
        {
            if (!_protos.TryIndex(spellID, out var spell))
                return TurnResult.Aborted;

            if (!_newEffects.TrySpawnEffect(spell.EffectID, out var effect))
                return TurnResult.Aborted;

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

            var result = _newEffects.Apply(caster, targetPair.Target, targetPair.Coords, effect.Value, args);

            if (IsAlive(effect))
                EntityManager.DeleteEntity(effect.Value);

            return result;
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

        public int CalcSpellMPCost(SpellPrototype proto, EntityUid caster, EntityUid effect)
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
}