using System;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;

namespace OpenNefia.Benchmarks.Serialization
{
    public abstract class SerializationBenchmark
    {
        public SerializationBenchmark()
        {
            IoCManager.InitThread();
            IoCSetup.Register(GameController.DisplayMode.Headless);
            IoCManager.BuildGraph();

            var assemblies = new[]
            {
                AppDomain.CurrentDomain.GetAssemblyByName("OpenNefia.Core"),
                AppDomain.CurrentDomain.GetAssemblyByName("OpenNefia.Benchmarks")
            };

            IoCManager.Resolve<IReflectionManager>().LoadAssemblies(assemblies);

            SerializationManager = IoCManager.Resolve<ISerializationManager>();
        }

        protected ISerializationManager SerializationManager { get; }

        public void InitializeSerialization()
        {
            SerializationManager.Initialize();
        }
    }
}
