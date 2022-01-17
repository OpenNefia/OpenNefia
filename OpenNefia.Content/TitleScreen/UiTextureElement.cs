using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.TitleScreen
{
    public sealed partial class RestoreSaveLayer
    {
        /// <summary>
        /// UiElement that wraps a LÖVE2D texture.
        /// </summary>
        private class UiTextureElement : UiElement
        {
            public Love.Texture InnerTexture { get; }

            public UiTextureElement(Love.Texture innerTexture)
            {
                InnerTexture = innerTexture;
            }

            public override void GetPreferredSize(out Vector2i size)
            {
                size = new(InnerTexture.GetWidth(), InnerTexture.GetHeight());
            }

            public override void Update(float dt)
            {
            }

            public override void Draw()
            {
                Love.Graphics.SetColor(Love.Color.White);
                GraphicsEx.DrawImage(InnerTexture, X, Y, Width, Height);
            }

            public override void Dispose()
            {
                InnerTexture.Dispose();
            }
        }
    }
}
