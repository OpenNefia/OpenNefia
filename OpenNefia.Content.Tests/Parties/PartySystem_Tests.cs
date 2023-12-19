using NUnit.Framework;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Parties;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests.Parties
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(PartySystem))]
    public class PartySystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEquipmentID = new("TestEquipment");
        private static readonly PrototypeId<EntityPrototype> TestCharaID = new("TestChara");

        private static readonly string Prototypes = @$"
- type: Entity
  id: {TestCharaID}
  parent: BaseChara
  components:
  - type: Party
  - type: Chara
    race: Elona.Slug
    class: Elona.Predator
  - type: Faction
    relationToPlayer: Hate
";

        private ISimulation SimulationFactory()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
                .InitializeInstance();

            return sim;
        }

        [Test]
        public void TestParties_RecruitAsAlly()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var entLeader = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entAlly = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            var partyLeader = entMan.GetComponent<PartyComponent>(entLeader);
            var partyAlly = entMan.GetComponent<PartyComponent>(entAlly);

            Assert.Multiple(() =>
            {
                Assert.That(parties.EnumerateMembers(entLeader), Is.EquivalentTo(Enumerable.Empty<EntityUid>()));
                Assert.That(parties.EnumerateMembers(entAlly), Is.EquivalentTo(Enumerable.Empty<EntityUid>()));

                Assert.That(parties.TryRecruitAsAlly(entLeader, entAlly), Is.True);

                var members = parties.EnumerateMembers(entLeader).ToList();
                Assert.That(members!.Count, Is.EqualTo(2));
                Assert.That(members.Contains(entLeader));
                Assert.That(members.Contains(entAlly));
                Assert.That(parties.EnumerateMembers(entAlly), Is.EquivalentTo(members));
                Assert.That(partyLeader.PartyID, Is.EqualTo(0));
                Assert.That(partyAlly.PartyID, Is.EqualTo(0));
            });
        }

        [Test]
        public void TestParties_RecruitAsAlly_Invalid()
        {
            var sim = SimulationFactory();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var entLeader1 = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entLeader2 = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entAlly1 = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entAlly2 = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(parties.TryRecruitAsAlly(entLeader1, entLeader1), Is.False, "Trying to recruit self");
                Assert.That(parties.TryRecruitAsAlly(entLeader1, EntityUid.Invalid), Is.False, "Trying to recruit invalid entity");
                Assert.That(parties.TryRecruitAsAlly(EntityUid.Invalid, entAlly1), Is.False, "Trying to recruit invalid entity");

                Assert.That(parties.TryRecruitAsAlly(entLeader1, entAlly1), Is.True);
                Assert.That(parties.TryRecruitAsAlly(entLeader2, entAlly2), Is.True);

                Assert.That(parties.TryRecruitAsAlly(entLeader1, entAlly2), Is.False, "Entity is already in another party");
                Assert.That(parties.TryRecruitAsAlly(entLeader1, entAlly2), Is.False, "Entity is already in same party");
                Assert.That(parties.TryRecruitAsAlly(entLeader1, entAlly2), Is.False, "Entity already has party");
            });
        }

        [Test]
        public void TestParties_RecruitAsAlly_Faction()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var entLeader = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entAlly = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            var factionLeader = entMan.GetComponent<FactionComponent>(entLeader);
            var factionAlly = entMan.GetComponent<FactionComponent>(entAlly);

            factionLeader.RelationToPlayer = Relation.Ally;
            factionAlly.RelationToPlayer = Relation.Hate;

            Assert.Multiple(() =>
            {
                Assert.That(parties.TryRecruitAsAlly(entLeader, entAlly), Is.True);

                Assert.That(factionLeader.RelationToPlayer, Is.EqualTo(Relation.Ally));
                Assert.That(factionAlly.RelationToPlayer, Is.EqualTo(Relation.Ally));
            });
        }

        [Test]
        public void TestParties_IsPartyLeaderOf()
        {
            var sim = SimulationFactory();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var entLeader = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entAlly = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(parties.IsPartyLeaderOf(entLeader, entAlly), Is.False);
                Assert.That(parties.IsPartyLeaderOf(entAlly, entLeader), Is.False);

                Assert.That(parties.TryRecruitAsAlly(entLeader, entAlly), Is.True);

                Assert.That(parties.IsPartyLeaderOf(entLeader, entAlly), Is.True);
                Assert.That(parties.IsPartyLeaderOf(entAlly, entLeader), Is.False);
            });
        }

        [Test]
        public void TestParties_TryGetLeader()
        {
            var sim = SimulationFactory();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var entLeader = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entAlly = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(parties.TryGetLeader(entLeader, out var _), Is.False);
                Assert.That(parties.TryGetLeader(entAlly, out var _), Is.False);

                Assert.That(parties.TryRecruitAsAlly(entLeader, entAlly), Is.True);

                Assert.That(parties.TryGetLeader(entLeader, out var resultLeader), Is.True);
                Assert.That(resultLeader!.Value, Is.EqualTo(entLeader));
                Assert.That(parties.TryGetLeader(entAlly, out resultLeader), Is.True);
                Assert.That(resultLeader!.Value, Is.EqualTo(entLeader));
            });
        }

        [Test]
        public void TestParties_TryLeaveParty()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var entLeader = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entAlly = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            var partyLeader = entMan.GetComponent<PartyComponent>(entLeader);
            var partyAlly = entMan.GetComponent<PartyComponent>(entAlly);

            Assert.Multiple(() =>
            {
                Assert.That(parties.RemoveFromCurrentParty(entLeader), Is.False);
                Assert.That(parties.RemoveFromCurrentParty(entAlly), Is.False);

                Assert.That(parties.TryRecruitAsAlly(entLeader, entAlly), Is.True);

                Assert.That(partyLeader.PartyID, Is.EqualTo(0));
                Assert.That(partyAlly.PartyID, Is.EqualTo(0));

                Assert.That(parties.IsPartyLeaderOf(entLeader, entAlly), Is.True);
                Assert.That(parties.RemoveFromCurrentParty(entAlly), Is.True);
                Assert.That(parties.IsPartyLeaderOf(entLeader, entAlly), Is.False);

                Assert.That(partyLeader.PartyID, Is.EqualTo(0));
                Assert.That(partyAlly.PartyID, Is.Null);

                Assert.That(parties.TryGetLeader(entLeader, out var resultLeader), Is.True);
                Assert.That(resultLeader!.Value, Is.EqualTo(entLeader));
                Assert.That(parties.TryGetLeader(entAlly, out _), Is.False);
            });
        }

        [Test]
        public void TestParties_TryLeaveParty_Leader()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var entLeader = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entAlly = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            var partyLeader = entMan.GetComponent<PartyComponent>(entLeader);
            var partyAlly = entMan.GetComponent<PartyComponent>(entAlly);

            Assert.Multiple(() =>
            {
                Assert.That(parties.RemoveFromCurrentParty(entLeader), Is.False);
                Assert.That(parties.RemoveFromCurrentParty(entAlly), Is.False);

                Assert.That(parties.TryRecruitAsAlly(entLeader, entAlly), Is.True);

                Assert.That(partyLeader.PartyID, Is.EqualTo(0));
                Assert.That(partyAlly.PartyID, Is.EqualTo(0));

                Assert.That(parties.IsPartyLeaderOf(entLeader, entAlly), Is.True);
                Assert.That(parties.RemoveFromCurrentParty(entLeader), Is.True);
                Assert.That(parties.IsPartyLeaderOf(entLeader, entAlly), Is.False);

                Assert.That(partyLeader.PartyID, Is.Null);
                Assert.That(partyAlly.PartyID, Is.EqualTo(0));

                Assert.That(parties.TryGetLeader(entAlly, out var resultLeader), Is.True);
                Assert.That(resultLeader!.Value, Is.EqualTo(entAlly));
            });
        }

        [Test]
        public void TestParties_OnEntityDeleted()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var entLeader = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entAlly = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            var partyLeader = entMan.GetComponent<PartyComponent>(entLeader);
            var partyAlly = entMan.GetComponent<PartyComponent>(entAlly);

            Assert.That(parties.TryRecruitAsAlly(entLeader, entAlly), Is.True);

            Assert.That(partyLeader.PartyID, Is.EqualTo(0));
            Assert.That(partyAlly.PartyID, Is.EqualTo(0));

            entMan.DeleteEntity(entLeader);

            Assert.Multiple(() =>
            {
                Assert.That(partyLeader.PartyID, Is.EqualTo(0));
                Assert.That(partyAlly.PartyID, Is.EqualTo(0));

                var members = parties.EnumerateMembers(entAlly).ToList();
                Assert.That(members.Count(), Is.EqualTo(1));
                Assert.That(members.Contains(entAlly), Is.True);

                Assert.That(parties.TryGetLeader(entAlly, out var resultLeader), Is.True);
                Assert.That(resultLeader!.Value, Is.EqualTo(entAlly));
            });
        }
    }
}