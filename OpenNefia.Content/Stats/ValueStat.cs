using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Stats
{
    public struct ValueStat<T> where T : notnull
    {
        public T Base { get; set; }
        public T Buffed { get; set; }

        public ValueStat(T @base)
        {
            Base = @base;
            Buffed = @base;
        }

        public void Refresh()
        {
            Buffed = Base;
        }

        public static implicit operator T(ValueStat<T> stat)
        {
            return stat.Base;
        }
    }
}
