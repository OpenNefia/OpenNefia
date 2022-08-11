using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class ThrowableComponent : Component
    {
        public override string Name => "Throwable";

        /// <summary>
        /// If non-null, the item will be split into a stack of this amount before it is thrown.
        /// </summary>
        [DataField]
        public int? SplitAmount { get; set; }
    }
}
