namespace OpenNefia.Core.UI.Element
{
    public interface IUiInputElement : IDrawable, IUiThemeable, IUiDefaultSizeable, IUiInput
    {
        KeybindWrapper Keybinds { get; }
        TextInputWrapper TextInput { get; }
        InputForwardsWrapper Forwards { get; set; }
        MouseBindWrapper MouseButtons { get; }
        MouseMovedWrapper MouseMoved { get; set; }
    }
}
