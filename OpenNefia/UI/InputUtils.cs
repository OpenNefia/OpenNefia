using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    internal static class InputUtils
    {
        public static Keys? GetModifier(Love.KeyConstant key)
        {
            switch (key)
            {
                case Love.KeyConstant.LCtrl:
                case Love.KeyConstant.RCtrl:
                    return Keys.Ctrl;
                case Love.KeyConstant.LShift:
                case Love.KeyConstant.RShift:
                    return Keys.Shift;
                case Love.KeyConstant.LAlt:
                case Love.KeyConstant.RAlt:
                    return Keys.Alt;
                case Love.KeyConstant.LGUI:
                case Love.KeyConstant.RGUI:
                    return Keys.GUI;
                default:
                    return null;
            }
        }
    }
}
