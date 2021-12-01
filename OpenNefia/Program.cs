using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Reflection;

namespace OpenNefia
{
    /// <summary>
    /// THE DICE HAS BEEN CAST A MILLION TIMES ALREADY I'M ON WELFARE
    /// </summary>
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
                Assembly.GetExecutingAssembly()
            });
        }
    }
}
