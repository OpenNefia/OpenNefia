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

namespace OpenNefia.Content.Levels
{
    public interface ILevelSystem : IEntitySystem
    {
        int GetLevel(EntityUid entity, LevelComponent? levelComp = null);
    }

    public sealed class LevelSystem : EntitySystem, ILevelSystem
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;

        public override void Initialize()
        {
            SubscribeComponent<LevelComponent, EntityTurnStartingEventArgs>(ProcLevelGain, priority: EventPriorities.VeryLow);
        }

        private void ProcLevelGain(EntityUid uid, LevelComponent level, EntityTurnStartingEventArgs args)
        {
            while (level.Experience >= level.ExperienceToNext)
            {
                if (_gameSession.IsPlayer(uid))
                {
                    _audio.Play(Protos.Sound.Ding1);
                    _mes.Alert();
                }
                
                if (TryComp<SkillsComponent>(uid, out var skills))
                {
                    _skills.GainLevel(uid, showMessage: true, skills, level);
                }
            }
        }

        public int GetLevel(EntityUid entity, LevelComponent? levelComp = null)
        {
            if (!Resolve(entity, ref levelComp))
                return 1;

            return levelComp.Level;
        }
    }
}
