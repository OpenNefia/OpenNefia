using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public interface IInputForwarder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="priority"></param>
        void ForwardTo(IInputHandler keys, int? priority = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        void UnforwardTo(IInputHandler keys);

        /// <summary>
        /// 
        /// </summary>
        void ClearAllForwards();
    }
}
