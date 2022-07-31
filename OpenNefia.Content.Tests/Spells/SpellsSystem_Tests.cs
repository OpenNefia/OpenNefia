using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Maths;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Spells;

namespace OpenNefia.Content.Tests.Spells
{
    /// <summary>
    /// The values in this test originate from a mocked version of 1.22.
    /// (except character experience gain, which is randomized)
    /// </summary>
    [TestFixture, Parallelizable]
    [TestOf(typeof(SpellSystem))]
    public class SpellsSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestSpellsEntityID = new("TestSpellsEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestSpellsEntityID}
  components:
  - type: Level
    level: 1
    experience: 1000
    experienceToNext: 3024
  - type: Spells
  - type: Skills
    skills:
      {Skill.AttrWill}:
        level: 8
        potential: 161
";

        [Test]
        public void TestGainSpell()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.CreateMapAndSetActive(10, 10);

            var spellSys = sim.GetEntitySystem<ISpellSystem>();

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));
            var spells = entMan.EnsureComponent<SpellsComponent>(ent);

            spellSys.GainSpell(ent, Spell.SpellCureOfJure, 25);
            var spell = spells.Ensure(Spell.SpellCureOfJure);

            Assert.Multiple(() =>
            {
                Assert.That(spell.Level.Base, Is.EqualTo(1));
                Assert.That(spell.Level.Buffed, Is.EqualTo(1));
                Assert.That(spell.Experience, Is.EqualTo(0));
                Assert.That(spell.Potential, Is.EqualTo(200));
                Assert.That(spell.SpellStock, Is.EqualTo(25));
            });
        }

        [Test]
        public void TestGainSpellExp()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.CreateMapAndSetActive(10, 10);

            var spellSys = sim.GetEntitySystem<ISpellSystem>();

            var ent = entMan.SpawnEntity(TestSpellsEntityID, map.AtPos(Vector2i.One));
            var spells = entMan.GetComponent<SpellsComponent>(ent);

            spellSys.GainSpell(ent, Spell.SpellCureOfJure, 25);
            var spell = spells.Ensure(Spell.SpellCureOfJure);

            spellSys.GainSpellExp(ent, Spell.SpellCureOfJure, 5000);

            Assert.Multiple(() =>
            {
                Assert.That(spell.Level.Base, Is.EqualTo(9));
                Assert.That(spell.Level.Buffed, Is.EqualTo(9));
                Assert.That(spell.Experience, Is.EqualTo(695));
                Assert.That(spell.Potential, Is.EqualTo(84));
                Assert.That(spell.SpellStock, Is.EqualTo(25));
            });
        }

        [Test]
        public void TestGainSpellExp_RelatedSkill()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.CreateMapAndSetActive(10, 10);

            var spellSys = sim.GetEntitySystem<ISpellSystem>();

            var ent = entMan.SpawnEntity(TestSpellsEntityID, map.AtPos(Vector2i.One));
            var spells = entMan.GetComponent<SpellsComponent>(ent);
            var skills = entMan.GetComponent<SkillsComponent>(ent);
            var level = entMan.GetComponent<LevelComponent>(ent);
            var skillWill = skills.Ensure(Skill.AttrWill);

            spellSys.GainSpell(ent, Spell.SpellCureOfJure, 25);
            var spellCureOfJure = spells.Ensure(Spell.SpellCureOfJure);

            spellSys.GainSpellExp(ent, Spell.SpellCureOfJure, 5000);

            Assert.Multiple(() =>
            {
                Assert.That(skillWill.Level.Base, Is.EqualTo(9));
                Assert.That(skillWill.Level.Buffed, Is.EqualTo(9));
                Assert.That(skillWill.Experience, Is.EqualTo(829));
                Assert.That(skillWill.Potential, Is.EqualTo(144));

                Assert.That(spellCureOfJure.Level.Base, Is.EqualTo(9));
                Assert.That(spellCureOfJure.Level.Buffed, Is.EqualTo(9));
                Assert.That(spellCureOfJure.Experience, Is.EqualTo(695));
                Assert.That(spellCureOfJure.Potential, Is.EqualTo(84));
                Assert.That(spellCureOfJure.SpellStock, Is.EqualTo(25));

                // Random.
                Assert.That(level.Experience, Is.EqualTo(15696));
            });
        }

        [Test]
        public void TestGainSpellExp_RelatedSkillExpDivisor()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.CreateMapAndSetActive(10, 10);

            var spellSys = sim.GetEntitySystem<ISpellSystem>();

            var ent = entMan.SpawnEntity(TestSpellsEntityID, map.AtPos(Vector2i.One));
            var spells = entMan.GetComponent<SpellsComponent>(ent);
            var skills = entMan.GetComponent<SkillsComponent>(ent);
            var skillWill = skills.Ensure(Skill.AttrWill);

            spellSys.GainSpell(ent, Spell.SpellCureOfJure, 25);

            spellSys.GainSpellExp(ent, Spell.SpellCureOfJure, 5000, relatedSkillExpDivisor: 2);

            Assert.Multiple(() =>
            {
                Assert.That(skillWill.Level.Base, Is.EqualTo(8));
                Assert.That(skillWill.Level.Buffed, Is.EqualTo(8));
                Assert.That(skillWill.Experience, Is.EqualTo(914));
                Assert.That(skillWill.Potential, Is.EqualTo(161));
            });
        }

        [Test]
        public void TestGainSpellExp_LevelExpDivisor()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.CreateMapAndSetActive(10, 10);

            var spellSys = sim.GetEntitySystem<ISpellSystem>();

            var ent = entMan.SpawnEntity(TestSpellsEntityID, map.AtPos(Vector2i.One));
            var spells = entMan.GetComponent<SpellsComponent>(ent);
            var level = entMan.GetComponent<LevelComponent>(ent);

            spellSys.GainSpell(ent, Spell.SpellCureOfJure, 25);
            var spellCureOfJure = spells.Ensure(Spell.SpellCureOfJure);

            spellSys.GainSpellExp(ent, Spell.SpellCureOfJure, 5000, levelExpDivisor: 20);

            Assert.Multiple(() =>
            {
                // Random.
                Assert.That(level.Experience, Is.EqualTo(1701));
            });
        }
    }
}