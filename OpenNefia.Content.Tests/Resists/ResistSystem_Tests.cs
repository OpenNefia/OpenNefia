using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Tests.Resists
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(ResistsSystem))]
    public class ResistSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestResistsEntityID = new("TestResistsEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestResistsEntityID}
  components:
  - type: Level
    level: 1
    experience: 1000
    experienceToNext: 3024
  - type: Resists
    skills:
      {Element.Fire}:
        level: 4
        potential: 81
";

        /// <summary>
        /// Test that buffing an unobtained resistance will still return the a buffed resist level.
        /// </summary>
        [Test]
        public void TestResistComponent_TryGetKnown_Buffed()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.CreateMapAndSetActive(10, 10);

            var resistsSys = sim.GetEntitySystem<IResistsSystem>();

            var ent = entMan.SpawnEntity(TestResistsEntityID, map.AtPos(Vector2i.One));
            var resists = entMan.GetComponent<ResistsComponent>(ent);

            Assert.Multiple(() =>
            {
                Assert.That(resists.TryGetKnown(Element.Cold, out _), Is.False);
                resistsSys.BuffLevel(ent, Element.Cold, 40);
                Assert.That(resists.TryGetKnown(Element.Cold, out var cold), Is.True);
                Assert.That(cold!.Level.Base, Is.EqualTo(0));
                Assert.That(cold.Level.Buffed, Is.EqualTo(40));
                resists.Refresh();
                Assert.That(resists.TryGetKnown(Element.Cold, out _), Is.False);
            });
        }
    }
}