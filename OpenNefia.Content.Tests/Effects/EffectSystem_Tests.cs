using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Effects.New;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Mount;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Tests;
using OpenNefia.Tests.Core.EngineVariables;

namespace OpenNefia.Content.Tests.Effects
{
    [TestFixture]
    [Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
    [TestOf(typeof(NewEffectSystem))]
    public class EffectSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity = new("TestEntity");
        private static readonly PrototypeId<EntityPrototype> TestEffectEntity = new("TestEffectEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity}
  components:
  - type: Spatial
  - type: Party
  - type: Skills
  - type: TestEffectResultComponent

- type: Entity
  id: {TestEffectEntity}
  components:
  - type: MetaData
    isMapSavable: false
  - type: Effect
  - type: EffectTargetOther
  - type: EffectTest
";

        [Test]
        public void Test_Retargeting()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .RegisterComponents(factory =>
                {
                    factory.RegisterClass<TestEffectResultComponent>();
                    factory.RegisterClass<EffectTestComponent>();
                })
                .RegisterEntitySystems(factory =>
                {
                    factory.LoadExtraSystemType<EffectTestSystem>();
                })
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var sys = sim.GetEntitySystem<INewEffectSystem>();
            var mounts = sim.GetEntitySystem<IMountSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var npcRider = entMan.SpawnEntity(TestEntity, map.AtPos(0, 0));
            var npcMount = entMan.SpawnEntity(TestEntity, map.AtPos(0, 1));

            entMan.EnsureComponent<MountRiderComponent>(npcRider);
            entMan.EnsureComponent<MountComponent>(npcMount);

            var riderHits = entMan.EnsureComponent<TestEffectResultComponent>(npcRider);
            var mountHits = entMan.EnsureComponent<TestEffectResultComponent>(npcMount);

            var effect = entMan.SpawnEntity(TestEffectEntity, MapCoordinates.Global);
            
            var retarget = entMan.EnsureComponent<EffectDamageRetargetComponent>(effect);
            retarget.Rules = new List<EffectRetargetRule>() { EffectRetargetRule.AlwaysMount };

            Assert.Multiple(() =>
            {
                Assert.That(riderHits.HitCount, Is.EqualTo(0));
                Assert.That(mountHits.HitCount, Is.EqualTo(0));

                sys.Apply(npcRider, npcRider, null, effect);
                Assert.That(riderHits.HitCount, Is.EqualTo(1));
                Assert.That(mountHits.HitCount, Is.EqualTo(0));
                
                Assert.That(mounts.TryMount(npcRider, npcMount), Is.True, "Mount");
                sys.Apply(npcRider, npcRider, null, effect);
                Assert.That(riderHits.HitCount, Is.EqualTo(1));
                Assert.That(mountHits.HitCount, Is.EqualTo(1));
            });
        }

        [DataDefinition]
        private sealed class TestEffectResultComponent : Component
        {
            [DataField]
            public int HitCount { get; set; } = 0;
        }

        [DataDefinition]
        private sealed class EffectTestComponent : Component
        {
        }

        [Reflect(false)]
        private sealed class EffectTestSystem : EntitySystem
        {
            public override void Initialize()
            {
                SubscribeComponent<EffectTestComponent, ApplyEffectDamageEvent>(ApplyTestEffect);
            }

            private void ApplyTestEffect(EntityUid uid, EffectTestComponent component, ApplyEffectDamageEvent args)
            {
                if (args.Handled || !IsAlive(args.InnerTarget))
                    return;

                var comp = EnsureComp<TestEffectResultComponent>(args.InnerTarget.Value);
                comp.HitCount++;

                args.Handle(TurnResult.Succeeded);
            }
        }
    }
}