using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class CommonProtectionsComponent : Component
    {
        public override string Name => "CommonProtections";

        // TODO figure out how traits and temporary flags will work
        // slots? refreshing?

        [DataField]
        public bool IsProtectedFromRottenFood { get; set; }

        [DataField]
        public bool IsProtectedFromTheft { get; set; }
    }
}