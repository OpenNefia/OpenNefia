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
            IoCSetup.Register(DisplayMode.Headless);
            IoCManager.BuildGraph();

            var assemblies = new[]
            {
                typeof(OpenNefia.Core.Engine).Assembly,
                typeof(SerializationBenchmark).Assembly
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
