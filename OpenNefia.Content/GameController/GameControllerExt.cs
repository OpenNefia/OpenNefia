using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameController
{
    public static class GameControllerExt
    {
        public static void Wait(this IGameController gameController, float timeSecs, bool noRefreshScreen = false)
        {
            var field = IoCManager.Resolve<IFieldLayer>();
            var remaining = timeSecs;

            while (remaining > 0f)
            {
                var dt = gameController.StepFrame();
                remaining -= dt;
                if (!noRefreshScreen)
                    field.RefreshScreen();
            }
        }
    }
}
