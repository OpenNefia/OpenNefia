using OpenNefia.Analyzers;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Game;
using OpenNefia.Content.Feats;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Visibility;

namespace OpenNefia.Content.Skills
{
    /// <summary>
    /// This holds the leveling algorithms shared by skills and magic, both of which
    /// implement <see cref="ISkillPrototype"/>.
    /// </summary>
    public sealed partial class SkillsSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;

        public static readonly IntRange PotentialRange = new(1, 400);

        public const float PotentialLevelingDecayRate = 0.9f;

        public void ModifyPotential(EntityUid uid, ISkillPrototype skillProto, LevelAndPotential level, int delta)
        {
            if (delta == 0)
                return;

            level.Potential = PotentialRange.Clamp(level.Potential + delta);
        }

        private int CalcRelatedSkillExp(int baseAmount, int expDivisor)
        {
            return baseAmount / (2 + expDivisor);
        }

        private int CalcSkillExpGain(int baseExpGained, int potential, int skillLevel, float growthBuff)
        {
            var expGained = baseExpGained * potential / (100 + skillLevel * 15);

            if (growthBuff > 0)
                expGained = (int)(expGained * (1.0f + growthBuff));

            return expGained;
        }

        private int CalcLevelExpGainFromSkillExpGain(int experienceToNext, int level, int skillExpGained, int levelExpDivisor)
        {
            return _rand.Next(experienceToNext * skillExpGained / 1000 / (level + levelExpDivisor) + 1) + _rand.Next(2);
        }

        /// <inheritdoc/>
        /// <hsp>#deffunc skillMod int skill, int chara, int exp</hsp>
        public void GainFixedSkillExp(EntityUid uid, ISkillPrototype skillProto, LevelAndPotential level, int expGained,
            SkillsComponent? skills = null)
        {
            if (expGained == 0)
                return;

            var potential = level.Potential;
            var newExp = level.Experience + expGained;

            if (potential == 0)
                return;

            var levelDelta = ProcSkillLeveling(uid, skillProto, level, newExp);

            var ev = new SkillExpGainedEvent(skillProto, expGained, expGained, levelDelta);
            RaiseEvent(uid, ref ev);
        }

        /// <inheritdoc/>
        public void GainSkillExp(EntityUid uid, ISkillPrototype skillProto,
            LevelAndPotential level,
            int baseExpGained,
            int relatedSkillExpDivisor,
            int levelExpDivisor = 0)
        {
            // >>>>>>>> shade2/module.hsp:283 	#deffunc skillExp int sid,int c,int a,int refExpD ..

            if (baseExpGained == 0)
                return;

            if (skillProto.RelatedSkill != null)
            {
                // [[[ Recursion alert! ]]]
                var relatedSkillExp = CalcRelatedSkillExp(baseExpGained, relatedSkillExpDivisor);
                GainSkillExp(uid, skillProto.RelatedSkill.Value, relatedSkillExp);
            }

            var baseLevel = level.Level.Base;
            var potential = level.Potential;

            if (potential == 0)
                return;

            int actualExpGained;

            if (baseExpGained > 0)
            {
                // TODO growth buffs
                var growthBuff = 0;
                actualExpGained = CalcSkillExpGain(baseExpGained, potential, baseLevel, growthBuff);
                if (actualExpGained == 0)
                {
                    if (_rand.OneIn(baseLevel / 10 + 1))
                        actualExpGained = 1;
                    else
                        return;
                }
            }
            else
            {
                actualExpGained = baseExpGained;
            }

            // TODO map experience divisor

            var applyLevelDivisor = skillProto.SkillType != SkillType.Attribute && skillProto.SkillType != SkillType.AttributeSpecial;
            if (actualExpGained > 0 && applyLevelDivisor && levelExpDivisor <= 1000)
            {
                if (EntityManager.TryGetComponent(uid, out LevelComponent levelComp))
                {
                    var levelExpGained = CalcLevelExpGainFromSkillExpGain(levelComp.ExperienceToNext, levelComp.Level, actualExpGained, levelExpDivisor);

                    _levels.GainExperience(uid, levelExpGained, levelComp);
                }
            }

            var newExp = level.Experience + actualExpGained;
            var levelDelta = ProcSkillLeveling(uid, skillProto, level, newExp);

            var ev = new SkillExpGainedEvent(skillProto, baseExpGained, actualExpGained, levelDelta);
            RaiseEvent(uid, ref ev);
            // <<<<<<<< shade2/module.hsp:349 	#defcfunc calcFame int c,int per ..
        }

