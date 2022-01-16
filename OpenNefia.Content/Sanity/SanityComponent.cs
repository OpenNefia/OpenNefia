using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Sanity
{
    [RegisterComponent]
    public sealed class SanityComponent : Component
    {
        public override string Name => "Sanity";

        /// <summary>
        /// Denotes lack of sanity. Higher value means less sane.
        /// </summary>
        [DataField]
        public int Insanity { get; set; }
    }
}
