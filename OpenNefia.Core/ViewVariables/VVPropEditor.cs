using OpenNefia.Core.UI.Wisp;
using System;

namespace OpenNefia.Core.ViewVariables
{
    /// <summary>
    ///     An editor for the value of a property.
    /// </summary>
    public abstract class VVPropEditor
    {
        /// <summary>
        ///     Invoked when the value was changed.
        /// </summary>
        internal event Action<object?, bool>? OnValueChanged;

        protected bool ReadOnly { get; private set; }

        public WispControl Initialize(object? value, bool readOnly)
        {
            ReadOnly = readOnly;
            return MakeUI(value);
        }

        protected abstract WispControl MakeUI(object? value);

        protected void ValueChanged(object? newValue, bool reinterpretValue = false)
        {
            OnValueChanged?.Invoke(newValue, reinterpretValue);
        }
    }
}
