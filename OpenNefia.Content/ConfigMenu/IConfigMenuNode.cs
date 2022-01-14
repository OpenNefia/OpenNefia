using OpenNefia.Core.Configuration;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.ConfigMenu
{
    /// <summary>
    /// <para>
    /// A UI-agnostic, declarative datastructure for specifying the layout of a config menu.
    /// </para>
    /// <para>
    /// The problem this is meant to solve is this: It should be possible to theme/replace
    /// significant portions of the UI with new implementations, for example ones based on XAML
    /// or another layouting engine. However, this does *not* mean that it should be necessary
    /// to rewrite the layout of the config menu for every possible UI framework. Mods should be
    /// able to specify their config hierarchy in a single place, and leave it up to the UI
    /// implementation to config that options hierarchy for rendering and such.
    /// </para>
    /// <para>
    /// All implementers of this interface are intended to be used in YAML within a
    /// <see cref="ConfigMenuItemPrototype"/>, which is where the single specification for the
    /// config menus should take place.
    /// </para>
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IConfigMenuNode
    {
    }

    public interface IConfigCVarMenuNode : IConfigMenuNode
    {
        CVarDef CVar { get; }
    }

    public interface IConfigCVarMenuNode<T> : IConfigMenuNode
        where T: notnull
    {
        CVarDef<T> CVar { get; }
    }

    public sealed class ConfigSubmenuMenuNode : IConfigMenuNode
    {
        [DataField]
        public Vector2i MenuSize { get; } = new(440, 300);

        [DataField("items")]
        private readonly List<PrototypeId<ConfigMenuItemPrototype>> _items = new();

        public IReadOnlyList<PrototypeId<ConfigMenuItemPrototype>> Items => _items;
    }

    public sealed class ConfigStringMenuNode : IConfigCVarMenuNode<string>
    {
        [DataField("cvar", required: true)]
        public CVarDef<string> CVar { get; } = default!;
    }

    public sealed class ConfigMultilineStringMenuNode : IConfigCVarMenuNode<string>
    {
        [DataField("cvar", required: true)]
        public CVarDef<string> CVar { get; } = default!;
    }

    public sealed class ConfigIntMenuNode : IConfigCVarMenuNode<int>
    {
        [DataField("cvar", required: true)]
        public CVarDef<int> CVar { get; } = default!;

        [DataField]
        public int Min { get; } = int.MinValue;

        [DataField]
        public int Max { get; } = int.MaxValue;
    }

    public sealed class ConfigBoolMenuNode : IConfigCVarMenuNode<bool>
    {
        [DataField("cvar", required: true)]
        public CVarDef<bool> CVar { get; } = default!;
    }

    public sealed class ConfigEnumMenuNode : IConfigCVarMenuNode
    {
        [DataField("cvar", required: true)]
        public CVarDef CVar { get; } = default!;

        [DataField(required: true)]
        public Type EnumType { get; } = default!;
    }

    #region Special Nodes

    /// <summary>
    /// This config option should provide a list of resolutions taken
    /// from the operating system.
    /// </summary>
    public sealed class ConfigScreenResolutionMenuNode : IConfigMenuNode
    {
        /// <summary>
        /// Typically "display.width".
        /// </summary>
        [DataField("cvarWidth", required: true)]
        public CVarDef<int> CVarWidth { get; } = default!;

        /// <summary>
        /// Typically "display.height".
        /// </summary>
        [DataField("cvarHeight", required: true)]
        public CVarDef<int> CVarHeight { get; } = default!;
    }

    /// <summary>
    /// This config option should provide a set of available MIDI device
    /// numbers on the system.
    /// </summary>
    public sealed class ConfigMidiDeviceMenuNode : IConfigMenuNode
    {
        /// <summary>
        /// Typically "audio.mididevice".
        /// </summary>
        [DataField("cvar", required: true)]
        public CVarDef<int> CVar { get; } = default!;
    }

    /// <summary>
    /// This config option should provide the list of all available
    /// prototypes of the given type.
    /// </summary>
    public sealed class ConfigPrototypeIdsMenuNode : IConfigMenuNode
    {
        [DataField("cvar", required: true)]
        public CVarDef<string> CVar { get; } = default!;

        [DataField(required: true)]
        public string PrototypeType { get; } = default!;

        /// <summary>
        /// Locale key in the prototype's locale namespace to use as the
        /// display name.
        /// </summary>
        [DataField]
        public string NameLocaleKey { get; } = "Name";
    }

    #endregion
}