        private int CalcNewPotentialFromLeveling(int potential, int levelDelta)
        {
            var newPotential = potential;

            if (levelDelta > 0)
            {
                for (int i = 0; i < levelDelta; i++)
                {
                    newPotential = PotentialRange.Clamp((int)(newPotential * PotentialLevelingDecayRate));
                }
            }
            else if (levelDelta < 0)
            {
                for (int i = 0; i > -levelDelta; i--)
                {
                    newPotential = PotentialRange.Clamp((int)(newPotential * (1.0f + (1.0f - PotentialLevelingDecayRate))) + 1);
                }
            }

            return newPotential;
        }

        private enum SkillChangeType
        {
            Increased,
            Decreased
        }

        private string GetSkillChangeText(EntityUid entity, ISkillPrototype skillProto, SkillChangeType type)
        {
            string keySuffix;

            switch (type)
            {
                case SkillChangeType.Increased:
                default:
                    keySuffix = "OnIncrease";
                    break;
                case SkillChangeType.Decreased:
                    keySuffix = "OnDecrease";
                    break;
            }

            // skillProto is going to be ISkillPrototype and not the concrete type, so GetStrongID()
            // will not work.
            if (Loc.TryGetPrototypeStringRaw(skillProto.GetType(), skillProto.ID, keySuffix, out var text, ("entity", entity)))
                return text;

            var skillName = Loc.GetPrototypeStringRaw(skillProto.GetType(), skillProto.ID, "Name");
            return Loc.GetString($"Elona.Skill.Default.{keySuffix}", ("entity", entity), ("skillName", skillName));
        }

        private int ProcSkillLeveling(EntityUid uid, ISkillPrototype skillProto, LevelAndPotential skill, int newExp)
        {
            if (newExp >= 1000)
            {
                var levelDelta = newExp / 1000;
                newExp %= 1000;
                var newLevel = skill.Level.Base + levelDelta;
                var newPotential = CalcNewPotentialFromLeveling(skill.Potential, levelDelta);

                skill.Level.Base = newLevel;
                skill.Potential = newPotential;
                skill.Experience = newExp;

                if (_visibility.IsInWindowFov(uid))
                {
                    var mesText = GetSkillChangeText(uid, skillProto, SkillChangeType.Increased);
                    var mesColor = UiColors.MesWhite;
                    var mesAlert = false;

                    if (_parties.IsInPlayerParty(uid))
                    {
                        Sounds.Play(Protos.Sound.Ding3);
                        mesColor = UiColors.MesGreen;
                        mesAlert = true;
                    }

                    _mes.Display(mesText, mesColor, alert: mesAlert);
                }

                _refresh.Refresh(uid);
                return levelDelta;
            }
            else if (newExp < 0)
            {
                var levelDelta = (-newExp / 1000 + 1);
                newExp = 1000 + newExp % 1000;
                if (skill.Level.Base - levelDelta < 1)
                {
                    levelDelta = Math.Max(skill.Level.Base - 1, 0);
                    if (skill.Level.Base == 1 && levelDelta == 0)
                    {
                        newExp = 0;
                    }
                }

                var newLevel = Math.Max(skill.Level.Base - levelDelta, 0);
                var newPotential = CalcNewPotentialFromLeveling(skill.Potential, -levelDelta);

                skill.Level.Base = newLevel;
                skill.Potential = newPotential;
                skill.Experience = newExp;

                if (levelDelta != 0 && _visibility.IsInWindowFov(uid))
                {
                    var mesText = GetSkillChangeText(uid, skillProto, SkillChangeType.Decreased);
                    _mes.Display(mesText, UiColors.MesRed, alert: true);
                }

                _refresh.Refresh(uid);
                return levelDelta;
            }
            else
            {
                skill.Experience = newExp;
                return 0;
            }
        }
    }

    [ByRefEvent]
    public struct SkillExpGainedEvent
    {
        public ISkillPrototype Skill { get; }
        public int BaseExpGained { get; }
        public int ActualExpGained { get; }
        public int LevelDelta { get; }

        public SkillExpGainedEvent(ISkillPrototype skillProto, int baseExpGained, int actualExpGained, int levelDelta)
        {
            Skill = skillProto;
            BaseExpGained = baseExpGained;
            ActualExpGained = actualExpGained;
            LevelDelta = levelDelta;
        }
    }
}
