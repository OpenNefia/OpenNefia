using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Prototypes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class PrototypeEventAttribute : Attribute
    {
        public Type PrototypeType { get; }

        public PrototypeEventAttribute(Type prototypeType)
        {
            PrototypeType = prototypeType;
        }
    }
}
