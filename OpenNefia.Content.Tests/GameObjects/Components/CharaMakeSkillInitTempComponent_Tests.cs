using NUnit.Framework;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests.GameObjects.Components
{
    public class CharaMakeSkillInitTempComponent_Tests : ContentUnitTest
    {
        [Test]
        public void CharaMakeSkillOverrideTest()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var _entityManager = sim.Resolve<IEntityManager>();
            var _mapManager = sim.Resolve<IMapManager>();

            var globalMap = _mapManager.GetMap(MapId.Global);
            var globalMapSpatial = _entityManager.GetComponent<SpatialComponent>(globalMap.MapEntityUid);


            var playerEntity = _entityManager.CreateEntityUninitialized(Protos.Chara.Player);
            var areaSpatial = _entityManager.GetComponent<SpatialComponent>(playerEntity);
            areaSpatial.AttachParent(globalMapSpatial);


            var entityGen = EntitySystem.Get<IEntityGen>();
            _entityManager.InitializeComponents(playerEntity);
            _entityManager.StartComponents(playerEntity);

            var charaMakeSkillInit = _entityManager.AddComponent<CharaMakeSkillInitTempComponent>(playerEntity);
            charaMakeSkillInit.Skills[Protos.Skill.StatStrength] = 777;
            charaMakeSkillInit.Skills[Protos.Skill.StatMagic] = 888;
            charaMakeSkillInit.Skills[Protos.Skill.StatDexterity] = 999;

            entityGen.FireGeneratedEvent(playerEntity);
            var skills = _entityManager.GetComponent<SkillsComponent>(playerEntity);
            Assert.That(skills.Skills[Protos.Skill.StatStrength].Level == 777);
            Assert.That(skills.Skills[Protos.Skill.StatMagic].Level == 888);
            Assert.That(skills.Skills[Protos.Skill.StatDexterity].Level == 999);
            Assert.That(!_entityManager.TryGetComponent<CharaMakeSkillInitTempComponent>(playerEntity, out _));
        }
    }
}
