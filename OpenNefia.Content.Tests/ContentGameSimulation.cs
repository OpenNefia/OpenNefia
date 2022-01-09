using OpenNefia.Content.EntityGen;
using OpenNefia.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                   factory.LoadExtraSystemType<EntityGenSystem>();
               });
        }
    }
}
