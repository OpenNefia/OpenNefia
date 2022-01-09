using NUnit.Framework;
using OpenNefia.Content.Charas;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using System.Reflection;

namespace OpenNefia.Content.Tests
{
    public class ContentLocalizationUnitTest : LocalizationUnitTest
    {
        protected static readonly PrototypeId<EntityPrototype> EntityCharaFemaleID = new("CharaFemale");
        protected static readonly PrototypeId<EntityPrototype> EntityCharaMaleID = new("CharaMale");
        protected static readonly PrototypeId<EntityPrototype> EntityItemSingleID = new("ItemSingle");
        protected static readonly PrototypeId<EntityPrototype> EntityItemStackedID = new("ItemStacked");

        private static readonly string Prototypes = @$"
- type: Entity
  id: {EntityCharaFemaleID}
  components:
  - type: Spatial
  - type: Chara
    gender: Female

- type: Entity
  id: {EntityCharaMaleID}
  components:
  - type: Spatial
  - type: Chara
    gender: Male

- type: Entity
  id: {EntityItemSingleID}
  components:
  - type: Spatial
  - type: Stack
    count: 1

- type: Entity
  id: {EntityItemStackedID}
  components:
  - type: Spatial
  - type: Stack
    count: 5
";

        protected ISimulation SimulationFactory()
        {
            var sim = ContentGameSimulation
               .NewSimulation()
               .RegisterComponents(factory =>
               {
                   factory.RegisterClass<CharaComponent>();
               })
               .RegisterDependencies(factory => factory.Register<ILocalizationManager, TestingLocalizationManager>(overwrite: true))
               .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
               .InitializeInstance();

            sim.CreateMapAndSetActive(10, 10);

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.SwitchLanguage(TestingLanguage);

            return sim;
        }

        protected override Assembly[] GetContentAssemblies()
        {
            return new Assembly[2]
            {
                typeof(OpenNefia.Content.EntryPoint).Assembly,
                typeof(ContentUnitTest).Assembly
            };
        }
    }
}
