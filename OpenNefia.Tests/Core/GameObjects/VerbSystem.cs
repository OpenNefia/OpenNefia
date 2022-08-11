using NUnit.Framework;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;

using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Tests.Core.GameObjects
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(VerbSystem))]
    public class VerbSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity = new("TestEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity}
  components:
  - type: Spatial
  - type: TestVerb
";

        [Test]
        public void TestVerbSystem()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory =>
                {
                    factory.LoadExtraSystemType<VerbSystem>();
                    factory.LoadExtraSystemType<VerbTestSystem>();
                })
                .RegisterComponents(factory => factory.RegisterClass<TestVerbComponent>())
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var verbSys = sim.GetEntitySystem<IVerbSystem>();
            var verbTestSys = sim.GetEntitySystem<VerbTestSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);
            var player = entMan.SpawnEntity(null, map.AtPos(0, 1));
            var ent1 = entMan.SpawnEntity(TestEntity, map.AtPos(1, 1));
            var ent2 = entMan.SpawnEntity(null, map.AtPos(1, 2));

            Assert.Multiple(() =>
            {
                Assert.That(verbSys.GetLocalVerbs(player, ent1).First()!.DisplayName, Is.EqualTo("Test Verb"), "GetLocalVerbs success");
                Assert.That(verbSys.GetLocalVerbs(ent1, player), Is.Empty, "GetLocalVerbs failure 1");
                Assert.That(verbSys.GetLocalVerbs(player, ent2), Is.Empty, "GetLocalVerbs failure 2");

                Assert.That(verbSys.CanUseVerbOn(player, ent1, "Test"), Is.True, "CanUseVerbOn success");
                Assert.That(verbSys.CanUseVerbOn(ent1, player, "Test"), Is.False, "CanUseVerbOn failure 1");
                Assert.That(verbSys.CanUseVerbOn(player, ent2, "Test"), Is.False, "CanUseVerbOn failure 2");
                
                Assert.That(verbSys.CanUseAnyVerbOn(player, ent1, new HashSet<string>() { "Test", "foo" }), Is.True, "CanUseAnyVerbOn success");
                Assert.That(verbSys.CanUseAnyVerbOn(ent1, player, new HashSet<string>() { "Test", "foo" }), Is.False, "CanUseAnyVerbOn failure");
                
                Assert.That(verbSys.TryGetVerb(ent1, player, "Test", out _), Is.False, "TryGetVerb failure 1");
                Assert.That(verbSys.TryGetVerb(player, ent2, "Test", out _), Is.False, "TryGetVerb failure 2");

                Assert.That(verbSys.TryGetVerb(player, ent1, "Test", out var verb), Is.True, "TryGetVerb success");
                Assert.That(verb, Is.Not.Null, "Verb is not null");
                Assert.That(verbTestSys.VerbsCalled, Is.EqualTo(0), "No verbs were called");
                Assert.That(verb!.Act(), Is.EqualTo(TurnResult.Succeeded), "Verb.Act()");
                Assert.That(verbTestSys.VerbsCalled, Is.EqualTo(1), "1 verb was called");
            });
        }

        private sealed class VerbTestSystem : EntitySystem
        {
            public int VerbsCalled { get; private set; } = 0;

            public override void Initialize()
            {
                SubscribeComponent<TestVerbComponent, GetVerbsEventArgs>(HandleGetVerbs);
            }

            private void HandleGetVerbs(EntityUid uid, TestVerbComponent component, GetVerbsEventArgs args)
            {
                // Sorting should be in alphabetical order by type then name.
                args.OutVerbs.Add(new Verb("Test", "Z Test Verb", () =>
                {
                    return TurnResult.Failed;
                }));

                args.OutVerbs.Add(new Verb("Test", "Test Verb", () =>
                {
                    VerbsCalled++;
                    return TurnResult.Succeeded;
                }));
            }
        }

        private sealed class TestVerbComponent : Component
        {
            public override string Name => "TestVerb";
        }
    }
}