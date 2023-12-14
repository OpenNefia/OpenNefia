using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.EntityGen
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(CharaGenSystem))]
    public class CharaGen_Tests : OpenNefiaUnitTest
    {
        private static ISimulation SimulationFactory()
        {
            var sim = ContentFullGameSimulation
               .NewSimulation()
               .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        [Test]
        public void CharaGenFltSelectTest()
        {
            var sim = SimulationFactory();
            var map = sim.CreateMapAndSetActive(1, 1);
            var entMan = sim.Resolve<IEntityManager>();
            var charaGen = sim.GetEntitySystem<ICharaGen>();

            var args = new CharaGenArgs()
            {
                Category = CreaturePacks.Dog
            };
            var id = charaGen.PickRandomCharaId(null, EntityGenArgSet.Make(args), minLevel: 28);

            Assert.That(id, Is.EqualTo(Protos.Chara.Dog));
        }
    }
}