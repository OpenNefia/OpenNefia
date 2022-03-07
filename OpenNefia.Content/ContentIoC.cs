using OpenNefia.Content.CharaMake;
using OpenNefia.Content.ConfigMenu;
using OpenNefia.Content.Logic;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.Repl;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.UI.Stylesheets;
using OpenNefia.Content.World;
using OpenNefia.Core.Console;
using OpenNefia.Core.IoC;
using PrettyPrompt.Consoles;

namespace OpenNefia.Content
{
    public static class ContentIoC
    {
        public static void Register()
        {
            IoCManager.Register<IPlayerQuery, PlayerQuery>();
            IoCManager.Register<IMainTitleLogic, MainTitleLogic>();
            IoCManager.Register<ICharaMakeLogic, CharaMakeLogic>();
            IoCManager.Register<IFieldLayer, FieldLayer>();
            IoCManager.Register<IHudLayer, HudLayer>();
            IoCManager.Register<IMessage, Message>();
            IoCManager.Register<IReplLayer, ReplLayer>();
            IoCManager.Register<IConsoleHost, ConsoleHost>();
            IoCManager.Register<IReplExecutor, ConsoleCommandReplExecutor>();
            IoCManager.Register<IConsoleOutput, ReplLayerConsoleOutput>();
            IoCManager.Register<ICSharpReplExecutor, CSharpReplExecutor>();
            IoCManager.Register<IConsole, DummyConsole>();
            IoCManager.Register<IRandomAliasGenerator, RandomAliasGenerator>();
            IoCManager.Register<IRandomNameGenerator, RandomNameGenerator>();
            IoCManager.Register<IConfigMenuUICellFactory, ConfigMenuUICellFactory>();
            IoCManager.Register<IPlayTimeManager, PlayTimeManager>();
            IoCManager.Register<IStylesheetManager, StylesheetManager>();
        }
    }
}
