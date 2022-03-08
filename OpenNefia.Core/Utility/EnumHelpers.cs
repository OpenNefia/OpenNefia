using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Utility
{
    public static class EnumHelpers
    {
        /// <summary>
        /// Gets all items for an enum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static IEnumerable<T> EnumerateValues<T>()
            where T: Enum
        {
            foreach (object item in Enum.GetValues(typeof(T)))
            {
                yield return (T)item;
            }
        }

        /// <summary>
        /// Gets all combined items from an enum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flag">The value.</param>
        /// <returns></returns>
        /// <example>
        /// Displays ValueA and ValueB.
        /// <code>
        /// EnumExample dummy = EnumExample.Combi;
        /// foreach (var item in dummy.GetAllSelectedItems<EnumExample>())
        /// {
        ///    Console.WriteLine(item);
        /// }
        /// </code>
        /// </example>
        public static IEnumerable<T> EnumerateValuesWithFlag<T>(this Enum flag)
        {
            int valueAsInt = Convert.ToInt32(flag, CultureInfo.InvariantCulture);

            foreach (object item in Enum.GetValues(typeof(T)))
            {
                int itemAsInt = Convert.ToInt32(item, CultureInfo.InvariantCulture);

                if (itemAsInt == (valueAsInt & itemAsInt))
                {
                    yield return (T)item;
                }
            }
        }

        /// <summary>
        /// Determines whether the enum value contains a specific value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="request">The request.</param>
        /// <returns>
        ///     <c>true</c> if value contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        /// <example>
        /// <code>
        /// EnumExample dummy = EnumExample.Combi;
        /// if (dummy.Contains<EnumExample>(EnumExample.ValueA))
        /// {
        ///     Console.WriteLine("dummy contains EnumExample.ValueA");
        /// }
        /// </code>
        /// </example>
        public static bool HasAllFlags<T>(this Enum value, T request)
        {
            int valueAsInt = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            int requestAsInt = Convert.ToInt32(request, CultureInfo.InvariantCulture);

            if (requestAsInt == (valueAsInt & requestAsInt))
            {
                return true;
            }

            return false;
        }

        public static T MinValue<T>() where T : Enum
        {
            var raw = EnumerateValues<T>().Cast<int>().Min();
            return (T)Enum.ToObject(typeof(T), raw);
        }

        public static T MaxValue<T>() where T : Enum
        {
            var raw = EnumerateValues<T>().Cast<int>().Max();
            return (T)Enum.ToObject(typeof(T), raw);
        }

        public static object MinValue(Type enumType)
        {
            var raw = Enum.GetValues(enumType).Cast<int>().Min();
            return Enum.ToObject(enumType, raw);
        }

        public static object MaxValue(Type enumType)
        {
            var raw = Enum.GetValues(enumType).Cast<int>().Max();
            return Enum.ToObject(enumType, raw);
        }
    }
}
