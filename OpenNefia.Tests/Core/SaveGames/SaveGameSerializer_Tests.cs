using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.SaveGames
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(SaveGameSerializer))]
    public class SaveGameSerializer_Tests : OpenNefiaUnitTest
    {
        [Test]
        public void TestSerializeOnEntitySystem()
        {
            // Arrange.
            var sim = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory =>
                {
                    factory.LoadExtraSystemType<SaveGameTestSystem>();
                })
                .RegisterDataDefinitionTypes(types => types.Add(typeof(TestSaveData)))
                .InitializeInstance();

            var save = new TempSaveGameHandle();
            var saveSerMan = sim.Resolve<ISaveGameSerializerInternal>();

            // Act.
            var sys = sim.GetEntitySystem<SaveGameTestSystem>();

            sys.Data.Foo = 42;
            sys.Data.Bar = new List<string> { "hoge" };
            saveSerMan.SaveGlobalData(save);

            Assert.That(save.Files.Exists(new ResourcePath("/global.yml")), Is.True);

            sys.Data.Foo = 999;
            sys.Data.Bar.Clear();
            saveSerMan.LoadGlobalData(save);

            // Check that the save data was loaded into the entity system
            Assert.That(sys.Data.Foo, Is.EqualTo(42));
            Assert.That(sys.Data.Bar, Is.EquivalentTo(new[] { "hoge" }));
        }
    }

    [Reflect(false)]
    public class SaveGameTestSystem : EntitySystem
    {
        [RegisterSaveData("SaveGameTestSystem.Data")]
        public TestSaveData Data { get; } = new();
    }

    [DataDefinition]
    public class TestSaveData
    {
        [DataField(required: true)]
        public int Foo { get; set; } = -1;

        [DataField(required: true)]
        public List<string> Bar = new() { "baz", "quux" };
    }
}
