using CommandLine;
using OpenNefia.Core.Console;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;

namespace OpenNefia.Content.ConsoleCommands
{
    public sealed class ShowTileLayerCommand : IConsoleCommand<ShowTileLayerCommand.Args>
    {
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;

        [Verb("showTileLayer", HelpText = "Shows or hides the tile layer of the given type.")]
        public class Args
        {
            [Value(0, HelpText = "Class name of the tile layer", Required = true)]
            public string TileLayerTypeName { get; set; } = default!;
        }

        public void Execute(IConsoleShell shell, Args args)
        {
            if (!_reflectionManager.TryLooseGetType(args.TileLayerTypeName, out var ty))
            {
                shell.WriteError($"No tile layer with typename '{args.TileLayerTypeName}' found.");
                return;
            }

            if (!ty.IsAssignableTo(typeof(ITileLayer)))
            {
                shell.WriteError($"Type '{ty}' does not implement '{nameof(ITileLayer)}.");
                return;
            }

            _mapRenderer.SetTileLayerEnabled(ty, true);
        }
    }

    public sealed class HideTileLayerCommand : IConsoleCommand<HideTileLayerCommand.Args>
    {
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;

        [Verb("hideTileLayer", HelpText = "Shows or hides the tile layer of the given type.")]
        public class Args
        {
            [Value(0, HelpText = "Class name of the tile layer", Required = true)]
            public string TileLayerTypeName { get; set; } = default!;
        }

        public void Execute(IConsoleShell shell, Args args)
        {
            if (!_reflectionManager.TryLooseGetType(args.TileLayerTypeName, out var ty))
            {
                shell.WriteError($"No tile layer with typename '{args.TileLayerTypeName}' found.");
                return;
            }

            if (!ty.IsAssignableTo(typeof(ITileLayer)))
            {
                shell.WriteError($"Type '{ty}' does not implement '{nameof(ITileLayer)}.");
                return;
            }

            _mapRenderer.SetTileLayerEnabled(ty, false);
        }
    }
}
