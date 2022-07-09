namespace OpenNefia.Core.UserInterface
{
    public interface IClipboardManager
    {
        string GetText();
        void SetText(string text);
    }
}