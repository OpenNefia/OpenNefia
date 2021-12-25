using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Describes a thing that can potentially be stacked using the
    /// <see cref="StackSystem"/>.
    /// </summary>
    public interface IStackable<in T>
    {
        /// <summary>
        /// Returns true if this object can be stacked with the other object.
        /// </summary>
        bool IsSameAs(T other);
    }
}
