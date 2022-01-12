using OpenNefia.Core.Timing;

namespace OpenNefia.Core.GameController
{
    public interface IGameController
    {
        public Action? MainCallback { get; set; }

        public bool Startup(GameControllerOptions options);
        public void Run();

        void Update(FrameEventArgs frame);
        void Draw();
        void SystemStep();
    }
}