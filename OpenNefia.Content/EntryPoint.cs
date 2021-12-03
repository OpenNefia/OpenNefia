using OpenNefia.Content.Logic;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content
{
    /// <summary>
    /// second-system syndrome is now in effect
    /// </summary>
    public class EntryPoint : ModEntryPoint
    {
        public override void Init()
        {
            ContentIoC.Register();
            IoCManager.BuildGraph();
        }

        public override void PostInit()
        {
            var gc = IoCManager.Resolve<IGameController>();
            var mainTitle = IoCManager.Resolve<IMainTitleLogic>();

            gc.MainCallback += () => mainTitle.RunTitleScreen();
        }
    }
}