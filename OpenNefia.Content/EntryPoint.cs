using OpenNefia.Content.ConfigMenu;
using OpenNefia.Content.Input;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.Repl;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Content.UI.Stylesheets;
using OpenNefia.Content.World;
using OpenNefia.Core.Console;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;

namespace OpenNefia.Content
{
    /// <summary>
    /// second-system syndrome is now in effect
    /// </summary>
    public class EntryPoint : ModEntryPoint
    {
        [Dependency] private readonly IConsoleHost _consoleHost = default!;
        [Dependency] private readonly IReplLayer _repl = default!;
        [Dependency] private readonly IRandomAliasGenerator _aliasGen = default!;
        [Dependency] private readonly IRandomNameGenerator _nameGen = default!;
        [Dependency] private readonly IConfigMenuUICellFactory _configMenuUICellFactory = default!;
        [Dependency] private readonly IPlayTimeManager _playTimeManager = default!;
        [Dependency] private readonly IStylesheetManager _stylesheetManager = default!;

        public override void PreInit()
        {
        }

        public override void Init()
        {
            ContentIoC.Register();
            IoCManager.BuildGraph();

            IoCManager.InjectDependencies(this);

            InitializeDependencies();
        }

        private void InitializeDependencies()
        {
            _repl.Initialize();
            _aliasGen.Initialize();
            _nameGen.Initialize();
            _configMenuUICellFactory.Initialize();
            _playTimeManager.Initialize();
            _stylesheetManager.Initialize();
        }

        public override void PostInit()
        {
            // Commands can take dependencies on entity systems, so this can't go in Init().
            _consoleHost.Initialize();

            // Setup key contexts
            var inputMan = IoCManager.Resolve<IInputManager>();
            ContentContexts.SetupContexts(inputMan.Contexts);

            var gc = IoCManager.Resolve<IGameController>();
            var mainTitle = IoCManager.Resolve<IMainTitleLogic>();

            gc.MainCallback += () => mainTitle.RunTitleScreen();
        }
    }
}