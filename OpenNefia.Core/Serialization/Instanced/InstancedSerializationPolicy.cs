using OdinSerializer;
using OpenNefia.Core.Utility;
using System.Reflection;

namespace OpenNefia.Core.Serialization.Instanced
{
    internal class InstancedSerializationPolicy : ISerializationPolicy
    {
        public string ID => $"OpenNefia.{nameof(InstancedSerializationPolicy)}";

        public bool AllowNonSerializableTypes => false;

        public bool ShouldSerializeMember(MemberInfo member)
        {
            if (member.HasCustomAttribute<NonSerializedAttribute>())
            {
                return false;
            }

            if (member is FieldInfo)
            {
                return true;
            }

            if (member is PropertyInfo)
            {
                return member.DeclaringType?.HasBackingField(member.Name) ?? false;
            }

            return false;
        }
    }
}