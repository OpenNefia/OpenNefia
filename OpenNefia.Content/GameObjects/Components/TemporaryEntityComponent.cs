using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Component for entities that will be deleted when the map they're in undergoes a major renewal.
    /// </summary>
    [RegisterComponent]
    public sealed class TemporaryEntityComponent : Component
    {
    }
}
