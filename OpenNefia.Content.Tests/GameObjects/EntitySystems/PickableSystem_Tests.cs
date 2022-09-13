using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Pickable;
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
    [TestOf(typeof(PickableSystem))]
    public class PickableSystem_Test : ContentUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> PickableEntityId = new("PickableTestDummy");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {PickableEntityId}
  components:
  - type: Pickable
";

        [OneTimeSetUp]
        public void SetUp()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            prototypeManager.RegisterType<EntityPrototype>();
            prototypeManager.ResolveResults();
        }

        [Test]
        public void TestItemGenOwnState()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan =>
                {
                    protoMan.LoadString(Prototypes);
                })
                .InitializeInstance();

            var map = sim.CreateMapAndSetActive(5, 5);

            var mapMan = sim.Resolve<IMapManager>();
            var entMan = sim.Resolve<IEntityManager>();
            var protoMan = sim.Resolve<IPrototypeManager>();

            var entGen = sim.GetEntitySystem<IEntityGen>();

            var args = EntityGenArgSet.Make(new ItemGenArgs() { OwnState = OwnState.NPC });
            var ent = entGen.SpawnEntity(PickableEntityId, map, args: args)!.Value;

            Assert.That(entMan.GetComponent<PickableComponent>(ent).OwnState, Is.EqualTo(OwnState.NPC));
        }
    }
}
