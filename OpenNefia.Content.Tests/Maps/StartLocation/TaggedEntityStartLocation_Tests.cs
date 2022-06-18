using NUnit.Framework;
using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests.Maps.StartLocation
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(TaggedEntityMapLocation))]
    public class TaggedEntityStartLocation_Tests
    {
        private static readonly PrototypeId<TagPrototype> TestTagID = new("TestTag");

        private static readonly string Prototypes = @$"
- type: Tag
  id: {TestTagID}
";

        /// <summary>
        /// Tests that when entering a map using a <see cref="TaggedEntityMapLocation"/>,
        /// the entities with <see cref="TagComponent"/> in the map
        /// are referenced.
        /// </summary>
        [Test]
        public void TestTaggedEntityMapLocation()
        {
            var sim = ContentGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protoFactory => {
                    protoFactory.RegisterType<TagPrototype>();
                    protoFactory.LoadString(Prototypes);
                })
               .RegisterEntitySystems(factory =>
               {
                   factory.LoadExtraSystemType<TagSystem>();
               })
                .RegisterComponents(factory => factory.RegisterClass<TagComponent>())
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();

            var map = sim.CreateMapAndSetActive(10, 10);
            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));

            var expectedPos = new Vector2i(3, 4);

            var taggedEnt = entMan.SpawnEntity(null, map.AtPos(expectedPos));
            var tagComp = entMan.EnsureComponent<TagComponent>(taggedEnt);
            tagComp.AddTag(TestTagID);

            var loc = new TaggedEntityMapLocation(TestTagID);
            Assert.That(loc.GetStartPosition(ent, map), Is.EqualTo(expectedPos));
        }
    }
}
