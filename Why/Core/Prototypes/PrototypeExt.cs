using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Prototypes
{
    public static class PrototypeExt
    {
        public static PrototypeId<T> GetStrongID<T>(this T proto) where T : class, IPrototype
        {
            return new(proto.ID);
        }
    }
}
