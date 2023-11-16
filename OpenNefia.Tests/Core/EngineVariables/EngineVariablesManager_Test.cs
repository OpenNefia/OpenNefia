using System;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework;
using OpenNefia.Core.EngineVariables;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Tests.Core.Prototypes;
using OpenNefia.Tests.Core.SaveGames;

namespace OpenNefia.Tests.Core.EngineVariables
{
    [UsedImplicitly]
    [TestFixture, Parallelizable]
    public class EngineVariablesManager_Test : OpenNefiaUnitTest
    {
        const string DOCUMENT = @"
- type: EngineVariable
  id: Test.TestInt
  default: 100
- type: EngineVariable
  id: Test.TestList
  default:
  - 100
  - 101
  - 102
- type: EngineVariable
  id: Test.TestMap
  default:
    foo: 100
    bar: 101
    baz: 102
";

        [Test]
        public void Test_DefaultValues()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterPrototypes(factory =>
                {
                    factory.LoadString(DOCUMENT);
                })
                .InitializeInstance();

            var engineVars = sim.Resolve<IEngineVariablesManager>();

            Assert.Multiple(() =>
            {
                Assert.That(engineVars.Get<int>(new("Test.TestInt")), Is.EqualTo(100));
                Assert.That(engineVars.Get<List<int>>(new("Test.TestList")), Is.EquivalentTo(new int[] { 100, 101, 102 }));
                Assert.That(engineVars.Get<Dictionary<string, int>>(new("Test.TestMap")), Is.EquivalentTo(new Dictionary<string, int>() { { "foo", 100 }, { "bar", 101 }, { "baz", 102 } }));
            });
        }

        [Test]
        public void Test_SystemInjection()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory =>
                {
                    factory.LoadExtraSystemType<EngineVariableTestSystem>();
                })
                .RegisterPrototypes(factory =>
                {
                    factory.LoadString(DOCUMENT);
                })
                .InitializeInstance();

            var sys = sim.GetEntitySystem<EngineVariableTestSystem>();

            Assert.Multiple(() =>
            {
                Assert.That(sys.TestInt, Is.EqualTo(100));
                Assert.That(sys.TestList, Is.EquivalentTo(new int[] { 100, 101, 102 }));
                Assert.That(sys.TestMap, Is.EquivalentTo(new Dictionary<string, int>() { { "foo", 100 }, { "bar", 101 }, { "baz", 102 } }));
            });
        }

        [Test]
        public void Test_SystemInjectionInvalid()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var sim = GameSimulation
                    .NewSimulation()
                    .RegisterEntitySystems(factory =>
                    {
                        factory.LoadExtraSystemType<EngineVariableInvalidTestSystem>();
                    })
                    .RegisterPrototypes(factory =>
                    {
                        factory.LoadString(DOCUMENT);
                    })
                    .InitializeInstance();
            });
        }

        [Test]
        public void Test_OverrideVariables()
        {
            var variables = @"
Test:
   TestInt: 400
   TestList:
   - 400
   - 401
   - 402
";

            var sim = GameSimulation
                .NewSimulation()
                .RegisterPrototypes(factory =>
                {
                    factory.LoadString(DOCUMENT);
                })
                .RegisterEngineVariables(factory =>
                {
                    factory.LoadString(variables);
                })
                .InitializeInstance();

            var engineVars = sim.Resolve<IEngineVariablesManager>();

            Assert.Multiple(() =>
            {
                Assert.That(engineVars.Get<int>(new("Test.TestInt")), Is.EqualTo(400));
                Assert.That(engineVars.Get<List<int>>(new("Test.TestList")), Is.EquivalentTo(new int[] { 400, 401, 402 }));
                Assert.That(engineVars.Get<Dictionary<string, int>>(new("Test.TestMap")), Is.EquivalentTo(new Dictionary<string, int>() { { "foo", 100 }, { "bar", 101 }, { "baz", 102 } }));
            });
        }

        [Test]
        public void Test_SystemInjectionOverrideVariables()
        {
            var variables = @"
Test:
   TestInt: 400
   TestList:
   - 400
   - 401
   - 402
";

            var sim = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory =>
                {
                    factory.LoadExtraSystemType<EngineVariableTestSystem>();
                })
                .RegisterPrototypes(factory =>
                {
                    factory.LoadString(DOCUMENT);
                })
                .RegisterEngineVariables(factory =>
                {
                    factory.LoadString(variables);
                })
                .InitializeInstance();

            var sys = sim.GetEntitySystem<EngineVariableTestSystem>();

            Assert.Multiple(() =>
            {
                Assert.That(sys.TestInt, Is.EqualTo(400));
                Assert.That(sys.TestList, Is.EquivalentTo(new int[] { 400, 401, 402 }));
                Assert.That(sys.TestMap, Is.EquivalentTo(new Dictionary<string, int>() { { "foo", 100 }, { "bar", 101 }, { "baz", 102 } }));
            });
        }
    }

    [Reflect(false)]
    public class EngineVariableTestSystem : EntitySystem
    {
        [EngineVariable("Test.TestInt")]
        public int TestInt { get; } = -1;

        [EngineVariable("Test.TestList")]
        public List<int> TestList { get; } = new();

        [EngineVariable("Test.TestMap")]
        public Dictionary<string, int> TestMap { get; } = new();
    }

    [Reflect(false)]
    public class EngineVariableInvalidTestSystem : EntitySystem
    {
        [EngineVariable("Foo")]
        public int Foo { get; } = -1;
    }
}
