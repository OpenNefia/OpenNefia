using OpenNefia.Core.Data.Types;

namespace OpenNefia.Core.UI.Hud
{
    public interface IHudMessageWindow : IHudWidget
    {
        void Print(string queryText, Maths.Color? color = null);
    }
}