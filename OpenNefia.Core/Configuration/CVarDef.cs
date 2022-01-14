using System;
using JetBrains.Annotations;

namespace OpenNefia.Core.Configuration
{
    public class CVarDef
    {
        public object DefaultValue { get; }
        public Type Type { get; }
        public CVar Flags { get; }
        public string Name { get; }
        public string? Desc { get; }

        private protected CVarDef(string name, Type type, object defaultValue, CVar flags, string? desc)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Flags = flags;
            Desc = desc;
        }

        public static CVarDef<T> Create<T>(
            string name,
            T defaultValue,
            CVar flag = CVar.None,
            string? desc = null) where T : notnull
        {
            return new(name, defaultValue, flag, desc);
        }
    }

    public sealed class CVarDef<T> : CVarDef where T : notnull
    {
        public new T DefaultValue { get; }

        internal CVarDef(string name, T defaultValue, CVar flags, string? desc)
            : base(name, typeof(T), defaultValue, flags, desc)
        {
            DefaultValue = defaultValue;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public sealed class CVarDefsAttribute : Attribute
    {

    }
}
