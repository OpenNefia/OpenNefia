using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace OpenNefia.Core.ViewVariables
{
    public static class ViewVariablesUtility
    {
        /// <summary>
        ///     Check whether this type has any fields or properties with the <see cref="ViewVariablesAttribute"/>,
        ///     i.e. whether there are any members that are visible in a VV window.
        /// </summary>
        /// <remarks>
        ///     This is quite an expensive operation, don't use it lightly.
        /// </remarks>
        /// <param name="type">The type to check.</param>
        /// <returns>True if there are members with the attribute, false otherwise.</returns>
        public static bool TypeHasVisibleMembers(Type type)
        {
            return type.GetAllFields().Cast<MemberInfo>().Concat(type.GetAllProperties())
                .Any(f => TryGetViewVariablesAccess(f, out _));
        }

        /// <summary>
        /// Gets the <see cref="VVAccess"/> defined for the member, if defined.
        /// </summary>
        /// <param name="info">The member to check access for.</param>
        /// <param name="access">The found access. Will be null if no access is defined</param>
        /// <returns>True if access is defined, false if not.</returns>
        public static bool TryGetViewVariablesAccess(MemberInfo info, [NotNullWhen(true)] out VVAccess? access)
        {
            if (info.TryGetCustomAttribute<ViewVariablesAttribute>(out var vv))
            {
                access = vv.Access;
                return true;
            }

            if (info is PropertyInfo propertyInfo)
            {
                if (info.HasCustomAttribute<DataFieldAttribute>())
                {
                    if (propertyInfo.CanWrite)
                    {
                        access = VVAccess.ReadWrite;
                    }
                    else
                    {
                        access = VVAccess.ReadOnly;
                    }
                    return true;
                }

                var getter = propertyInfo.GetGetMethod();
                var getterIsPublic = getter != null && getter.IsPublic;
                var setter = propertyInfo.GetSetMethod(true);
                var setterIsPublic = setter != null && setter.IsPublic;

                if (setter != null && setter.GetParameters().Length != 1)
                {
                    // otherwise it errors on Invoke(obj, new[] {v})
                    access = null;
                    return false;
                }

                if (getterIsPublic)
                {
                    if (setterIsPublic)
                        access = VVAccess.ReadWrite;
                    else
                        access = VVAccess.ReadOnly;

                    return true;
                }

                access = null;
                return false;
            }

            if (info.HasCustomAttribute<DataFieldAttribute>())
            {
                access = VVAccess.ReadOnly;
                return true;
            }

            access = null;
            return false;
        }
    }
}
