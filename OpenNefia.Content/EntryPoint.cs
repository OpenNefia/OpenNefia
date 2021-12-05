using OpenNefia.Content.Logic;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
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
            SetupLogging();
        }

        private void SetupLogging()
        {
            var logManager = IoCManager.Resolve<ILogManager>();

            logManager.GetSawmill("repl.exec").Level = LogLevel.Info;
        }

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