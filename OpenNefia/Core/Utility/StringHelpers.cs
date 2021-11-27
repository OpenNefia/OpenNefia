using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Utility
{
    public static class StringHelpers
    {
        /// <summary>
        /// Converts an UpperCamelCase C# identifier to its default name in YAML.
        /// </summary>
        /// <remarks>
        /// "BaseSkills" -> "baseSkills"
        /// "ID" -> "iD"
        /// </remarks>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetPrototypeFieldName(this string fieldName)
        {
            return char.ToLower(fieldName[0]) + fieldName.Substring(1);
        }
    }
}
