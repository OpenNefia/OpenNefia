using OpenNefia.Core.Timing;

namespace OpenNefia.Core.GameController
{
    public interface IGameController
    {
        public Action? MainCallback { get; set; }

        public bool Startup();
        public void Run();

        void Update(FrameEventArgs frame);
        void Draw();
        void SystemStep();
    }
}