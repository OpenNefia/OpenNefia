using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    /// <summary>
    /// Provides some convenient syntax for defining key forwards on UI classes that support them.
    /// </summary>
    public class InputForwardsWrapper : IInputForwarder
    {
        public IInputHandler Parent { get; }

        public InputForwardsWrapper(IInputHandler parent)
        {
            this.Parent = parent;
        }

        public static InputForwardsWrapper operator +(InputForwardsWrapper forwardsWrapper, IInputHandler child)
        {
            forwardsWrapper.ForwardTo(child);
            return forwardsWrapper;
        }

        public static InputForwardsWrapper operator -(InputForwardsWrapper forwardsWrapper, IInputHandler child)
        {
            forwardsWrapper.UnforwardTo(child);
            return forwardsWrapper;
        }

        public void Clear() => this.ClearAllForwards();

        public void ForwardTo(IInputHandler child, int? priority = null)
        {
            this.Parent.ForwardTo(child, priority);
        }

        public void UnforwardTo(IInputHandler child)
        {
            this.Parent.UnforwardTo(child);
        }

        public void ClearAllForwards()
        {
            this.Parent.ClearAllForwards();
        }
    }
}
