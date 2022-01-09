using OpenNefia.Content.Aliases;
using OpenNefia.Content.CharaMake;
using OpenNefia.Content.Logic;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.UI.Layer.Repl;
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
            IoCManager.Register<IReplLayer, ReplLayer>();
            IoCManager.Register<IRandomAliasGenerator, RandomAliasGenerator>();
        }
    }
}
