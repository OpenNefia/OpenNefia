using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Locale
{
    public interface IComponentLocalizable
    {
        /// <summary>
        /// Method for localizing fields on this component from its
        /// corresponding localization data in Lua.
        /// </summary>
        /// <param name="table">Table containing the localization data.</param>
        void LocalizeFromLua(LuaTable table) {}
    }
}
