using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Maths;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Tests.Skills
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(SkillsSystem))]
    public class SkillsSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestSkillsEntityID = new("TestSkillsEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestSkillsEntityID}
  components:
  - type: Skills
    skills:
      {Skill.AttrStrength}:
        level: 4
        potential: 81
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
                Assert.That(skill.Experience, Is.EqualTo(892));
                Assert.That(skill.Potential, Is.EqualTo(42));
            });
        }
    }
}
