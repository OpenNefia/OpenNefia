using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.IoC;
using Why.Core.Log;
using Why.Core.Maps;
using Why.Core.Utility;

namespace Why.Core.GameObjects
{
    public class TestSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ItemComponent, TestEntityEvent>(DoTest);
        }

        public void DoTest(EntityUid uid, ItemComponent component, TestEntityEvent args)
        {
            if (!EntityManager.TryGetEntity(uid, out var entity))
                return;

            foreach (var pos in PosUtils.GetSurroundingCoords(entity.Coords))
            {
                foreach (var other in pos.GetEntities())
                {
                    Logger.Log(LogLevel.Info, $"Found entity {DisplayNameSystem.GetDisplayName(uid)} {other.Prototype?.ID} (args {args.TestField}) (value: {component.Value})");
                }
            }
        }
    }

    public sealed class TestEntityEvent : EntityEventArgs
    {
        public int TestField;

        public TestEntityEvent(int testField)
        {
            TestField = testField;
        }
    }
}
