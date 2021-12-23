using NUnit.Framework;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Tests.Core.GameObjects.Components
{
    [TestFixture, Parallelizable]
    public class ContainerTest
    {
        private static readonly PrototypeId<EntityPrototype> DummyId = new("dummy");

        private static readonly ContainerId DummyContId = new("dummy");
        private static readonly ContainerId DummyCont2Id = new("dummy2");
        private static readonly ContainerId DummyCont3Id = new("dummy3");

        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
                .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        const string Prototypes = @"
- type: Entity
  id: dummy
";

        [Test]
        public void TestCreation()
        {
            var sim = SimulationFactory();
            var contSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var entity = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;

            var container = contSys.MakeContainer<Container>(entity, DummyContId);

            Assert.That(container.ID, Is.EqualTo(DummyContId));
            Assert.That(container.Owner, Is.EqualTo(entity));

            var manager = IoCManager.Resolve<IEntityManager>().GetComponent<IContainerManager>(entity);

            Assert.That(container.Manager, Is.EqualTo(manager));
            Assert.That(() => contSys.MakeContainer<Container>(entity, DummyContId), Throws.ArgumentException);

            Assert.That(contSys.HasContainer(entity, DummyCont2Id), Is.False);
            var container2 = contSys.MakeContainer<Container>(entity, DummyCont2Id);

            Assert.That(container2.Manager, Is.EqualTo(manager));
            Assert.That(container2.Owner, Is.EqualTo(entity));
            Assert.That(container2.ID, Is.EqualTo(DummyCont2Id));

            Assert.That(contSys.HasContainer(entity, DummyContId), Is.True);
            Assert.That(contSys.HasContainer(entity, DummyCont2Id), Is.True);
            Assert.That(contSys.HasContainer(entity, DummyCont3Id), Is.False);

            Assert.That(contSys.GetContainer(entity, DummyContId), Is.EqualTo(container));
            Assert.That(contSys.GetContainer(entity, DummyCont2Id), Is.EqualTo(container2));
            Assert.That(() => contSys.GetContainer(entity, DummyCont3Id), Throws.TypeOf<KeyNotFoundException>());

            IoCManager.Resolve<IEntityManager>().DeleteEntity(entity);

            Assert.That(manager.Deleted, Is.True);
            Assert.That(container.Deleted, Is.True);
            Assert.That(container2.Deleted, Is.True);
        }

        [Test]
        public void TestInsertion()
        {
            var sim = SimulationFactory(); 
            var contSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var owner = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var inserted = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var transform = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(inserted);

            var container = contSys.MakeContainer<Container>(owner, DummyContId);
            Assert.That(container.Insert(inserted), Is.True);
            Assert.That(transform.Parent!.OwnerUid, Is.EqualTo(owner));

            var container2 = contSys.MakeContainer<Container>(inserted, DummyContId);
            Assert.That(container2.Insert(owner), Is.False);

            var success = container.Remove(inserted);
            Assert.That(success, Is.True);

            success = container.Remove(inserted);
            Assert.That(success, Is.False);

            container.Insert(inserted);
            IoCManager.Resolve<IEntityManager>().DeleteEntity(owner);
            // Make sure inserted was detached.
            Assert.That(transform.Deleted, Is.True);
        }

        [Test]
        public void TestNestedRemoval()
        {
            var sim = SimulationFactory();
            var contSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var owner = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var inserted = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var transform = IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(inserted);
            var entity = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;

            var container = contSys.MakeContainer<Container>(owner, DummyContId);
            Assert.That(container.Insert(inserted), Is.True);
            Assert.That(transform.Parent!.OwnerUid, Is.EqualTo(owner));

            var container2 = contSys.MakeContainer<Container>(inserted, DummyContId);
            Assert.That(container2.Insert(entity), Is.True);
            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(entity).Parent!.OwnerUid, Is.EqualTo(inserted));

            Assert.That(container2.Remove(entity), Is.True);
            Assert.That(container.Contains(entity), Is.True);
            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(entity).Parent!.OwnerUid, Is.EqualTo(owner));

            IoCManager.Resolve<IEntityManager>().DeleteEntity(owner);
            Assert.That(transform.Deleted, Is.True);
        }

        [Test]
        public void TestNestedRemovalWithDenial()
        {
            var sim = SimulationFactory(); 
            var contSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var coordinates = new EntityCoordinates(new EntityUid(1), (0, 0));
            var entityOne = sim.SpawnEntity(DummyId, coordinates).Uid;
            var entityTwo = sim.SpawnEntity(DummyId, coordinates).Uid;
            var entityThree = sim.SpawnEntity(DummyId, coordinates).Uid;
            var entityItem = sim.SpawnEntity(DummyId, coordinates).Uid;

            var container = contSys.MakeContainer<Container>(entityOne, DummyContId);
            var container2 = contSys.MakeContainer<ContainerOnlyContainer>(entityTwo, DummyContId);
            var container3 = contSys.MakeContainer<Container>(entityThree, DummyContId);

            var entMan = IoCManager.Resolve<IEntityManager>();

            Assert.That(container.Insert(entityTwo), Is.True);
            Assert.That(entMan.GetComponent<SpatialComponent>(entityTwo).Parent!.OwnerUid, Is.EqualTo(entityOne));

            Assert.That(container2.Insert(entityThree), Is.True);
            Assert.That(entMan.GetComponent<SpatialComponent>(entityThree).Parent!.OwnerUid, Is.EqualTo(entityTwo));

            Assert.That(container3.Insert(entityItem), Is.True);
            Assert.That(entMan.GetComponent<SpatialComponent>(entityItem).Parent!.OwnerUid, Is.EqualTo(entityThree));

            Assert.That(container3.Remove(entityItem), Is.True);
            Assert.That(container.Contains(entityItem), Is.True);
            Assert.That(entMan.GetComponent<SpatialComponent>(entityItem).Parent!.OwnerUid, Is.EqualTo(entityOne));

            entMan.DeleteEntity(entityOne);
            Assert.That(entMan.Deleted(entityOne), Is.True);
        }

        [Test]
        public void BaseContainer_SelfInsert_False()
        {
            var sim = SimulationFactory();
            var contSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var entity = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var container = contSys.MakeContainer<Container>(entity, DummyContId);

            Assert.That(container.Insert(entity), Is.False);
            Assert.That(container.CanInsert(entity), Is.False);
        }

        [Test]
        public void BaseContainer_InsertMap_False()
        {
            var sim = SimulationFactory();
            var contSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var mapEnt = new EntityUid(1);
            var entity = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var container = contSys.MakeContainer<Container>(entity, DummyContId);

            Assert.That(container.Insert(mapEnt), Is.False);
            Assert.That(container.CanInsert(mapEnt), Is.False);
        }

        [Test]
        public void BaseContainer_InsertGrid_False()
        {
            var sim = SimulationFactory();
            var contSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var map = sim.Resolve<IMapManager>().ActiveMap!.MapEntityUid;
            var entity = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var container = contSys.MakeContainer<Container>(entity, DummyContId);

            Assert.That(container.Insert(map), Is.False);
            Assert.That(container.CanInsert(map), Is.False);
        }

        [Test]
        public void BaseContainer_Insert_True()
        {
            var sim = SimulationFactory();
            var contSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var containerEntity = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var container = contSys.MakeContainer<Container>(containerEntity, DummyContId);
            var insertEntity = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;

            var result = container.Insert(insertEntity);

            Assert.That(result, Is.True);
            Assert.That(container.ContainedEntities.Count, Is.EqualTo(1));

            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(containerEntity).ChildCount, Is.EqualTo(1));
            Assert.That(IoCManager.Resolve<IEntityManager>().GetComponent<SpatialComponent>(containerEntity).ChildEntities.First(), Is.EqualTo(insertEntity));

            result = contSys.TryGetContainerMan(insertEntity, out var resultContainerMan);
            Assert.That(result, Is.True);
            Assert.That(resultContainerMan, Is.EqualTo(container.Manager));
        }

        [Test]
        public void BaseContainer_RemoveNotAdded_False()
        {
            var sim = SimulationFactory();
            var contSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var containerEntity = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var container = contSys.MakeContainer<Container>(containerEntity, DummyContId);
            var insertEntity = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;

            var result = container.Remove(insertEntity);

            Assert.That(result, Is.False);
        }

        [Test]
        public void BaseContainer_Transfer_True()
        {
            var sim = SimulationFactory();
            var contSys = sim.Resolve<IEntitySystemManager>().GetEntitySystem<ContainerSystem>();

            var entity1 = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var container1 = contSys.MakeContainer<Container>(entity1, DummyContId);
            var entity2 = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            var container2 = contSys.MakeContainer<Container>(entity2, DummyContId);
            var transferEntity = sim.SpawnEntity(DummyId, new EntityCoordinates(new EntityUid(1), (0, 0))).Uid;
            container1.Insert(transferEntity);

            var result = container2.Insert(transferEntity);

            Assert.That(result, Is.True);
            Assert.That(container1.ContainedEntities.Count, Is.EqualTo(0));
            Assert.That(container2.ContainedEntities.Count, Is.EqualTo(1));
        }

        private class ContainerOnlyContainer : BaseContainer
        {
            /// <summary>
            /// The generic container class uses a list of entities
            /// </summary>
            private readonly List<EntityUid> _containerList = new();
            private readonly List<EntityUid> _expectedEntities = new();

            public override string ContainerType => nameof(ContainerOnlyContainer);

            /// <inheritdoc />
            public override IReadOnlyList<EntityUid> ContainedEntities => _containerList;

            /// <inheritdoc />
            protected override void InternalInsert(EntityUid toinsert, IEntityManager? entMan = null)
            {
                _containerList.Add(toinsert);
                base.InternalInsert(toinsert, entMan);
            }

            /// <inheritdoc />
            protected override void InternalRemove(EntityUid toremove, IEntityManager? entMan = null)
            {
                _containerList.Remove(toremove);
                base.InternalRemove(toremove, entMan);
            }

            /// <inheritdoc />
            public override bool Contains(EntityUid contained)
            {
                return _containerList.Contains(contained);
            }

            /// <inheritdoc />
            public override void Shutdown()
            {
                base.Shutdown();

                var entMan = IoCManager.Resolve<IEntityManager>();

                foreach (var entity in _containerList)
                {
                    entMan.DeleteEntity(entity);
                }
            }

            public override bool CanInsert(EntityUid toinsert, IEntityManager? entMan = null)
            {
                IoCManager.Resolve(ref entMan);

                return entMan.TryGetComponent(toinsert, out IContainerManager? _);
            }
        }
    }
}
