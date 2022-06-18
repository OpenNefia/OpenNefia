using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Wisp.Styling
{
    /// <summary>
    /// Helper functions for use with NLua binding in stylesheet .lua files.
    /// 
    /// TODO: expose this privately
    /// </summary>
    public static class StylesheetUtilities
    {
        public static IAssetInstance GetAssetInstance(string id)
        {
            return IoCManager.Resolve<IAssetManager>().GetAsset(new(id));
        }
    }
}
