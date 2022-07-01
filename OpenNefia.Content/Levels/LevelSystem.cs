using OpenNefia.Content.Logic;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Skills;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.Levels
{
    public interface ILevelSystem : IEntitySystem
    {
        int CalcExperienceToNext(EntityUid entity, LevelComponent? levelComp = null);
        int GetLevel(EntityUid entity, LevelComponent? levelComp = null);
        void GainLevel(EntityUid uid, bool showMessage = true, LevelComponent? level = null);
    }

    public sealed class LevelSystem : EntitySystem, ILevelSystem
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;

        public override void Initialize()
        {
            SubscribeComponent<LevelComponent, EntityBeingGeneratedEvent>(SetInitialRequiredExperience, priority: EventPriorities.High);
            SubscribeComponent<LevelComponent, EntityTurnStartingEventArgs>(ProcLevelGain, priority: EventPriorities.VeryLow);
        }

        private void SetInitialRequiredExperience(EntityUid uid, LevelComponent component, ref EntityBeingGeneratedEvent args)
        {
            component.ExperienceToNext = CalcExperienceToNext(uid, component);
        }

        private void ProcLevelGain(EntityUid uid, LevelComponent level, EntityTurnStartingEventArgs args)
        {
            while (level.Experience >= level.ExperienceToNext)
            {
                level.Experience = Math.Max(level.Experience - level.ExperienceToNext, 0);
                GainLevel(uid, showMessage: true, level);
            }
        }

        public void GainLevel(EntityUid uid, bool showMessage = true, LevelComponent? level = null)
        {
            if (!Resolve(uid, ref level))
                return;
            
            level.Level++;

            if (_gameSession.IsPlayer(uid) && showMessage)
            {
                _audio.Play(Protos.Sound.Ding1);
                _mes.Alert();
            }

            if (TryComp<SkillsComponent>(uid, out var skills))
            {
                _skills.ApplyEntityLevelUpGrowth(uid, showMessage, skills, level);
            }

            level.MaxLevelReached = Math.Max(level.MaxLevelReached, level.Level);
            level.ExperienceToNext = CalcExperienceToNext(uid, level);
        }

        public int CalcExperienceToNext(EntityUid entity, LevelComponent? levelComp = null)
        {
            if (!Resolve(entity, ref levelComp))
                return 0;

            var level = Math.Clamp(levelComp.Level, 1, 200);
            return Math.Clamp(level * (level + 1) * (level + 2) * (level + 3) + 3000, 0, 100000000);
        }

        public int GetLevel(EntityUid entity, LevelComponent? levelComp = null)
        {
            if (!Resolve(entity, ref levelComp))
                return 1;

            return levelComp.Level;
        }
    }
}
