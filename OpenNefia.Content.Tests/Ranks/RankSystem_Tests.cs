using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Nefia;
using OpenNefia.Content.RandomAreas;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Ranks;

namespace OpenNefia.Content.Tests.Ranks
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(RankSystem))]
    public class RankSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<RankPrototype> TestRankID = new("TestRank");

        private static readonly string Prototypes = $@"
- type: Elona.Rank
  id: {TestRankID}
";

        [Test]
        public void TestRankSet()
        {
            var sim = ContentGameSimulation
                .NewSimulation()
                .RegisterDataDefinitionTypes(types => types.Add(typeof(Rank)))
                .RegisterPrototypes(protos =>
                {
                    protos.RegisterType<RankPrototype>();
                    protos.LoadString(Prototypes);
                })
                .RegisterEntitySystems(factory => factory.LoadExtraSystemType<RankSystem>())
                .InitializeInstance();

            var ranks = sim.GetEntitySystem<RankSystem>();

            Assert.That(ranks.GetRank(TestRankID).Experience, Is.EqualTo(10000));
            Assert.That(ranks.GetRank(TestRankID).Place, Is.EqualTo(100));

            ranks.SetRank(TestRankID, 0);
            Assert.That(ranks.GetRank(TestRankID).Experience, Is.EqualTo(100));
            Assert.That(ranks.GetRank(TestRankID).Place, Is.EqualTo(1));

            ranks.SetRank(TestRankID, 100000);
            Assert.That(ranks.GetRank(TestRankID).Experience, Is.EqualTo(10000));
            Assert.That(ranks.GetRank(TestRankID).Place, Is.EqualTo(100));
        }

        [Test]
        public void TestRankModify()
        {
            var sim = ContentGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos =>
                {
                    protos.RegisterType<RankPrototype>();
                    protos.LoadString(Prototypes);
                })
                .RegisterEntitySystems(factory => factory.LoadExtraSystemType<RankSystem>())
                .InitializeInstance();

            var ranks = sim.GetEntitySystem<RankSystem>();

            Assert.That(ranks.GetRank(TestRankID).Experience, Is.EqualTo(10000));

            ranks.ModifyRank(TestRankID, 100);
            Assert.That(ranks.GetRank(TestRankID).Experience, Is.EqualTo(9424));

            ranks.ModifyRank(TestRankID, 100000);
            Assert.That(ranks.GetRank(TestRankID).Experience, Is.EqualTo(100));

            ranks.ModifyRank(TestRankID, 100000);
            Assert.That(ranks.GetRank(TestRankID).Experience, Is.EqualTo(100));
        }
    }
}