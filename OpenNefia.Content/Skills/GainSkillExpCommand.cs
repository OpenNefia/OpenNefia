using CommandLine;
using OpenNefia.Core.Console;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Skills
{
    public sealed class GainSkillExpCommand : IConsoleCommand<GainSkillExpCommand.Args>
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        [Verb("gainSkillExp", HelpText = "Gains skill experience.")]
        public class Args
        {
            [Value(0, HelpText = "Skill ID", Required = true)]
            public PrototypeId<SkillPrototype> SkillID { get; set; } = default!;

            [Value(1, HelpText = "Amount of experience to gain", Default = 1000)]
            public int ExperienceAmount { get; set; }

            [Option("fixed", HelpText = "Do not apply experience divisors.", Default = false)]
            public bool Fixed { get; set; }
        }

        public void Execute(IConsoleShell shell, Args args)
        {
            if (!_protos.HasIndex(args.SkillID))
            {
                shell.WriteError($"No skill with ID '{args.SkillID}' found.");
                return;
            }

            var entity = _gameSession.Player;

            if (args.Fixed)
            {
                _skills.GainFixedSkillExp(entity, args.SkillID, args.ExperienceAmount);
            }
            else
            {
                _skills.GainSkillExp(entity, args.SkillID, args.ExperienceAmount);
            }
        }
    }
}
