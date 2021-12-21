using CommandLine;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.CommandLine.Commands
{
    /// <summary>
    /// Batch command to run multiple commands in the same engine instance, since
    /// startup time is pretty significant.
    /// </summary>
    [Verb(name: "batch", HelpText = "Runs a sequence of commands from a batch file.")]
    internal class BatchCommand : BaseCommand
    {
        [Dependency] private readonly ICommandLineController _commandLineController = default!;

        [Value(0, MetaName = "filepath", Required = true, HelpText = "Path to a text file with one command per line.")]
        public string BatchFilePath { get; set; } = default!;

        public override bool CanRunInBatchMode => false;

        // https://stackoverflow.com/a/298968
        private static string[] ParseArguments(string commandLine)
        {
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"')
                {
                    inQuote = !inQuote;
                }
                if (!inQuote && parmChars[index] == ' ')
                {
                    parmChars[index] = '\n';
                }
            }
            return (new string(parmChars)).Split('\n');
        }

        public override void Execute()
        {
            using (var stream = File.Open(BatchFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var streamReader = new StreamReader(stream, EncodingHelpers.UTF8))
                {
                    foreach (var line in streamReader.ReadLines())
                    {
                        var args = ParseArguments(line);
                        if (_commandLineController.TryParseCommand(args, out var cmd))
                        {
                            if (cmd.CanRunInBatchMode)
                            {
                                cmd.Execute();
                            }
                            else
                            {
                                Logger.ErrorS("cli.batch", $"Cannot run command in batch mode: '{line}'");
                            }
                        }
                        else
                        {
                            Logger.ErrorS("cli.batch", $"Could not parse command: '{line}'");
                        }
                    }
                }
            }
        }
    }
}
