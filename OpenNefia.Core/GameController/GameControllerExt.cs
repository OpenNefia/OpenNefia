using OpenNefia.Core.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameController
{
    public static class GameControllerExt
    {
        public static float StepFrame(this IGameController gameController, bool stepInput = false)
        {
            var dt = Love.Timer.GetDelta();
            var frameArgs = new FrameEventArgs(dt, stepInput: false);
            gameController.Update(frameArgs);
            gameController.Draw();
            gameController.SystemStep(stepInput: stepInput);
            return dt;
        }

    }
}
