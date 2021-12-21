using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    public abstract class SlottableComponent : Component
    {
        /// <summary>
        /// Merged power level of this component.
        /// </summary>
        public int MergedPower { get; set; }
    }
}
