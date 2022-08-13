using NUnit.Framework;
using OpenNefia.Content.CharaMake;
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
    public class CharaMakeGenArgs_Tests : ContentUnitTest
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

            var charaGenArgs = new CharaMakeGenArgs();

            charaGenArgs.InitialSkills[Protos.Skill.AttrStrength] = 777;
            charaGenArgs.InitialSkills[Protos.Skill.AttrMagic] = 888;
            charaGenArgs.InitialSkills[Protos.Skill.AttrDexterity] = 999;

            entityGen.FireGeneratingEvents(playerEntity, args: EntityGenArgSet.Make(charaGenArgs));
            var skills = _entityManager.GetComponent<SkillsComponent>(playerEntity);
            Assert.That(skills.Skills[Protos.Skill.AttrStrength].Level.Base == 777);
            Assert.That(skills.Skills[Protos.Skill.AttrMagic].Level.Base == 888);
            Assert.That(skills.Skills[Protos.Skill.AttrDexterity].Level.Base == 999);
        }
    }
}