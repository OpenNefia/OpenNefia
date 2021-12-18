using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Tests;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.GameObjects.EntitySystems
{
    [TestFixture]
    [TestOf(typeof(CharaSystem))]
    public class CharaSystem_Test : ContentUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> CharaEntityMaleId = new("CharaTestDummyMale");
        private static readonly PrototypeId<EntityPrototype> CharaEntityFemaleId = new("CharaTestDummyFemale");

        private static readonly PrototypeId<ChipPrototype> ChipMaleId = new("ChipMale");
        private static readonly PrototypeId<ChipPrototype> ChipFemaleId = new("ChipFemale");

        private static readonly string Prototypes = $@"
- type: Chip
  id: {ChipMaleId}
  filepath: /Test/ChipMale.png

- type: Chip
  id: {ChipFemaleId}
  filepath: /Test/ChipFemale.png

- type: Race
  id: CharaTestRace
  chipMale: {ChipMaleId}
  chipFemale: {ChipFemaleId}
- type: Class
  id: CharaTestClass

- type: Entity
  id: {CharaEntityMaleId}
  components:
  - type: Chara
    race: CharaTestRace
    class: CharaTestClass
    gender: Male
  - type: Chip

- type: Entity
  id: {CharaEntityFemaleId}
  components:
  - type: Chara
    race: CharaTestRace
    class: CharaTestClass
    gender: Female
  - type: Chip
";

        [OneTimeSetUp]
        public void SetUp()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            prototypeManager.RegisterType<EntityPrototype>();
            prototypeManager.RegisterType<ChipPrototype>();
            prototypeManager.RegisterType<RacePrototype>();
            prototypeManager.RegisterType<ClassPrototype>();
            prototypeManager.Resync();
        }

        [Test]
        public void TestDefaultRaceChip()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(entitySystems => entitySystems.LoadExtraSystemType<CharaSystem>())
                .RegisterComponents(compMan => {
                    compMan.RegisterClass<ChipComponent>();
                    compMan.RegisterClass<CharaComponent>();
                })
                .RegisterPrototypes(protoMan =>
                {
                    protoMan.RegisterType<ChipPrototype>();
                    protoMan.RegisterType<RacePrototype>();
                    protoMan.RegisterType<ClassPrototype>();
                    protoMan.LoadString(Prototypes);
                })
                .InitializeInstance();

            var map = sim.CreateMapAndSetActive(50, 50);

            var mapMan = sim.Resolve<IMapManager>();
            var entMan = sim.Resolve<IEntityManager>();
            var protoMan = sim.Resolve<IPrototypeManager>();

            var entMale = entMan.SpawnEntity(CharaEntityMaleId, map.AtPos(Vector2i.Zero)).Uid;
            var entFemale = entMan.SpawnEntity(CharaEntityFemaleId, map.AtPos(Vector2i.One)).Uid;
            var chipCompMale = entMan.GetComponent<ChipComponent>(entMale);
            var chipCompFemale = entMan.GetComponent<ChipComponent>(entFemale);

            Assert.That(chipCompMale.ChipID, Is.EqualTo(ChipMaleId));
            Assert.That(chipCompFemale.ChipID, Is.EqualTo(ChipFemaleId));
        }
    }
}
