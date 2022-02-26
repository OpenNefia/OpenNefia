using CommandLine;
using OpenNefia.Core.Console;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Skills
{
    public sealed class GainAllSkillsCommand : IConsoleCommand<GainAllSkillsCommand.Args>
    {
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        [Verb("gainAllSkills", HelpText = "Gains all skills.")]
        public class Args
        {
            [Value(0, HelpText = "Starting skill level", Default = 100)]
            public int SkillLevel { get; set; }
        }

        public void Execute(IConsoleShell shell, Args args)
        {
            var entity = _gameSession.Player;

            var initial = new LevelAndPotential()
            {
                Level = new(args.SkillLevel)
            };

            var gained = 0;

            foreach (var skill in _skills.EnumerateRegularSkills())
            {
                var skillId = skill.GetStrongID();

                if (!_skills.HasSkill(entity, skillId))
                {
                    _skills.GainSkill(entity, skillId, initial);
                    gained++;    
                }
            }

            if (gained > 0)
            {
                shell.WriteLine($"Gained {gained} skills.");
            }
            else
            {
                shell.WriteLine($"All skills are known already.");
            }
        }
    }
}
