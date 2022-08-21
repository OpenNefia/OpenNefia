using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Combat;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Materials;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Weight;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Materials
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(MaterialSystem))]
    public class MaterialSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntityWeapon = new("TestEntityWeapon");
        private static readonly PrototypeId<EntityPrototype> TestEntityFurniture = new("TestEntityFurniture");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntityWeapon}
  components:
  - type: Spatial
  - type: Material
    materialID: {Protos.Material.Ether}
  - type: Quality
    quality: Great
  - type: Weight
    weight: 100
  - type: Value
    value: 100
  - type: Chip
    id: {Protos.Chip.ItemDagger}
    color: '#FFFFFF'
  - type: EquipStats
    hitBonus: 10
    damageBonus: 10
    dv: 10
    pv: 10
  - type: Weapon
    diceX: 10
    diceY: 10
  - type: Ammo
    diceX: 10
    diceY: 10

- type: Entity
  id: {TestEntityFurniture}
  components:
  - type: Spatial
  - type: Material
    materialID: {Protos.Material.Ether}
  - type: Quality
    quality: Great
  - type: Furniture
    furnitureQuality: 4
  - type: Value
    value: 100
";

        [Test]
        public void TestMaterialSystem_ApplyMaterial()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var refresh = sim.GetEntitySystem<IRefreshSystem>();

            var sys = sim.GetEntitySystem<IMaterialSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var entWeapon = entGen.SpawnEntity(TestEntityWeapon, map.AtPos(0, 0))!.Value;
            var entFurniture = entGen.SpawnEntity(TestEntityFurniture, map.AtPos(0, 0))!.Value;

            entMan.GetComponent<MaterialComponent>(entWeapon).RandomSeed = 0;
            entMan.GetComponent<MaterialComponent>(entFurniture).RandomSeed = 0;

            void DoAsserts()
            {
                Assert.Multiple(() =>
                {
                    var weight = entMan.GetComponent<WeightComponent>(entWeapon);
                    Assert.That(weight.Weight.Buffed, Is.EqualTo(80));

                    var value = entMan.GetComponent<ValueComponent>(entWeapon);
                    Assert.That(value.Value.Buffed, Is.EqualTo(1200));

                    var chip = entMan.GetComponent<ChipComponent>(entWeapon);
                    Assert.That(chip.Color, Is.EqualTo(Color.FromHex("#AFAFFF")));

                    var equipStats = entMan.GetComponent<EquipStatsComponent>(entWeapon);
                    Assert.That(equipStats.HitBonus.Buffed, Is.EqualTo(9));
                    Assert.That(equipStats.DamageBonus.Buffed, Is.EqualTo(30));
                    Assert.That(equipStats.DV.Buffed, Is.EqualTo(42));
                    Assert.That(equipStats.PV.Buffed, Is.EqualTo(9));

                    var weapon = entMan.GetComponent<WeaponComponent>(entWeapon);
                    Assert.That(weapon.DiceX.Buffed, Is.EqualTo(10));
                    Assert.That(weapon.DiceY.Buffed, Is.EqualTo(26));

                    var ammo = entMan.GetComponent<AmmoComponent>(entWeapon);
                    Assert.That(ammo.DiceX.Buffed, Is.EqualTo(10));
                    Assert.That(ammo.DiceY.Buffed, Is.EqualTo(31));

                    // Furniture value calculation.
                    var valueFurniture = entMan.GetComponent<ValueComponent>(entFurniture);
                    Assert.That(valueFurniture.Value.Buffed, Is.EqualTo(4000));
                });
            }

            DoAsserts();

            // Ensure that the random values won't change after refresh.
            refresh.Refresh(entWeapon);
            refresh.Refresh(entFurniture);

            DoAsserts();
        }
    }
}