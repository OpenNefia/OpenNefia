using OpenNefia.Core.Maths;

namespace OpenNefia.Content.UI.Hud
{
    public interface IHudMessageWindow : IHudWidget
    {
        void Print(string queryText, Color? color = null, bool newLine = true);
    }
}