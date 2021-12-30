using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Utility
{
    public static class LuaHelpers
    {
        public static object? TryGetValue(this LuaTable table, string key)
        {
            var obj = table[key];
            return obj;
        }
    }
}
