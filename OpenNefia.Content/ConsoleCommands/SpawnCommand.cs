using CommandLine;
using CommandLine.Text;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Console;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.ConsoleCommands
{
    public sealed class SpawnCommand : IConsoleCommand<SpawnCommand.Args>
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        [Verb("spawn", HelpText = "Spawns an entity at the given map coordinates.")]
        public class Args
        {
            [Value(0, HelpText = "EntityPrototype ID.", Required = true)]
            public string ID { get; set; } = default!;

            [Value(1, HelpText = "X coordinate in the map.", Required = true)]
            public int X { get; set; }

            [Value(2, HelpText = "Y coordinate in the map.", Required = true)]
            public int Y { get; set; }
        }

        public void Execute(IConsoleShell shell, Args args)
        {
            var map = _mapManager.ActiveMap;
            if (map == null)
            {
                shell.WriteError($"No active map.");
                return;
            }

            var id = new PrototypeId<EntityPrototype>(args.ID);

            if (!_protos.HasIndex(id))
            {
                shell.WriteError($"No entity prototype with ID '{id}' found.");
                return;
            }

            var coords = map.AtPos(args.X, args.Y);

            _entityGen.SpawnEntity(id, coords);
        }
    }
}
