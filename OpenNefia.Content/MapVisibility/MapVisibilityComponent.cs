using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.MapVisibility
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public sealed class MapVisibilityComponent : Component
    {
        /// <summary>
        /// Shadow map for rendering purposes. Not serialized.
        /// </summary>
        public ShadowMap ShadowMap = new();
    }
}
