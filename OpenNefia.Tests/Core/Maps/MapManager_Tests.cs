using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Maps
{
    [TestFixture]
    [TestOf(typeof(MapManager))]
    public class MapManager_Tests : OpenNefiaUnitTest
    {
        [Test]
        public void TestGetLiveEntitiesAtPos()
        {
            var simulation = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory => factory.LoadExtraSystemType<EntityLookup>())
                .InitializeInstance();

            var map = new Map(50, 50);
            simulation.SetActiveMap(map);

            var entMan = simulation.Resolve<IEntityManager>();
            var entSysMan = simulation.Resolve<IEntitySystemManager>();
            var lookup = entSysMan.GetEntitySystem<IEntityLookup>();

            var pos = Vector2i.Zero;

            // "Alive" means the entity is considered the primary entity on the tile.
            // In HSP Elona, each map tile can only hold a single character ID for
            // positional querying purposes; this emulates that behavior.
            var entAlive = entMan.SpawnEntity(null, map, pos);
            var metaAlive = entMan.GetComponent<MetaDataComponent>(entAlive.Uid);
            metaAlive.Liveness = EntityGameLiveness.Alive;

            // "AliveSecondary" means the entity can be targeted, but shouldn't
            // be used for certain calculations like AoE spells. This will be used
            // for Riding/Tag Team allies.
            var entAliveSecondary = entMan.SpawnEntity(null, map, pos);
            var metaAliveSecondary = entMan.GetComponent<MetaDataComponent>(entAliveSecondary.Uid);
            metaAliveSecondary.Liveness = EntityGameLiveness.AliveSecondary;

            // "Hidden" means the entity is not visible in the map and cannot be targeted,
            // but should not be removed from the map, such as for dead allies.
            var entHidden = entMan.SpawnEntity(null, map, pos);
            var metaHidden = entMan.GetComponent<MetaDataComponent>(entHidden.Uid);
            metaHidden.Liveness = EntityGameLiveness.Hidden;

            // "DeadAndBuried" means the entity can be removed at any time.
            var entDead = entMan.SpawnEntity(null, map, pos);
            var metaDead = entMan.GetComponent<MetaDataComponent>(entDead.Uid);
            metaDead.Liveness = EntityGameLiveness.DeadAndBuried;

            var ents = lookup.GetLiveEntitiesAtPos(map.AtPos(pos));

            Assert.That(ents, Is.EquivalentTo(new[]
            {
                entAlive,
                entAliveSecondary
            }));
        }
    }
}
