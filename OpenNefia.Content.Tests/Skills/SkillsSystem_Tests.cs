using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Maths;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Levels;

namespace OpenNefia.Content.Tests.Skills
{
    /// <summary>
    /// The values in this test originate from a mocked version of 1.22.
    /// (except character experience gain, which is randomized)
    /// </summary>
    [TestFixture, Parallelizable]
    [TestOf(typeof(SkillsSystem))]
    public class SpellsSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestSkillsEntityID = new("TestSkillsEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestSkillsEntityID}
  components:
  - type: Level
    level: 1
    experience: 1000
    experienceToNext: 3024
  - type: Skills
    skills:
      {Skill.AttrStrength}:
        level: 4
        potential: 81
      {Skill.Tactics}:
        level: 1
        potential: 50
";

        [Test]
        public void TestGainSkillExp()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.CreateMapAndSetActive(10, 10);

            var skillsSys = sim.GetEntitySystem<ISkillsSystem>();

            var ent = entMan.SpawnEntity(TestSkillsEntityID, map.AtPos(Vector2i.One));
            var skills = entMan.GetComponent<SkillsComponent>(ent);
            var skill = skills.Ensure(Skill.AttrStrength);

            skillsSys.GainSkillExp(ent, Skill.AttrStrength, 5000);

            Assert.Multiple(() =>
            {
                Assert.That(skill.Level.Base, Is.EqualTo(6));
                Assert.That(skill.Level.Buffed, Is.EqualTo(6));
                Assert.That(skill.Experience, Is.EqualTo(531));
                Assert.That(skill.Potential, Is.EqualTo(64));
            });
        }

        [Test]
        public void TestGainSkillExp_RelatedSkill()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.CreateMapAndSetActive(10, 10);

            var skillsSys = sim.GetEntitySystem<ISkillsSystem>();

            var ent = entMan.SpawnEntity(TestSkillsEntityID, map.AtPos(Vector2i.One));
            var skills = entMan.GetComponent<SkillsComponent>(ent);
            var level = entMan.GetComponent<LevelComponent>(ent);
            var skillTactics = skills.Ensure(Skill.Tactics);
            var skillStr = skills.Ensure(Skill.AttrStrength);

            skillsSys.GainSkillExp(ent, Skill.Tactics, 5000);

            Assert.Multiple(() =>
            {
                Assert.That(skillTactics.Level.Base, Is.EqualTo(3));
                Assert.That(skillTactics.Level.Buffed, Is.EqualTo(3));
                Assert.That(skillTactics.Experience, Is.EqualTo(173));
                Assert.That(skillTactics.Potential, Is.EqualTo(40));

                Assert.That(skillStr.Level.Base, Is.EqualTo(5));
                Assert.That(skillStr.Level.Buffed, Is.EqualTo(5));
                Assert.That(skillStr.Experience, Is.EqualTo(265));
                Assert.That(skillStr.Potential, Is.EqualTo(72));

                // Random.
                Assert.That(level.Experience, Is.EqualTo(4673));
            });
        }

        [Test]
        public void TestGainSkillExp_RelatedSkillExpDivisor()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.CreateMapAndSetActive(10, 10);

            var skillsSys = sim.GetEntitySystem<ISkillsSystem>();

            var ent = entMan.SpawnEntity(TestSkillsEntityID, map.AtPos(Vector2i.One));
            var skills = entMan.GetComponent<SkillsComponent>(ent);
            var skillTactics = skills.Ensure(Skill.Tactics);
            var skillStr = skills.Ensure(Skill.AttrStrength);

            skillsSys.GainSkillExp(ent, Skill.Tactics, 5000, relatedSkillExpDivisor: 2);

            Assert.Multiple(() =>
            {
                Assert.That(skillStr.Level.Base, Is.EqualTo(4));
                Assert.That(skillStr.Level.Buffed, Is.EqualTo(4));
                Assert.That(skillStr.Experience, Is.EqualTo(632));
                Assert.That(skillStr.Potential, Is.EqualTo(81));
            });
        }

        [Test]
        public void TestGainSkillExp_LevelExpDivisor()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.CreateMapAndSetActive(10, 10);

            var skillsSys = sim.GetEntitySystem<ISkillsSystem>();

            var ent = entMan.SpawnEntity(TestSkillsEntityID, map.AtPos(Vector2i.One));
            var skills = entMan.GetComponent<SkillsComponent>(ent);
            var level = entMan.GetComponent<LevelComponent>(ent);
            var skillTactics = skills.Ensure(Skill.Tactics);
            var skillStr = skills.Ensure(Skill.AttrStrength);

            skillsSys.GainSkillExp(ent, Skill.Tactics, 5000, levelExpDivisor: 20);

            Assert.Multiple(() =>
            {
                // Random.
                Assert.That(level.Experience, Is.EqualTo(1175));
            });
        }
    }
}
