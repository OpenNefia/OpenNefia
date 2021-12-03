using System;
using System.IO;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.GameObjects;

[TestFixture]
public class StackableComponent_Tests : OpenNefiaUnitTest
{
    private const string Prototypes = @"
- type: Entity
  name: dummy
  id: dummy
  components:
  - type: Stackable
";

    [OneTimeSetUp]
    public void Setup()
    {
        var componentFactory = IoCManager.Resolve<IComponentFactory>();
        componentFactory.RegisterClass<StackableComponent>();
        componentFactory.FinishRegistration();

        IoCManager.Resolve<ISerializationManager>().Initialize();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        prototypeManager.RegisterType(typeof(EntityPrototype));
        prototypeManager.LoadFromStream(new StringReader(Prototypes));
        prototypeManager.Resync();
    }

    [Test]
    public void StackableComponentLivenessTest()
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

        var dummy = entityManager.CreateEntityUninitialized("dummy");

        Assert.That(dummy, Is.Not.Null);
        Assert.That(entityManager.IsAlive(dummy.Uid), Is.True);
        Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.False);

        var stackableComp = entityManager.GetComponent<StackableComponent>(dummy.Uid);

        Assert.That(stackableComp.Amount, Is.EqualTo(1));

        stackableComp.Amount = 0;

        Assert.That(stackableComp.Amount, Is.EqualTo(0));
        Assert.That(entityManager.IsAlive(dummy.Uid), Is.False);
        Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.True);

        stackableComp.Amount = 1;

        Assert.That(stackableComp.Amount, Is.EqualTo(1));
        Assert.That(entityManager.IsAlive(dummy.Uid), Is.True);
        Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.False);
    }

    [Test]
    public void StackableComponentBoundingTest()
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

        var dummy = entityManager.CreateEntityUninitialized("dummy");

        Assert.That(dummy, Is.Not.Null);

        var stackableComp = entityManager.GetComponent<StackableComponent>(dummy.Uid);

        Assert.That(stackableComp.Amount, Is.EqualTo(1));

        stackableComp.Amount = -5;
        Assert.That(stackableComp.Amount, Is.EqualTo(0));

        stackableComp.Amount = 5;
        Assert.That(stackableComp.Amount, Is.EqualTo(5));
    }
}