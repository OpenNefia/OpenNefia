using System.IO;
using System.Text.RegularExpressions;
using CommandLine;
using JetBrains.Annotations;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Console.Commands
{
    internal sealed class BatchCommand : IConsoleCommand<BatchCommand.Args>
    {
        [Dependency] private readonly IResourceManager _resourceManager = default!;

        [Verb("batch", HelpText = HelpText)]
        public class Args
        {
            [Value(0, MetaName = "fileName")]
            public ResourcePath FileName { get; set; } = default!;
        }

        private const string HelpText =
            @"Executes a script file from the game's data directory.
              Each line in the file is executed as a single command, unless it starts with a #";

        private static readonly Regex CommentRegex = new Regex(@"^\s*#");

        public void Execute(IConsoleShell shell, Args args)
        {
            var path = args.FileName.ToRootedPath();
            if (!_resourceManager.UserData.Exists(path))
            {
                shell.WriteError("File does not exist.");
                return;
            }

            using var text = _resourceManager.UserData.OpenText(path);
            while (true)
            {
                var line = text.ReadLine();
                if (line == null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(line) || CommentRegex.IsMatch(line))
                {
                    // Comment or whitespace.
                    continue;
                }

                shell.ExecuteCommand(line);
            }
        }
    }
}