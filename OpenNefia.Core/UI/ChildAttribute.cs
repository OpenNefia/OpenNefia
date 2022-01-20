using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    /// <summary>
    /// Indicates that a field or property of a <see cref="UiElement"/> should
    /// be parented to that element on initialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ChildAttribute : Attribute
    {
    }
}
