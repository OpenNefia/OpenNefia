using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Damage;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Tests;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Tests.Damage
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(DamageSystem))]
    public class DamageSystemSystem_Tests : OpenNefiaUnitTest
    {

        [Test]
        public void TestDamageSystem_SetLivenessOnDeath()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var parties = sim.GetEntitySystem<IPartySystem>();
            var world = sim.GetEntitySystem<IWorldSystem>();
            var sys = sim.GetEntitySystem<IDamageSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            world.State.GameDate = new GameDateTime(512, 1, 2);

            var player = entGen.SpawnEntity(Protos.Chara.Putit, map.AtPos(0, 0))!.Value;
            sim.Resolve<IGameSessionManager>().Player = player;

            var ally = entGen.SpawnEntity(Protos.Chara.Putit, map.AtPos(0, 1))!.Value;
            parties.TryRecruitAsAlly(player, ally, force: true);

            var villager = entGen.SpawnEntity(Protos.Chara.Shopkeeper, map.AtPos(1, 1))!.Value;
            entMan.EnsureComponent<RoleShopkeeperComponent>(villager);

            var adventurer = entGen.SpawnEntity(Protos.Chara.Warrior, map.AtPos(1, 1))!.Value;
            entMan.EnsureComponent<RoleAdventurerComponent>(adventurer);

            var enemy = entGen.SpawnEntity(Protos.Chara.Putit, map.AtPos(1, 2))!.Value;

            sys.Kill(player);
            sys.Kill(ally);
            sys.Kill(villager);
            sys.Kill(adventurer);
            sys.Kill(enemy);

            Assert.Multiple(() =>
            {
                Assert.That(entMan.GetComponent<CharaComponent>(player).Liveness, Is.EqualTo(CharaLivenessState.Dead));
                Assert.That(entMan.GetComponent<CharaComponent>(ally).Liveness, Is.EqualTo(CharaLivenessState.PetDead));
                Assert.That(entMan.GetComponent<CharaComponent>(villager).Liveness, Is.EqualTo(CharaLivenessState.VillagerDead));
                Assert.That(entMan.GetComponent<CharaComponent>(villager).RevivalDate, Is.EqualTo(new GameDateTime(512, 1, 4)));
                Assert.That(entMan.GetComponent<CharaComponent>(adventurer).Liveness, Is.EqualTo(CharaLivenessState.AdventurerHospital));
                Assert.That(entMan.GetComponent<CharaComponent>(adventurer).RevivalDate, Is.GreaterThan(new GameDateTime(512, 1, 2))); // Randomized
                Assert.That(entMan.GetComponent<CharaComponent>(enemy).Liveness, Is.EqualTo(CharaLivenessState.Dead));
            });
        }
    }
}