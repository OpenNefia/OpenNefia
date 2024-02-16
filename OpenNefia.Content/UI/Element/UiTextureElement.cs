using OpenNefia.Content.UI;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI.Element
{
    /// <summary>
    /// UiElement that wraps a LÖVE2D texture.
    /// </summary>
    public class UiTextureElement : UiElement
    {
        public Love.Texture InnerTexture { get; }

        public UiTextureElement(Love.Texture innerTexture)
        {
            InnerTexture = innerTexture;
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = new(InnerTexture.GetWidth(), InnerTexture.GetHeight());
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Love.Color.White);
            GraphicsEx.DrawImageS(UIScale, InnerTexture, X, Y, Width, Height);
        }
    }
}
