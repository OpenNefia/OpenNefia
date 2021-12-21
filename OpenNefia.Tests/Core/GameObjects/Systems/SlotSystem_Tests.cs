using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Tests.Core.GameObjects.Systems
{
    [TestFixture]
    [TestOf(typeof(SlotSystem))]
    public class SlotSystem_Tests : OpenNefiaUnitTest
    {
        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterComponents(compFac => {
                    compFac.RegisterClass<SlotsComponent>();
                    compFac.RegisterClass<SlotTestComponent>();
                })
                .RegisterDataDefinitionTypes(types => types.Add(typeof(SlotTestComponent)))
                .RegisterEntitySystems(entSysMan => entSysMan.LoadExtraSystemType<SlotSystem>())
                .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        /// <summary>
        /// When a slot with no components is added, it will create a blank SlotRegistration.
        /// </summary>
        [Test]
        public void TestAddSlot_NoComponents()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entSysMan = sim.Resolve<IEntitySystemManager>();
            var map = sim.ActiveMap!;

            var slotSys = entSysMan.GetEntitySystem<SlotSystem>();

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.Zero)).Uid;

            var comps = new ComponentRegistry();
            var slotId = slotSys.AddSlot(ent, comps);
            
            var slots = entMan.GetComponent<SlotsComponent>(ent);

            Assert.That(slots.Registrations.ContainsKey(slotId), Is.True);
            
            var reg = slots.Registrations[slotId];

            Assert.That(reg.Id, Is.EqualTo(slotId));
            Assert.That(reg.CompTypes.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// When a slot with components already on the entity is added, it will exclude those components
        /// from the slot.
        /// </summary>
        [Test]
        public void TestAddSlot_AlreadyExistingComponents()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entSysMan = sim.Resolve<IEntitySystemManager>();
            var map = sim.ActiveMap!;

            var slotSys = entSysMan.GetEntitySystem<SlotSystem>();

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.Zero)).Uid;

            var comps = new ComponentRegistry();
            var spatial = new SpatialComponent();
            comps[spatial.Name] = spatial;

            var slotId = slotSys.AddSlot(ent, comps);

            var slots = entMan.GetComponent<SlotsComponent>(ent);

            Assert.That(slots.Registrations.ContainsKey(slotId), Is.True);

            var reg = slots.Registrations[slotId];

            Assert.That(reg.Id, Is.EqualTo(slotId));
            Assert.That(reg.CompTypes.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// When a slot with components not on the entity is added, it will add those components
        /// to the entity.
        /// </summary>
        [Test]
        public void TestAddSlot_NonExistingComponents()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entSysMan = sim.Resolve<IEntitySystemManager>();
            var map = sim.ActiveMap!;

            var slotSys = entSysMan.GetEntitySystem<SlotSystem>();

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.Zero)).Uid;

            var comps = new ComponentRegistry();
            var slotTest = new SlotTestComponent()
            {
                Field = 42
            };
            comps[slotTest.Name] = slotTest;

            var slotId = slotSys.AddSlot(ent, comps);

            var slots = entMan.GetComponent<SlotsComponent>(ent);

            Assert.That(slots.Registrations.ContainsKey(slotId), Is.True);

            var reg = slots.Registrations[slotId];

            Assert.That(reg.Id, Is.EqualTo(slotId));
            Assert.That(reg.CompTypes.Count, Is.EqualTo(1));

            var slotTestComp = entMan.GetComponent<SlotTestComponent>(ent);

            Assert.That(slotTestComp.Field, Is.EqualTo(42));
        }

        /// <summary>
        /// When a registered slot is removed, the components in the slot are removed.
        /// </summary>
        [Test]
        public void TestRemoveSlot_Single()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entSysMan = sim.Resolve<IEntitySystemManager>();
            var map = sim.ActiveMap!;

            var slotSys = entSysMan.GetEntitySystem<SlotSystem>();

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.Zero)).Uid;

            var comps = new ComponentRegistry();
            var slotTest = new SlotTestComponent();
            comps[slotTest.Name] = slotTest;

            var slotId = slotSys.AddSlot(ent, comps);
            slotSys.RemoveSlot(ent, slotId);

            Assert.That(entMan.HasComponent<SlotsComponent>(ent), Is.False);
        }

        /// <summary>
        /// When a registered slot is removed, the components in the slot are removed
        /// if no other registered slot also registered those components previously.
        /// </summary>
        [Test]
        public void TestRemoveSlot_Duplicate()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entSysMan = sim.Resolve<IEntitySystemManager>();
            var map = sim.ActiveMap!;

            var slotSys = entSysMan.GetEntitySystem<SlotSystem>();

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.Zero)).Uid;

            var comps = new ComponentRegistry();
            var slotTest = new SlotTestComponent();
            comps[slotTest.Name] = slotTest;

            var slotId1 = slotSys.AddSlot(ent, comps);
            var slotId2 = slotSys.AddSlot(ent, comps);

            // Remove the first slot.
            slotSys.RemoveSlot(ent, slotId1);

            Assert.That(entMan.HasComponent<SlotTestComponent>(ent), Is.True);

            // Remove the second slot.
            slotSys.RemoveSlot(ent, slotId2);

            Assert.That(entMan.HasComponent<SlotTestComponent>(ent), Is.False);
        }

        /// <summary>
        /// When a registered slot is removed, any components that were removed
        /// beforehand are ignored.
        /// </summary>
        [Test]
        public void TestRemoveSlot_RemovedExternally()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entSysMan = sim.Resolve<IEntitySystemManager>();
            var map = sim.ActiveMap!;

            var slotSys = entSysMan.GetEntitySystem<SlotSystem>();

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.Zero)).Uid;

            var comps = new ComponentRegistry();
            var slotTest = new SlotTestComponent();
            comps[slotTest.Name] = slotTest;

            var slotId1 = slotSys.AddSlot(ent, comps);

            // Remove the component that the slot added.
            entMan.RemoveComponent<SlotTestComponent>(ent);

            Assert.That(entMan.HasComponent<SlotTestComponent>(ent), Is.False);

            slotSys.RemoveSlot(ent, slotId1);

            Assert.That(entMan.HasComponent<SlotTestComponent>(ent), Is.False);
        }

        [Test]
        public void TestSlotWithComponent()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entSysMan = sim.Resolve<IEntitySystemManager>();
            var map = sim.ActiveMap!;

            var slotSys = entSysMan.GetEntitySystem<SlotSystem>();

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.Zero)).Uid;

            Assert.That(slotSys.SlotWithComponent<SlotTestComponent>(ent), Is.EqualTo(null));

            var comps = new ComponentRegistry();
            var slotTest = new SlotTestComponent();
            comps[slotTest.Name] = slotTest;

            var slotId = slotSys.AddSlot(ent, comps);

            Assert.That(slotSys.SlotWithComponent<SlotTestComponent>(ent), Is.EqualTo(slotId));
        }

        private class SlotTestComponent : Component
        {
            public override string Name => "SlotTest";

            [DataField]
            public int Field { get; set; }
        }
    }
}
