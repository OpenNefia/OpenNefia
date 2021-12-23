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
    [TestOf(typeof(EntityLookup))]
    public class EntityLookup_Tests : OpenNefiaUnitTest
    {
        [Test]
        public void TestGetLiveEntitiesAtPos()
        {
            var simulation = GameSimulation
                .NewSimulation()
                .InitializeInstance();

            var map = simulation.CreateMapAndSetActive(50, 50);

            var entMan = simulation.Resolve<IEntityManager>();
            var entSysMan = simulation.Resolve<IEntitySystemManager>();
            var lookup = entSysMan.GetEntitySystem<IEntityLookup>();

            var pos = Vector2i.Zero;

            var entMap = entMan.GetEntity(simulation.ActiveMap!.MapEntityUid);

            // "Alive" means the entity is considered the primary entity on the tile.
            // In HSP Elona, each map tile can only hold a single character ID for
            // positional querying purposes; this emulates that behavior.
            var entAlive = entMan.SpawnEntity(null, map.AtPos(pos));
            var metaAlive = entMan.GetComponent<MetaDataComponent>(entAlive.Uid);
            metaAlive.Liveness = EntityGameLiveness.Alive;

            // Child entities don't count as being in the map.
            var entChild = entMan.SpawnEntity(null, new EntityCoordinates(entAlive.Uid, Vector2i.Zero));
            var metaChild = entMan.GetComponent<MetaDataComponent>(entChild.Uid);
            metaChild.Liveness = EntityGameLiveness.Alive;

            // "Hidden" means the entity is not visible in the map and cannot be targeted,
            // but should not be removed from the map, such as for dead allies.
            var entHidden = entMan.SpawnEntity(null, map.AtPos(pos));
            var metaHidden = entMan.GetComponent<MetaDataComponent>(entHidden.Uid);
            metaHidden.Liveness = EntityGameLiveness.Hidden;

            // "DeadAndBuried" means the entity can be removed at any time.
            var entDead = entMan.SpawnEntity(null, map.AtPos(pos));
            var metaDead = entMan.GetComponent<MetaDataComponent>(entDead.Uid);
            metaDead.Liveness = EntityGameLiveness.DeadAndBuried;

            var ents = lookup.GetAllEntitiesInMap(map.Id);

            Assert.That(ents, Is.EquivalentTo(new[]
            {
                entAlive,
                entChild,
                entHidden,
                entDead,
                entMap
            }));

            ents = lookup.GetEntitiesDirectlyInMap(map.Id);

            Assert.That(ents, Is.EquivalentTo(new[]
            {
                entAlive,
                entHidden,
                entDead,
                entMap
            }));

            ents = lookup.GetLiveEntitiesAtPos(map.AtPos(pos));

            Assert.That(ents, Is.EquivalentTo(new[]
            {
                entAlive,
                entMap
            }));
        }
    }
}
