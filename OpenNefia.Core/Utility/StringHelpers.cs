using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Utility
{
    public static class StringHelpers
    {
        public static string FirstCharToUpper(this string input)
        {
            return input switch
            {
                "" => "",
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
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
        /// Converts a string to lowerCamelCase, accounting for successive leading capitals.
        /// </summary>
        /// <remarks>
        /// <para>
        /// "URLValue" -> "urlValue"
        /// </para>
        /// <para>
        /// "ID" -> "id"
        /// </para>
        /// <para>
        /// "SOME PROPERTY" -> "some PROPERTY"
        /// </para>
        /// </remarks>
        /// <param name="s"></param>
        /// <returns></returns>
        // https://github.com/JamesNK/Newtonsoft.Json/blob/52190a3a3de6ef9a556583cbcb2381073e7197bc/Src/Newtonsoft.Json/Utilities/StringUtils.cs#L155
        public static string ToLowerCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    // if the next character is a space, which is not considered uppercase 
                    // (otherwise we wouldn't be here...)
                    // we want to ensure that the following:
                    // 'FOO bar' is rewritten as 'foo bar', and not as 'foO bar'
                    // The code was written in such a way that the first word in uppercase
                    // ends when if finds an uppercase letter followed by a lowercase letter.
                    // now a ' ' (space, (char)32) is considered not upper
                    // but in that case we still want our current character to become lowercase
                    if (char.IsSeparator(chars[i + 1]))
                    {
                        chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
                    }

                    break;
                }

                chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
            }

            return new string(chars);
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
        
        public static string WidePadRight(this string str, int totalWideWidth, char paddingChar = ' ')
        {
            var wideWidth = str.GetWideLength();
            var needed = Math.Max(totalWideWidth - wideWidth, 0);

            return $"{str}{new string(paddingChar, needed)}";
        }

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

        public static int HashStringToInteger(string str)
        {
            var hasher = SHA256.Create();
            var hashed = hasher.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToInt32(hashed, 0);
        }
    }
}
