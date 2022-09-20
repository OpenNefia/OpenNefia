using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Indicates this entity is the player.
    /// </summary>
    [Obsolete("Replace this with IGameSession.IsPlayer()")]
    [RegisterComponent]
    public class PlayerComponent : Component
    {    }
}
