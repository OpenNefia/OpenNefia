using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using OpenNefia.Content.Nefia;
using OpenNefia.Content.RandomAreas;
using OpenNefia.Content.Food;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.World;

namespace OpenNefia.Content.Tests.Areas
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(FoodSystem))]
    public class FoodSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestFoodPerishable = new("TestFoodPerishable");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestFoodPerishable}
  parent: BaseItem
  components:
  - type: Food
    spoilageInterval: 24:00:00
    foodType: Elona.Meat
    foodQuality: 5
  - type: Item
    material: Elona.Fresh
";

        [Test]
        public void TestInitFood()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var mapMan = sim.Resolve<IMapManager>();
            var worldSys = sim.GetEntitySystem<IWorldSystem>();
            var foodSys = sim.GetEntitySystem<IFoodSystem>();

            worldSys.State.GameDate = new GameDateTime(512, 1, 12);

            var map = sim.CreateMapAndSetActive(10, 10);
            var food = entGen.SpawnEntity(TestFoodPerishable, map.AtPos(Vector2i.Zero))!.Value;
            var foodComp = entMan.GetComponent<FoodComponent>(food);

            Assert.That(foodComp.SpoilageDate, Is.EqualTo(new GameDateTime(512, 1, 13)));
            Assert.That(entMan.GetComponent<ChipComponent>(food).ChipID, Is.EqualTo(Protos.Chip.ItemDishMeat5));
        }

        [Test]
        public void TestSpoilInMap()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var mapMan = sim.Resolve<IMapManager>();
            var worldSys = sim.GetEntitySystem<IWorldSystem>();
            var foodSys = sim.GetEntitySystem<IFoodSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);
            var food = entGen.SpawnEntity(TestFoodPerishable, map.AtPos(Vector2i.Zero))!.Value;
            var foodComp = entMan.GetComponent<FoodComponent>(food);

            Assert.That(foodComp.IsRotten, Is.False);

            worldSys.PassTime(GameTimeSpan.FromHours(23));
            Assert.That(foodComp.IsRotten, Is.False);

            worldSys.PassTime(GameTimeSpan.FromHours(2));
            Assert.That(foodComp.IsRotten, Is.True);
            Assert.That(entMan.GetComponent<ChipComponent>(food).ChipID, Is.EqualTo(Protos.Chip.ItemRottenFood));
        }
    }
}
