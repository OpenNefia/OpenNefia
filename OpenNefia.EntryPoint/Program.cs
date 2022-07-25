using System.Reflection;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Timing;

namespace OpenNefia
{
    /// <summary>
    /// Separate entry point to allow adding mods as build dependencies.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                GameStart();
            }
            else
            {
                CommandLineStart(args);
            }
        }

        private static void GameStart()
        {
            InitIoC(DisplayMode.Love);

            var options = new GameControllerOptions();

            var gc = IoCManager.Resolve<IGameController>();

            using (var logger = new ProfilerLogger(LogLevel.Info, "engine", "Engine init"))
            {
                if (!gc.Startup(options))
                {
                    Logger.Fatal("Failed to start game controller!");
                    return;
                }
            }

            gc.Run();
        }

        private static void CommandLineStart(string[] args)
        {
            InitIoC(DisplayMode.Headless);
            Logger.Error("TODO");
        }

        private static void InitIoC(DisplayMode mode)
        {
            IoCManager.InitThread();
            IoCSetup.Register(mode);
            IoCManager.BuildGraph();

            RegisterReflection();
        }

        private static void RegisterReflection()
        {
            // Gets a handle to the shared and the current (client) dll.
            IoCManager.Resolve<IReflectionManager>().LoadAssemblies(new List<Assembly>(1)
            {
                typeof(OpenNefia.Core.Engine).Assembly,
                Assembly.GetExecutingAssembly()
            });
        }
    }
}