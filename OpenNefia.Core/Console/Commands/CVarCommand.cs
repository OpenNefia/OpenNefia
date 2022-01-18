using System.Globalization;
using CommandLine;
using JetBrains.Annotations;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Console;
using OpenNefia.Core.IoC;

namespace Robust.Server.Console.Commands
{
    [UsedImplicitly]
    internal sealed class CVarCommand : IConsoleCommand<CVarCommand.Args>
    {
        [Verb("cvar", HelpText = HelpText)]
        public sealed class Args
        {
            [Value(0, MetaName = "name", Required = true)]
            public string Name { get; set; } = default!;

            [Value(1, MetaName = "value")]
            public string? Value { get; set; }
        }

        private const string HelpText = @"Gets or sets a CVar.
If a value is passed, the value is parsed and stored as the new value of the CVar.
If not, the current value of the CVar is displayed.
Use 'cvar ?' to get a list of all registered CVars.";

        public void Execute(IConsoleShell shell, Args args)
        {
            var configManager = IoCManager.Resolve<IConfigurationManager>();

            if (args.Name == "?")
            {
                var cvars = configManager.GetRegisteredCVars();
                shell.WriteLine(string.Join("\n", cvars));
                return;
            }

            if (!configManager.IsCVarRegistered(args.Name))
            {
                shell.WriteLine($"CVar '{args.Name}' is not registered. Use 'cvar ?' to get a list of all registered CVars.");
                return;
            }

            if (args.Value == null)
            {
                // Read CVar
                var value = configManager.GetCVar<object>(args.Name);
                shell.WriteLine(value.ToString()!);
            }
            else
            {
                // Write CVar
                var type = configManager.GetCVarType(args.Name);
                try
                {
                    var parsed = ParseObject(type, args.Value);
                    configManager.SetCVar(args.Name, parsed);
                }
                catch (FormatException)
                {
                    shell.WriteLine($"Input value is in incorrect format for type {type}");
                }
            }
        }

        private static object ParseObject(Type type, string input)
        {
            if (type == typeof(bool))
            {
                if (bool.TryParse(input, out var val))
                    return val;

                if (int.TryParse(input, out var intVal))
                {
                    if (intVal == 0) return false;
                    if (intVal == 1) return true;
                }

                throw new FormatException($"Could not parse bool value: {input}");
            }

            if (type == typeof(string))
            {
                return input;
            }

            if (type == typeof(int))
            {
                return int.Parse(input, CultureInfo.InvariantCulture);
            }

            if (type == typeof(float))
            {
                return float.Parse(input, CultureInfo.InvariantCulture);
            }

            throw new NotImplementedException();
        }
    }
}
