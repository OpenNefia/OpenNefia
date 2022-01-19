using Love;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;
using System.Collections.Generic;
using System.Globalization;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.TitleScreen
{
    public class TitleScreenBGLayer : UiLayer
    {
        private IAssetInstance AssetTitle;

        public TitleScreenBGLayer()
        {
            AssetTitle = Assets.Get(Asset.Title);
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(Love.Color.White);
            AssetTitle.Draw(UIScale, X, Y, Width, Height);
        }
    }
}