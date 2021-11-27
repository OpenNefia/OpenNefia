using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.IoC;

namespace Why.Core.Prototypes
{
    public static class PrototypeIdExt
    {
        /// <summary>
        /// Converts a prototype ID to a prototype reference.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The reason Robust does not encourage having references to prototypes as data fields
        /// is because it makes prototype hotloading infeasible.
        /// </para>
        /// <para>
        /// I want to replace this with prototype weak references later.
        /// </para>
        /// </remarks>
        public static T ResolvePrototype<T>(this PrototypeId<T> id) where T : class, IPrototype
        {
            return IoCManager.Resolve<IPrototypeManager>().Index(id);
        }
    }
}
