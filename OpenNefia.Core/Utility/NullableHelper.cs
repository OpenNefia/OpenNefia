using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace OpenNefia.Core.Utility
{
    public static class NullableHelper
    {
        private static NullabilityInfoContext _nullabilityContext;
        private static ConcurrentDictionary<MemberInfo, NullabilityInfo> _cachedInfo;

        static NullableHelper()
        {
            _nullabilityContext = new NullabilityInfoContext();
            _cachedInfo = new ConcurrentDictionary<MemberInfo, NullabilityInfo>();
        }

        public static Type EnsureNullableType(this Type type)
        {
            if (type.IsValueType)
            {
                return typeof(Nullable<>).MakeGenericType(type);
            }

            return type;
        }

        public static Type EnsureNotNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private static NullabilityInfo GetNullabilityInfo(FieldInfo field)
        {
            if (_cachedInfo.TryGetValue(field, out var info))
                return info;
            lock (_nullabilityContext)
            {
                info = _nullabilityContext.Create(field);
                _cachedInfo[field] = info;
            }
            return info;
        }

        private static NullabilityInfo GetNullabilityInfo(PropertyInfo property)
        {
            if (_cachedInfo.TryGetValue(property, out var info))
                return info;
            lock (_nullabilityContext)
            {
                info = _nullabilityContext.Create(property);
                _cachedInfo[property] = info;
            }
            return info;
        }

        /// <summary>
        /// Checks if the field has a nullable annotation [?]
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool IsMarkedAsNullable(FieldInfo field)
        {
            var nullabilityInfo = GetNullabilityInfo(field);
            return nullabilityInfo.WriteState == NullabilityState.Nullable;
        }

        /// <summary>
        /// Checks if the property has a nullable annotation [?]
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool IsMarkedAsNullable(PropertyInfo property)
        {
            var nullabilityInfo = GetNullabilityInfo(property);
            return nullabilityInfo.WriteState == NullabilityState.Nullable;
        }

        /// <summary>
        /// Checks if the field has a nullable annotation [?]
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        internal static bool IsMarkedAsNullable(AbstractFieldInfo field)
            => field.IsMarkedAsNullable();

        public static bool IsNullable(this Type type)
        {
            return IsNullable(type, out _);
        }

        public static bool IsNullable(this Type type, [NotNullWhen(true)] out Type? underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);

            if (underlyingType == null)
            {
                return false;
            }

            return true;
        }
    }
}
