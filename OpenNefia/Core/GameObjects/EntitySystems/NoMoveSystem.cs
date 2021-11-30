using OpenNefia.Core.Logic;
using OpenNefia.Core.Random;
using OpenNefia.Core.UI;

namespace OpenNefia.Core.GameObjects.EntitySystems
{
    public class NoMoveSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<MoveableComponent, OnMoveEventArgs>(HandleOnMove, before: new[] { typeof(MovementSystem) });
        }

        private void HandleOnMove(EntityUid uid, MoveableComponent moveable, OnMoveEventArgs args)
        {
            if (args.Handled)
                return;

            if (Rand.OneIn(5))
            {
                Mes.Display("Oh no you don't.", UiColors.MesRed);
                args.Handled = true;
                args.TurnResult = TurnResult.Succeeded;
            }
        }
    }
}
