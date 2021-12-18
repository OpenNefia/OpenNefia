using System.IO;
using NUnit.Framework;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Content.Tests.GameObjects.Components;

[TestFixture]
public class CharaComponent_Tests : ContentUnitTest
{
    private const string Prototypes = @"
- type: Entity
  name: dummy
  id: dummy
  components:
  - type: Chara
";

    [OneTimeSetUp]
    public void Setup()
    {
        var componentFactory = IoCManager.Resolve<IComponentFactory>();
        componentFactory.RegisterClass<CharaComponent>();
        componentFactory.FinishRegistration();

        IoCManager.Resolve<ISerializationManager>().Initialize();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        prototypeManager.RegisterType<EntityPrototype>();
        prototypeManager.LoadFromStream(new StringReader(Prototypes));
        prototypeManager.Resync();
    }

    [Test]
    public void CharaComponentLivenessTest()
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

        var dummy = entityManager.CreateEntityUninitialized(new("dummy"));

        Assert.That(dummy, Is.Not.Null);
        Assert.That(entityManager.IsAlive(dummy.Uid), Is.True);
        Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.False);

        var charaComp = entityManager.GetComponent<CharaComponent>(dummy.Uid);

        Assert.That(charaComp.Liveness, Is.EqualTo(CharaLivenessState.Alive));

        charaComp.Liveness = CharaLivenessState.PetDead;

        Assert.That(charaComp.Liveness, Is.EqualTo(CharaLivenessState.PetDead));
        Assert.That(entityManager.IsAlive(dummy.Uid), Is.False);
        Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.False);

        charaComp.Liveness = CharaLivenessState.Dead;

        Assert.That(charaComp.Liveness, Is.EqualTo(CharaLivenessState.Dead));
        Assert.That(entityManager.IsAlive(dummy.Uid), Is.False);
        Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.True);

        charaComp.Liveness = CharaLivenessState.Alive;

        Assert.That(charaComp.Liveness, Is.EqualTo(CharaLivenessState.Alive));
        Assert.That(entityManager.IsAlive(dummy.Uid), Is.True);
        Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.False);
    }
}