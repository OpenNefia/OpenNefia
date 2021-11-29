using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core
{
    public delegate string LocaleFunc<T1>(T1 arg1);
    public delegate string LocaleFunc<T1, T2>(T1 arg1, T2 arg2);
}
