using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaInfo
{
    /// <summary>
    /// Standalone key hint bar that's used in the character info menu.
    /// </summary>
    public sealed class UiKeyHintBar : UiElement
    {
        private const string KeyHintBarRegionID = "keyHintBar";

        private IAssetInstance AssetMessageWindow;
        private IAssetInstance AssetTipIcons;

        [Child] private UiText UiText;

        public string Text
        {
            get => UiText.Text;
            set => UiText.Text = value;
        }

        public UiKeyHintBar()
        {
            AssetMessageWindow = Assets.Get(Asset.MessageWindow);
            AssetTipIcons = Assets.Get(Asset.TipIcons);
            UiText = new UiTextShadowed(UiFonts.KeyHintBar);
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            UiText.GetPreferredSize(out size);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            UiText.SetSize(Width, Height);
            MinSize = UiText.Size;
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(X + 32, Y + 1);
        }

        public override void Update(float dt)
        {
            UiText.Update(dt);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Love.Color.White);

            var (keyHintBarWidth, _) = AssetMessageWindow.VirtualSize(UIScale, KeyHintBarRegionID);
            var count = Width / keyHintBarWidth;

            for (int i = 0; i < count; i++)
            {
                AssetMessageWindow.DrawRegion(UIScale, KeyHintBarRegionID, X + 8 + i * keyHintBarWidth, Y);
            }

            AssetTipIcons.DrawRegion(UIScale, "1", X, Y);

            UiText.Draw();
        }
    }
}
