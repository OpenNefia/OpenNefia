using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    public class MapTypeFieldComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTypeField";
    }
}
