using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
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
               .RegisterDependencies(factory =>
               {
                   factory.Register<IMessagesManager, DummyMessageManager>();
               })
               .RegisterEntitySystems(factory =>
               {
                   factory.LoadExtraSystemType<DisplayNameSystem>();
               });
        }
    }

    internal class DummyMessageManager : IMessagesManager
    {
        public void Display(string text, Color? color = null, bool alert = false, bool noCapitalize = false, EntityUid? entity = null)
        {
        }

        public void Newline()
        {
        }

        public void Clear()
        {
        }
    }
}
