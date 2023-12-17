using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Maths;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Spells;
using static OpenNefia.Content.CharaInfo.SkillsListControl.SkillsListEntry;

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
      {Content.Prototypes.Protos.Skill.AttrWill}:
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
            var skills = entMan.EnsureComponent<SkillsComponent>(ent);

            Assert.That(spellSys.TryGetKnown(ent, Spell.CureOfJure, out _, out _), Is.False);

            spellSys.GainSpell(ent, Spell.CureOfJure, 25);
            var spell = spells.Ensure(Spell.CureOfJure);
            var skill = skills.Ensure(Content.Prototypes.Protos.Skill.SpellCureOfJure);

            Assert.Multiple(() =>
            {
                Assert.That(skill.Level.Base, Is.EqualTo(1));
                Assert.That(skill.Level.Buffed, Is.EqualTo(1));
                Assert.That(skill.Experience, Is.EqualTo(0));
                Assert.That(skill.Potential, Is.EqualTo(200));
                Assert.That(spell.SpellStock, Is.EqualTo(25));

                Assert.That(spellSys.TryGetKnown(ent, Spell.CureOfJure, out skill, out spell), Is.True);
                Assert.That(skill!.Level.Base, Is.EqualTo(1));
                Assert.That(skill.Level.Buffed, Is.EqualTo(1));
                Assert.That(skill.Experience, Is.EqualTo(0));
                Assert.That(skill.Potential, Is.EqualTo(200));
                Assert.That(spell!.SpellStock, Is.EqualTo(25));
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
            var skills = entMan.GetComponent<SkillsComponent>(ent);

            spellSys.GainSpell(ent, Spell.CureOfJure, 25);
            var spell = spells.Ensure(Spell.CureOfJure);
            var skill = skills.Ensure(Content.Prototypes.Protos.Skill.SpellCureOfJure);

            spellSys.GainSpellExp(ent, Spell.CureOfJure, 5000);

            Assert.Multiple(() =>
            {
                Assert.That(skill.Level.Base, Is.EqualTo(9));
                Assert.That(skill.Level.Buffed, Is.EqualTo(9));
                Assert.That(skill.Experience, Is.EqualTo(695));
                Assert.That(skill.Potential, Is.EqualTo(84));
                Assert.That(spell.SpellStock, Is.EqualTo(25));

                Assert.That(spellSys.BaseLevel(ent, Spell.CureOfJure), Is.EqualTo(9));
                Assert.That(spellSys.Level(ent, Spell.CureOfJure), Is.EqualTo(9));
                Assert.That(spellSys.Potential(ent, Spell.CureOfJure), Is.EqualTo(84));
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
            var skillWill = skills.Ensure(Content.Prototypes.Protos.Skill.AttrWill);

            spellSys.GainSpell(ent, Spell.CureOfJure, 25);
            var spellCureOfJure = spells.Ensure(Spell.CureOfJure);
            var skillCureOfJure = skills.Ensure(Content.Prototypes.Protos.Skill.SpellCureOfJure);

            spellSys.GainSpellExp(ent, Spell.CureOfJure, 5000);

            Assert.Multiple(() =>
            {
                Assert.That(skillWill.Level.Base, Is.EqualTo(9));
                Assert.That(skillWill.Level.Buffed, Is.EqualTo(9));
                Assert.That(skillWill.Experience, Is.EqualTo(829));
                Assert.That(skillWill.Potential, Is.EqualTo(144));

                Assert.That(skillCureOfJure.Level.Base, Is.EqualTo(9));
                Assert.That(skillCureOfJure.Level.Buffed, Is.EqualTo(9));
                Assert.That(skillCureOfJure.Experience, Is.EqualTo(695));
                Assert.That(skillCureOfJure.Potential, Is.EqualTo(84));
                Assert.That(spellCureOfJure.SpellStock, Is.EqualTo(25));

                // Random.
                Assert.That(level.Experience, Is.EqualTo(24823));
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
            var skillWill = skills.Ensure(Content.Prototypes.Protos.Skill.AttrWill);

            spellSys.GainSpell(ent, Spell.CureOfJure, 25);

            spellSys.GainSpellExp(ent, Spell.CureOfJure, 5000, relatedSkillExpDivisor: 2);

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

            spellSys.GainSpell(ent, Spell.CureOfJure, 25);
            var spellCureOfJure = spells.Ensure(Spell.CureOfJure);

            spellSys.GainSpellExp(ent, Spell.CureOfJure, 5000, levelExpDivisor: 20);

            Assert.Multiple(() =>
            {
                // Random.
                Assert.That(level.Experience, Is.EqualTo(2135));
            });
        }
    }
}