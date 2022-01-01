using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.CustomName
{
    [RegisterComponent]
    public class CustomNameComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "CustomName";

        [DataField("name")]
        public LocaleKey DisplayName { get; set; }
    }
}
