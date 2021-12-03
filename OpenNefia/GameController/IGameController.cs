namespace OpenNefia.Core.GameController
{
    public interface IGameController
    {
        public Action? MainCallback { get; set; }

        public bool Startup();
        public void Run();

        void Update(float dt);
        void Draw();
        void SystemStep();
    }
}