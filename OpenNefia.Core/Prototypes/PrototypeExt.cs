using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static IEnumerable<PrototypeId<T>> ProtoIDs<T>(this IEnumerable<T> enumerable)
            where T: class, IPrototype
        {
            return enumerable.Select(p => GetStrongID(p));
        }
    }
}
