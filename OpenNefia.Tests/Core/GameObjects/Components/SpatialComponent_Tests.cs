using System.IO;
using System.Reflection;
using Moq;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Timing;

namespace OpenNefia.Tests.Core.GameObjects.Components
{
    [TestFixture]
    [TestOf(typeof(SpatialComponent))]
    class SpatialComponent_Tests : OpenNefiaUnitTest
    {
        private IEntityManagerInternal EntityManager = default!;
        private IMapManager MapManager = default!;
        private PrototypeId<EntityPrototype> IdDummy = new("dummy");
        private PrototypeId<EntityPrototype> IdMapDummy = new("mapDummy");

        const string PROTOTYPES = @"
- type: Entity
  id: dummy
  components:
  - type: Spatial

- type: Entity
  id: mapDummy
  components:
  - type: Spatial
  - type: Map
    mapId: 123
";

        private IMap MapA = default!;
        private EntityUid MapEntityAId;
        private IMap MapB = default!;
        private EntityUid MapEntityBId;

        private static readonly EntityCoordinates InitialPos = new(new EntityUid(1), (0, 0));

        [OneTimeSetUp]
        public void Setup()
        {
            IoCManager.Resolve<IComponentFactory>().FinishRegistration();

            EntityManager = IoCManager.Resolve<IEntityManagerInternal>();
            MapManager = IoCManager.Resolve<IMapManager>();

            IoCManager.Resolve<ISerializationManager>().Initialize();
            var manager = IoCManager.Resolve<IPrototypeManager>();
            manager.RegisterType(typeof(EntityPrototype));
            manager.LoadFromStream(new StringReader(PROTOTYPES));
            manager.Resync();

            MapA = MapManager.CreateMap(50, 50);
            MapEntityAId = MapManager.GetMapEntity(MapA.Id).Uid;
            MapB = MapManager.CreateMap(50, 50);
            MapEntityBId = MapManager.GetMapEntity(MapB.Id).Uid;
        }

        [Test]
        public void ParentMapSwitchTest()
        {
            // two entities
            var parent = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var child = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;

            var parentTrans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(parent);
            var childTrans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(child);

            // that are not on the same map
            parentTrans.Coordinates = new EntityCoordinates(MapEntityAId, (5, 5));
            childTrans.Coordinates = new EntityCoordinates(MapEntityBId, (4, 4));

            // if they are parented, the child keeps its world position, but moves to the parents map
            childTrans.AttachParent(parentTrans);


            Assert.Multiple(() =>
            {
                Assert.That(childTrans.MapID, Is.EqualTo(parentTrans.MapID));
                Assert.That(childTrans.Coordinates, Is.EqualTo(new EntityCoordinates(parentTrans.OwnerUid, (-1, -1))));
                Assert.That(childTrans.WorldPosition, Is.EqualTo(new Vector2i(4, 4)));
            });

            // move the parent, and the child should move with it
            childTrans.LocalPosition = new Vector2i(6, 6);
            parentTrans.WorldPosition = new Vector2i(-8, -8);

            Assert.That(childTrans.WorldPosition, Is.EqualTo(new Vector2i(-2, -2)));

            // if we detach parent, the child should be left where it was, still relative to parents grid
            var oldLpos = new Vector2i(-2, -2);
            var oldWpos = childTrans.WorldPosition;

            childTrans.AttachToMap();

            // the gridId won't match, because we just detached from the grid entity

            Assert.Multiple(() =>
            {
                Assert.That(childTrans.Coordinates.Position, Is.EqualTo(oldLpos));
                Assert.That(childTrans.WorldPosition, Is.EqualTo(oldWpos));
            });
        }

        /// <summary>
        ///     Tests that a child entity does not move when attaching to a parent.
        /// </summary>
        [Test]
        public void ParentAttachMoveTest()
        {
            // Arrange
            var parent = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var child = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var parentTrans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(parent);
            var childTrans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(child);
            parentTrans.WorldPosition = new Vector2i(5, 5);
            childTrans.WorldPosition = new Vector2i(6, 6);

            // Act
            var oldWpos = childTrans.WorldPosition;
            childTrans.AttachParent(parentTrans);
            var newWpos = childTrans.WorldPosition;

            // Assert
            Assert.That(oldWpos == newWpos);
        }

        /// <summary>
        ///     Tests that a child entity does not move when attaching to a parent.
        /// </summary>
        [Test]
        public void ParentDoubleAttachMoveTest()
        {
            // Arrange
            var parent = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var childOne = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var childTwo = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var parentTrans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(parent);
            var childOneTrans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(childOne);
            var childTwoTrans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(childTwo);
            parentTrans.WorldPosition = new Vector2i(1, 1);
            childOneTrans.WorldPosition = new Vector2i(2, 2);
            childTwoTrans.WorldPosition = new Vector2i(3, 3);

            // Act
            var oldWpos = childOneTrans.WorldPosition;
            childOneTrans.AttachParent(parentTrans);
            var newWpos = childOneTrans.WorldPosition;
            Assert.That(oldWpos, Is.EqualTo(newWpos));

            oldWpos = childTwoTrans.WorldPosition;
            childTwoTrans.AttachParent(parentTrans);
            newWpos = childTwoTrans.WorldPosition;
            Assert.That(oldWpos, Is.EqualTo(newWpos));

            oldWpos = childTwoTrans.WorldPosition;
            childTwoTrans.AttachParent(childOneTrans);
            newWpos = childTwoTrans.WorldPosition;
            Assert.That(oldWpos, Is.EqualTo(newWpos));
        }

