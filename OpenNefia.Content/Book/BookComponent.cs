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
    public sealed class BookComponent : Component
    {
        public override string Name => "Book";

        [DataField]
        public PrototypeId<BookPrototype> BookID { get; set; }
    }
}