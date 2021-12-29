using OpenNefia.Core.Input;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Input
{
    [DataDefinition]
    public class KeyBindingRegistration
    {
        [DataField("function")]
        public BoundKeyFunction Function;
        [DataField("type")]
        public KeyBindingType Type = KeyBindingType.State;
        [DataField("key")]
        public Keyboard.Key BaseKey;
        [DataField("mod1")]
        public Keyboard.Key Mod1;
        [DataField("mod2")]
        public Keyboard.Key Mod2;
        [DataField("mod3")]
        public Keyboard.Key Mod3;
        [DataField("priority")]
        public int Priority;
        [DataField("canFocus")]
        public bool CanFocus;
        [DataField("canRepeat")]
        public bool CanRepeat;
        [DataField("allowSubCombs")]
        public bool AllowSubCombs;
        [DataField("repeatMode")]
        public KeyRepeatMode? RepeatMode;
    }
}