        /// <summary>
        ///     Tests to see if parenting multiple entities with WorldPosition places the leaf properly.
        /// </summary>
        [Test]
        public void PositionCompositionTest()
        {
            // Arrange
            var node1 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var node2 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var node3 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var node4 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;

            var node1Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node1);
            var node2Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node2);
            var node3Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node3);
            var node4Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node4);

            node1Trans.WorldPosition = new Vector2i(0, 0);
            node2Trans.WorldPosition = new Vector2i(1, 1);
            node3Trans.WorldPosition = new Vector2i(2, 2);
            node4Trans.WorldPosition = new Vector2i(0, 2);

            node2Trans.AttachParent(node1Trans);
            node3Trans.AttachParent(node2Trans);
            node4Trans.AttachParent(node3Trans);

            //Act
            node1Trans.LocalPosition = new Vector2i(4, 4);

            //Assert
            var result = node4Trans.WorldPosition;

            Assert.That(result, Is.EqualTo(new Vector2i(4, 6)));
        }

        /// <summary>
        ///     Tests to see if setting the world position of a child causes position rounding errors.
        /// </summary>
        [Test]
        public void ParentLocalPositionRoundingErrorTest()
        {
            // Arrange
            var node1 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var node2 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var node3 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;

            var node1Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node1);
            var node2Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node2);
            var node3Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node3);

            node1Trans.WorldPosition = new Vector2i(0, 0);
            node2Trans.WorldPosition = new Vector2i(1, 1);
            node3Trans.WorldPosition = new Vector2i(2, 2);

            node2Trans.AttachParent(node1Trans);
            node3Trans.AttachParent(node2Trans);

            // Act
            var oldWpos = node3Trans.WorldPosition;

            for (var i = 0; i < 10000; i++)
            {
                var dx = i % 2 == 0 ? 5 : -5;
                node1Trans.LocalPosition += new Vector2i(dx, dx);
                node2Trans.LocalPosition += new Vector2i(dx, dx);
                node3Trans.LocalPosition += new Vector2i(dx, dx);
            }

            var newWpos = node3Trans.WorldPosition;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(MathHelper.CloseToPercent(oldWpos.X, newWpos.Y), $"{oldWpos.X} should be {newWpos.Y}");
                Assert.That(MathHelper.CloseToPercent(oldWpos.Y, newWpos.Y), newWpos.ToString);
            });
        }

        /// <summary>
        ///     Tests that the world and inverse world Spatials are built properly.
        /// </summary>
        [Test]
        public void TreeComposeWorldMatricesTest()
        {
            // Arrange
            var control = Matrix3.Identity;

            var node1 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var node2 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var node3 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var node4 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;

            var node1Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node1);
            var node2Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node2);
            var node3Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node3);
            var node4Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node4);

            node1Trans.WorldPosition = new Vector2i(0, 0);
            node2Trans.WorldPosition = new Vector2i(1, 1);
            node3Trans.WorldPosition = new Vector2i(2, 2);
            node4Trans.WorldPosition = new Vector2i(0, 2);

            node2Trans.AttachParent(node1Trans);
            node3Trans.AttachParent(node2Trans);
            node4Trans.AttachParent(node3Trans);

            //Act
            node1Trans.WorldPosition = new Vector2i(1, 1);

            var worldMat = node4Trans.WorldMatrix;
            var invWorldMat = node4Trans.InvWorldMatrix;

            Matrix3.Multiply(ref worldMat, ref invWorldMat, out var leftVerifyMatrix);
            Matrix3.Multiply(ref invWorldMat, ref worldMat, out var rightVerifyMatrix);

            //Assert

            Assert.Multiple(() =>
            {
                // these should be the same (A × A-1 = A-1 × A = I)
                Assert.That(leftVerifyMatrix, new ApproxEqualityConstraint(rightVerifyMatrix));

                // verify matrix == identity matrix (or very close to because float precision)
                Assert.That(leftVerifyMatrix, new ApproxEqualityConstraint(control));
            });
        }

        /// <summary>
        ///     Test that, in a chain A -> B -> C, if A is moved C's world position correctly updates.
        /// </summary>
        [Test]
        public void MatrixUpdateTest()
        {
            var node1 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var node2 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;
            var node3 = EntityManager.SpawnEntity(IdDummy, InitialPos).Uid;

            var node1Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node1);
            var node2Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node2);
            var node3Trans = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(node3);

            node2Trans.AttachParent(node1Trans);
            node3Trans.AttachParent(node2Trans);

            node3Trans.LocalPosition = new Vector2i(5, 5);
            node2Trans.LocalPosition = new Vector2i(5, 5);
            node1Trans.LocalPosition = new Vector2i(5, 5);

            Assert.That(node3Trans.WorldPosition, Is.EqualTo(new Vector2i(15, 15)));
        }

        [Test]
        public void TestMapIdInitOrder()
        {
            // Tests that if a child initializes before its parent, MapID still gets initialized correctly.

            // Set private _parent field via reflection here.
            // This basically simulates the field getting set in ExposeData(), with way less test boilerplate.
            var field = typeof(SpatialComponent).GetField("_parent", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var parent = EntityManager.CreateEntityUninitialized(IdMapDummy);
            var child1 = EntityManager.CreateEntityUninitialized(IdDummy);
            var child2 = EntityManager.CreateEntityUninitialized(IdDummy);

            field.SetValue(IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(child1.Uid), parent.Uid);
            field.SetValue(IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(child2.Uid), child1.Uid);

            EntityManager.FinishEntityInitialization(child2);
            EntityManager.FinishEntityInitialization(child1);
            EntityManager.FinishEntityInitialization(parent);

            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(child2.Uid).MapID, Is.EqualTo(new MapId(123)));
            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(child1.Uid).MapID, Is.EqualTo(new MapId(123)));
            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(parent.Uid).MapID, Is.EqualTo(new MapId(123)));
        }
    }
}
