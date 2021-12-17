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

        /// <summary>
        /// Removes a leading substring from a string, if it exists.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="prefix"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static string RemovePrefix(this string str, string prefix, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(prefix))
            {
                return str;
            }

            if (str.StartsWith(prefix, comparisonType))
            {
                return str.Remove(0, prefix.Length);
            }

            return str;
        }

        /// <summary>
        /// Gets the CJK length of this string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetWideLength(this string str) => UnicodeWidth.GetWidthCJK(str);

        /// <summary>
        /// Gets the CJK width of this character.
        /// </summary>
        /// <param name="rune"></param>
        /// <returns></returns>
        public static int GetWideWidth(this Rune rune) => UnicodeWidth.GetWidth(rune);

        /// <summary>
        /// Like <see cref="string.Substring(int, int)"/>, but operating on CJK width instead of character count.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string WideSubstring(this string str, int? startIndex = null, int? length = null)
        {
            int? boundLeft = null;
            bool foundRight = false;

            if (startIndex == null)
            {
                boundLeft = 0;
                startIndex = 0;
            }
            if (length == null)
            {
                length = str.GetWideLength();
            }

            int i = startIndex.Value;
            int innerLength = 0;
            int normalLength = 0;
            var len = 0;

            for (int pos = 0; pos <= str.Length; pos++)
            {
                if (boundLeft == null && i < 1)
                    boundLeft = pos;

                if (boundLeft != null && foundRight)
                    break;

                if (pos >= str.Length)
                    break;

                len = Rune.GetRuneAt(str, pos).GetWideWidth();
                i -= len;

                if (innerLength + len > length)
                {
                    foundRight = true;
                    break;
                }

                if (boundLeft != null)
                {
                    normalLength += 1;
                    innerLength += len;
                }
            }

            if (boundLeft == null)
                boundLeft = str.Length;
            if (!foundRight)
                return str.Substring(boundLeft.Value);

            return str.Substring(boundLeft.Value, normalLength);
        }
    }
}
