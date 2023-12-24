using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;

namespace OpenNefia.Content.Wishes
{
    /// <summary>
    /// Logic that can handle wishes.
    /// To match a wish against a specific keyword, localize the "Keyword" field
    /// of this prototype and specify either a string or list of possible strings
    /// to match in the wish.
    /// If no keyword is available, the handler will always run if reached, so you can
    /// write custom matching logic.
    /// </summary>
    [Prototype("Elona.WishHandler")]
    public class WishHandlerPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(WishHandlerPrototype))]
    public sealed class P_WishHandlerOnWishEvent : HandledPrototypeEventArgs
    { 
        public P_WishHandlerOnWishEvent(EntityUid wisher, string wish)
        {
            Wisher = wisher;
            Wish = wish;
        }

        public EntityUid Wisher { get; }
        public string Wish { get; }

        public bool OutDidSomething { get; set; } = true;
    }
}