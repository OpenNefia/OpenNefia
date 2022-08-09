using OpenNefia.Content.Activity;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Effects;
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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Spells
{
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

        bool TryGetKnown(EntityUid uid, PrototypeId<SpellPrototype> protoId, [NotNullWhen(true)] out LevelPotentialAndStock? level, SpellsComponent? spells = null);

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

        EffectResult Cast(PrototypeId<SpellPrototype> spellId, int power, EntityUid target, EntityUid? source = null, EntityUid? item = null, CurseState curseState = CurseState.Normal);
    }

    public sealed class SpellSystem : EntitySystem, ISpellSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        #region Querying

        public bool TryGetKnown(EntityUid uid, PrototypeId<SpellPrototype> protoId, [NotNullWhen(true)] out LevelPotentialAndStock? level, SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells))
            {
                level = null;
                return false;
            }

            return spells.TryGetKnown(protoId, out level);
        }

        public int Level(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null)
            => Level(uid, proto.GetStrongID(), spells);

        public int Level(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells))
                return 0;

            return spells.Level(protoId);
        }

        public int BaseLevel(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null)
            => BaseLevel(uid, proto.GetStrongID(), spells);

        public int BaseLevel(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells))
                return 0;

            return spells.BaseLevel(protoId);
        }

        public int Potential(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null)
            => Potential(uid, proto.GetStrongID(), spells);

        public int Potential(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells))
                return 0;

            return spells.Potential(protoId);
        }

        public bool HasSpell(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null)
            => HasSpell(uid, proto.GetStrongID(), spells);

        public bool HasSpell(EntityUid uid, PrototypeId<SpellPrototype> protoId, SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells))
                return false;

            return spells.TryGetKnown(protoId, out _);
        }

        public int SpellStock(EntityUid uid, SpellPrototype proto, SpellsComponent? spells = null)
            => SpellStock(uid, proto.GetStrongID(), spells);
        public int SpellStock(EntityUid uid, PrototypeId<SpellPrototype> spellID, SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells) || !spells.TryGetKnown(spellID, out var level))
                return 0;

            return level.SpellStock;
        }

        #endregion

        #region Leveling

        public void ModifyPotential(EntityUid uid, PrototypeId<SpellPrototype> spellID, int delta, SpellsComponent? spells = null)
        {
            if (delta == 0 || !Resolve(uid, ref spells) || !_protos.TryIndex(spellID, out var spellProto) || !TryGetKnown(uid, spellID, out var level))
                return;

            _skills.ModifyPotential(uid, spellProto, level.Stats, delta);
        }

        public void ModifyPotential(EntityUid uid, SpellPrototype skillProto, int delta, SpellsComponent? spells = null)
            => ModifyPotential(uid, skillProto.GetStrongID(), delta, spells);

        public void GainFixedSpellExp(EntityUid uid, PrototypeId<SpellPrototype> spellID, int expGained,
            SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells) || !TryGetKnown(uid, spellID, out var level))
                return;

            if (!_protos.TryIndex(spellID, out var spellProto))
            {
                Logger.WarningS("spell", $"No spell with ID {spellID} found.");
                return;
            }

            _skills.GainFixedSkillExp(uid, spellProto, level.Stats, expGained);
        }

        public void GainSpellExp(EntityUid uid, PrototypeId<SpellPrototype> spellID,
            int baseExpGained,
            int relatedSkillExpDivisor,
            int levelExpDivisor = 0,
            SpellsComponent? spells = null)
        {
            if (!Resolve(uid, ref spells) || !TryGetKnown(uid, spellID, out var level))
                return;

            if (!_protos.TryIndex(spellID, out var skillProto))
            {
                Logger.WarningS("spell", $"No spell with ID {spellID} found.");
                return;
            }

            _skills.GainSkillExp(uid, skillProto, level.Stats, baseExpGained, relatedSkillExpDivisor, levelExpDivisor);
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
            if (!Resolve(uid, ref spells))
                return;

            if (spells.TryGetKnown(spellID, out var level))
            {
                if (_gameSession.IsPlayer(uid))
                {
                    level.SpellStock += spellStock;
                    ModifyPotential(uid, spellID, 1);
                }
                return;
            }

            var newSkill = new LevelPotentialAndStock()
            {
                Level = initialValues?.Level ?? new(1),
                Potential = initialValues?.Potential ?? 0,
                Experience = initialValues?.Experience ?? 0,
                SpellStock = spellStock
            };
            newSkill.Level.Base = Math.Max(newSkill.Level.Base, 1);

            spells.Spells[spellID] = newSkill;

            if (newSkill.Potential <= 0)
            {
                ModifyPotential(uid, spellID, DefaultSpellPotential);
            }

            _refresh.Refresh(uid);
        }

        #endregion
        
        public EffectResult Cast(PrototypeId<SpellPrototype> spellId, int power, EntityUid target, EntityUid? source = null, EntityUid? item = null, CurseState curseState = CurseState.Normal)
        {
            IoCManager.Resolve<IMessagesManager>().Display($"TODO: Cast spell {spellId}", UiColors.MesYellow);
            return EffectResult.Succeeded;
        }
    }
}