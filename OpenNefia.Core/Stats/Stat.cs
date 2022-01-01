using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Stats
{
    public struct Stat<T>
    {
        public T Base { get; set; }
        public T Buffed { get; set; }

        public Stat(T baseValue) : this (baseValue, baseValue) {}

        public Stat(T baseValue, T buffedValue)
        {
            Base = baseValue;
            Buffed = buffedValue;
        }

        public void Reset()
        {
            Buffed = Base;
        }

        public static implicit operator T(Stat<T> stat)
        {
            return stat.Base;
        }

        public static implicit operator Stat<T>(T value)
        {
            return new(value);
        }
    }
}
