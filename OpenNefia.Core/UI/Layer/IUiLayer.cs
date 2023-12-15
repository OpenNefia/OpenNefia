using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI.Layer
{
    public interface IUiLayer : IDrawable, IUiElement, ILocalizable
    {
        int ZOrder { get; set; }
        bool ExceptionTolerance { get; }

        // TODO these should not be out parameters.
        void GetPreferredBounds(out UIBox2 bounds);
        void GetPreferredPosition(out Vector2 pos);
        void SetPreferredPosition();

        void OnQuery();
        void OnQueryFinish();
        bool IsQuerying();
        bool IsInActiveLayerList();
        void Localize();
    }
}
