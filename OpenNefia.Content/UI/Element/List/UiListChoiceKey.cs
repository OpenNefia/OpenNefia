using OpenNefia.Core.Input;
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
        public Keyboard.Key Key { get; set; } = Keyboard.Key.Unknown;
        public bool UseKeybind = true;

        public UiListChoiceKey(Keyboard.Key key = Keyboard.Key.Unknown, bool useKeybind = true)
        {
            Key = key;
            UseKeybind = useKeybind;
        }

        public static UiListChoiceKey MakeDefault(int index)
        {
            var key = Keyboard.Key.A + (byte)index;
            if (key >= Keyboard.Key.A && key <= Keyboard.Key.R)
                return new UiListChoiceKey(key, true);

            return new UiListChoiceKey(Keyboard.Key.Unknown, false);
        }
    }
}
