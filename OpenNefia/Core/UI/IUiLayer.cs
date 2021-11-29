using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI
{
    public interface IUiLayer : IUiInputElement, ILocalizable
    {
        int ZOrder { get; set; }

        void GetPreferredBounds(out Box2i bounds);
        void OnQuery();
        void OnQueryFinish();
        bool IsQuerying();
    }
}
