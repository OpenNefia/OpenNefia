using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Why.Core.Utility
{
    public static class StringHelpers
    {
        public static string FirstCharToLowerCase(this string str)
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }
    }
}
