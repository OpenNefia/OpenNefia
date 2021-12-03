using System.Reflection;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;

namespace OpenNefia
{
    public class Program
    {
        public static void Main(string[] args)
        {
            InitIoC();

            var gc = IoCManager.Resolve<IGameController>();

            if (!gc.Startup())
            {
                Logger.Fatal("Failed to start game controller!");
                return;
            }

            gc.Run();
        }

        private static void InitIoC()
        {
            IoCManager.InitThread();
            IoCSetup.Run();
            IoCManager.BuildGraph();

            RegisterReflection();
        }

        private static void RegisterReflection()
        {
            // Gets a handle to the shared and the current (client) dll.
            IoCManager.Resolve<IReflectionManager>().LoadAssemblies(new List<Assembly>(1)
            {
                AppDomain.CurrentDomain.GetAssemblyByName("OpenNefia"),
                Assembly.GetExecutingAssembly()
            });
        }
    }
}