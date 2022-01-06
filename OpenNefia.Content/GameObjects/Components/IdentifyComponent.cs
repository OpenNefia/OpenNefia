using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class IdentifyComponent : Component, IComponentLocalizable
    {
        public override string Name => "Identify";

        [Localize]
        public string UnidentifiedName { get; private set; } = string.Empty;

        [DataField]
        public int IdentifyDifficulty { get; set; } = 0;

        [DataField]
        public IdentifyState IdentifyState { get; set; } = IdentifyState.None;

        void IComponentLocalizable.LocalizeFromLua(NLua.LuaTable table)
        {
            UnidentifiedName = table.GetStringOrEmpty(nameof(UnidentifiedName));
        }
    }

    public enum IdentifyState : byte
    {
        None = 0,
        Name = 1,
        Quality = 2,
        Full = 3
    }
}
