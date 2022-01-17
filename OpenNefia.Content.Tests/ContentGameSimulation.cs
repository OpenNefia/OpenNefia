using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests
{
    public class ContentGameSimulation
    {
        public static ISimulationFactory NewSimulation()
        {
            return GameSimulation
               .NewSimulation()
               .LoadAssemblies(list => list.Add(typeof(Content.EntryPoint).Assembly))
               .RegisterEntitySystems(factory =>
               {
                   factory.LoadExtraSystemType<DisplayNameSystem>();
                   factory.LoadExtraSystemType<EntityGenSystem>();
               });
        }
    }
}
