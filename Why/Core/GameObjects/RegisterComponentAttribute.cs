using System;
using JetBrains.Annotations;

namespace Why.Core.GameObjects
{
    /// <summary>
    ///     Marks a component as being automatically registered by <see cref="IComponentFactory.DoAutoRegistrations" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [BaseTypeRequired(typeof(IComponent))]
    [MeansImplicitUse]
    public sealed class RegisterComponentAttribute : Attribute
    {
    }
}
