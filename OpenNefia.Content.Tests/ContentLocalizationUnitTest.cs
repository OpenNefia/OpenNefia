using NUnit.Framework;
using OpenNefia.Content.Charas;
using OpenNefia.Content.GameObjects;
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
  - type: Item
  - type: Stack
    count: 1

- type: Entity
  id: {EntityItemStackedID}
  components:
  - type: Spatial
  - type: Item
  - type: Stack
    count: 2
";

        private static readonly string PrototypeLocalizations = $@"
OpenNefia.Prototypes.Entity = 
{{
    [""{EntityCharaFemaleID}""] = {{
        MetaData = {{
            Name = ""CharaFemale""
        }},
    }},
    [""{EntityCharaMaleID}""] = {{
        MetaData = {{
            Name = ""CharaMale""
        }},
    }},
    [""{EntityItemSingleID}""] = {{
        MetaData = {{
            Name = ""ItemSingle""
        }},
    }},
    [""{EntityItemStackedID}""] = {{
        MetaData = {{
            Name = ""ItemStacked""
        }},
    }},
}}
";

        protected virtual IFullSimulationFactory GetSimulationFactory()
        {
            return ContentFullGameSimulation
               .NewSimulation()
               .RegisterDependencies(factory =>
               {
                   factory.Register<ILocalizationManager, TestingLocalizationManager>(overwrite: true);
                })
               .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes));
        }

        protected ISimulation SimulationFactory()
        {
            var sim = GetSimulationFactory().InitializeInstance();

            sim.CreateMapAndSetActive(10, 10);

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.SwitchLanguage(TestingLanguage);
            locMan.LoadString(PrototypeLocalizations);

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
