using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Book
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class BookOfRachelComponent : Component
    {
        [DataField]
        public int BookOfRachelNumber { get; set; } = 0;
    }
}