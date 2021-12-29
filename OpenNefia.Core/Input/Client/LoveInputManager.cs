using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Input.Client
{
    public sealed class LoveInputManager : InputManager
    {
        public override ScreenCoordinates MouseScreenPosition => new(Love.Mouse.GetPosition());
    }
}
