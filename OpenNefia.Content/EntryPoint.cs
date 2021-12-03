using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content
{
    public class EntryPoint : ModEntryPoint
    {
        public override void PostInit()
        {
            var gc = IoCManager.Resolve<IGameController>();
            var mainTitle = IoCManager.Resolve<IMainTitleLogic>();

            gc.MainCallback += () => mainTitle.RunTitleScreen();
        }
    }
}