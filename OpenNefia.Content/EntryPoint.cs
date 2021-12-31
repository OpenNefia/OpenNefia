using OpenNefia.Content.Input;
using OpenNefia.Content.TitleScreen;
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
        public override void PreInit()
        {
        }

        public override void Init()
        {
            ContentIoC.Register();
            IoCManager.BuildGraph();
        }

        public override void PostInit()
        {
            // Setup key contexts
            var inputMan = IoCManager.Resolve<IInputManager>();
            ContentContexts.SetupContexts(inputMan.Contexts);

            var gc = IoCManager.Resolve<IGameController>();
            var mainTitle = IoCManager.Resolve<IMainTitleLogic>();

            gc.MainCallback += () => mainTitle.RunTitleScreen();
        }
    }
}