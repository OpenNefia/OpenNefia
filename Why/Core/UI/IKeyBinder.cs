using OpenNefia.Core.Data.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public interface IKeyBinder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        void BindKey(IKeybind keybind, Action<KeyInputEvent> func, bool trackReleased = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        void UnbindKey(IKeybind keybind);
    }
}
