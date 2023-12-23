using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Tests;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.GameObjects.Components.Tag
{
    [TestFixture]
    [TestOf(typeof(TagComponent))]
    public class TagComponent_Test : ContentUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TagEntityId = new("TagTestDummy");

        // Register these three into the prototype manager
        private static readonly PrototypeId<TagPrototype> StartingTag = new("A");
        private static readonly PrototypeId<TagPrototype> AddedTag = new("EIOU");
        private static readonly PrototypeId<TagPrototype> UnusedTag = new("E");

        // Do not register this one
        private static readonly PrototypeId<TagPrototype> UnregisteredTag = new("AAAAAAAAA");

        private static readonly string Prototypes = $@"
- type: Tag
  id: {StartingTag}

- type: Tag
  id: {AddedTag}

- type: Tag
  id: {UnusedTag}

- type: Entity
  id: {TagEntityId}
  name: {TagEntityId}
  components:
  - type: Tag
    tags:
    - {StartingTag}";

        [OneTimeSetUp]
        public void SetUp()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            prototypeManager.RegisterType<EntityPrototype>();
            prototypeManager.RegisterType<TagPrototype>();
            prototypeManager.ResolveResults();
        }

        [Test]
        public void TagComponentTest()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterComponents(compMan => compMan.RegisterClass<TagComponent>())
                .RegisterPrototypes(protoMan =>
                {
                    protoMan.RegisterType<TagPrototype>();
                    protoMan.LoadString(Prototypes);
                })
                .InitializeInstance();

            var map = sim.CreateMapAndSetActive(50, 50);

            var sMapManager = sim.Resolve<IMapManager>();
            var sEntityManager = sim.Resolve<IEntityManager>();
            var sPrototypeManager = sim.Resolve<IPrototypeManager>();

            EntityUid sTagDummy = default;
            TagComponent sTagComponent = null!;

            sTagDummy = sEntityManager.SpawnEntity(TagEntityId, map.AtPos(Vector2i.Zero));
            sTagComponent = sEntityManager.GetComponent<TagComponent>(sTagDummy);

            // Has one tag, the starting tag
            Assert.That(sTagComponent.Tags.Count, Is.EqualTo(1));
            sPrototypeManager.Index(StartingTag);
            Assert.That(sTagComponent.Tags, Contains.Item(StartingTag));

            // Single
            Assert.True(sTagComponent.HasTag(StartingTag));

            // Any
            Assert.True(sTagComponent.HasAnyTag(StartingTag));

            // All
            Assert.True(sTagComponent.HasAllTags(StartingTag));

            // Does not have the added tag
            var addedTagPrototype = sPrototypeManager.Index(AddedTag);
            Assert.That(sTagComponent.Tags, Does.Not.Contains(addedTagPrototype));

            // Single
            Assert.False(sTagComponent.HasTag(AddedTag));

            // Any
            Assert.False(sTagComponent.HasAnyTag(AddedTag));

            // All
            Assert.False(sTagComponent.HasAllTags(AddedTag));

            // Does not have the unused tag
            var unusedTagPrototype = sPrototypeManager.Index(UnusedTag);
            Assert.That(sTagComponent.Tags, Does.Not.Contains(unusedTagPrototype));

            // Single
            Assert.False(sTagComponent.HasTag(UnusedTag));

            // Any
            Assert.False(sTagComponent.HasAnyTag(UnusedTag));

            // All
            Assert.False(sTagComponent.HasAllTags(UnusedTag));

            // Throws when checking for an unregistered tag
            Assert.Throws<UnknownPrototypeException>(() =>
            {
                sPrototypeManager.Index(UnregisteredTag);
            });

            // Single
            Assert.Throws<UnknownPrototypeException>(() =>
            {
                sTagComponent.HasTag(UnregisteredTag);
            });

            // Any
            Assert.Throws<UnknownPrototypeException>(() =>
            {
                sTagComponent.HasAnyTag(UnregisteredTag);
            });

            // All
            Assert.Throws<UnknownPrototypeException>(() =>
            {
                sTagComponent.HasAllTags(UnregisteredTag);
            });

            // Cannot add the starting tag again
            Assert.That(sTagComponent.AddTag(StartingTag), Is.False);
            Assert.That(sTagComponent.AddTags(StartingTag, StartingTag), Is.False);
            Assert.That(sTagComponent.AddTags(new List<PrototypeId<TagPrototype>> { StartingTag, StartingTag }), Is.False);

            // Has the starting tag
            Assert.That(sTagComponent.HasTag(StartingTag), Is.True);
            Assert.That(sTagComponent.HasAllTags(StartingTag, StartingTag), Is.True);
            Assert.That(sTagComponent.HasAllTags(new List<PrototypeId<TagPrototype>> { StartingTag, StartingTag }), Is.True);
            Assert.That(sTagComponent.HasAnyTag(StartingTag, StartingTag), Is.True);
            Assert.That(sTagComponent.HasAnyTag(new List<PrototypeId<TagPrototype>> { StartingTag, StartingTag }), Is.True);

            // Does not have the added tag yet
            Assert.That(sTagComponent.HasTag(AddedTag), Is.False);
            Assert.That(sTagComponent.HasAllTags(AddedTag, AddedTag), Is.False);
            Assert.That(sTagComponent.HasAllTags(new List<PrototypeId<TagPrototype>> { AddedTag, AddedTag }), Is.False);
            Assert.That(sTagComponent.HasAnyTag(AddedTag, AddedTag), Is.False);
            Assert.That(sTagComponent.HasAnyTag(new List<PrototypeId<TagPrototype>> { AddedTag, AddedTag }), Is.False);

            // Has a combination of the two tags
            Assert.That(sTagComponent.HasAnyTag(StartingTag, AddedTag), Is.True);
            Assert.That(sTagComponent.HasAnyTag(new List<PrototypeId<TagPrototype>> { StartingTag, AddedTag }), Is.True);

            // Does not have both tags
            Assert.That(sTagComponent.HasAllTags(StartingTag, AddedTag), Is.False);
            Assert.That(sTagComponent.HasAllTags(new List<PrototypeId<TagPrototype>> { StartingTag, AddedTag }), Is.False);

            // Cannot remove a tag that does not exist
            Assert.That(sTagComponent.RemoveTag(AddedTag), Is.False);
            Assert.That(sTagComponent.RemoveTags(AddedTag, AddedTag), Is.False);
            Assert.That(sTagComponent.RemoveTags(new List<PrototypeId<TagPrototype>> { AddedTag, AddedTag }), Is.False);

            // Can add the new tag
            Assert.That(sTagComponent.AddTag(AddedTag), Is.True);

            // Cannot add it twice
            Assert.That(sTagComponent.AddTag(AddedTag), Is.False);

            // Cannot add existing tags
            Assert.That(sTagComponent.AddTags(StartingTag, AddedTag), Is.False);
            Assert.That(sTagComponent.AddTags(new List<PrototypeId<TagPrototype>> { StartingTag, AddedTag }), Is.False);

            // Now has two tags
            Assert.That(sTagComponent.Tags.Count, Is.EqualTo(2));

            // Has both tags
            Assert.That(sTagComponent.HasTag(StartingTag), Is.True);
            Assert.That(sTagComponent.HasTag(AddedTag), Is.True);
            Assert.That(sTagComponent.HasAllTags(StartingTag, StartingTag), Is.True);
            Assert.That(sTagComponent.HasAllTags(AddedTag, StartingTag), Is.True);
            Assert.That(sTagComponent.HasAllTags(new List<PrototypeId<TagPrototype>> { StartingTag, AddedTag }), Is.True);
            Assert.That(sTagComponent.HasAllTags(new List<PrototypeId<TagPrototype>> { AddedTag, StartingTag }), Is.True);
            Assert.That(sTagComponent.HasAnyTag(StartingTag, AddedTag), Is.True);
            Assert.That(sTagComponent.HasAnyTag(AddedTag, StartingTag), Is.True);

            // Remove the existing starting tag
            Assert.That(sTagComponent.RemoveTag(StartingTag), Is.True);

            // Remove the existing added tag
            Assert.That(sTagComponent.RemoveTags(AddedTag, AddedTag), Is.True);

            // No tags left to remove
            Assert.That(sTagComponent.RemoveTags(new List<PrototypeId<TagPrototype>> { StartingTag, AddedTag }), Is.False);

            // No tags left in the component
            Assert.That(sTagComponent.Tags, Is.Empty);
        }
    }
}
