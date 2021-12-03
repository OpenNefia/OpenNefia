using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element.List
{
    public class UiListChoiceKey
    {
        public Keys Key { get; set; } = Keys.None;
        public bool UseKeybind = true;

        public UiListChoiceKey(Keys key = Keys.None, bool useKeybind = true)
        {
            Key = key;
            UseKeybind = useKeybind;
        }

        public static UiListChoiceKey MakeDefault(int index)
        {
            var key = Keys.A + index;
            if (key >= Keys.A && key <= Keys.R)
                return new UiListChoiceKey(key, true);

            return new UiListChoiceKey(Keys.None, false);
        }
    }
}
