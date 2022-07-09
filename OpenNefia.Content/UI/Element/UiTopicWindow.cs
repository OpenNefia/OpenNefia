using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using static OpenNefia.Core.Rendering.AssetInstance;

namespace OpenNefia.Content.UI.Element
{
    public class UiTopicWindow : UiElement
    {
        public enum FrameStyleKind : int
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5
        }

        public enum WindowStyleKind : int
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6
        }

        protected FrameStyleKind FrameStyle;
        protected WindowStyleKind WindowStyle;

        private readonly Color ColorStyle0 = UiColors.TopicWindowStyle0;
        private readonly Color ColorStyle1 = UiColors.TopicWindowStyle1;
        private readonly Color ColorStyle2 = UiColors.TopicWindowStyle2;
        private readonly Color ColorStyle3 = UiColors.TopicWindowStyle3;
        private readonly Color ColorStyle4 = UiColors.TopicWindowStyle4;
        private readonly Color ColorStyle5 = UiColors.TopicWindowStyle5;
        private readonly Color ColorStyle6 = UiColors.TopicWindowStyle6;

        protected IAssetInstance AssetTopicWindow;
        protected IAssetInstance AssetWindow;

        protected Love.SpriteBatch TopicWindowBatch;

        private IAssetInstance GetTopicWindowAsset(FrameStyleKind frameStyle)
        {
            switch (frameStyle)
            {
                case FrameStyleKind.Zero:
                default:
                    return Assets.GetSized(Protos.Asset.TopicWindow0, PixelSize);
                case FrameStyleKind.One:
                    return Assets.GetSized(Protos.Asset.TopicWindow1, PixelSize);
                case FrameStyleKind.Two:
                    return Assets.GetSized(Protos.Asset.TopicWindow2, PixelSize);
                case FrameStyleKind.Three:
                    return Assets.GetSized(Protos.Asset.TopicWindow3, PixelSize);
                case FrameStyleKind.Four:
                    return Assets.GetSized(Protos.Asset.TopicWindow4, PixelSize);
                case FrameStyleKind.Five:
                    return Assets.GetSized(Protos.Asset.TopicWindow5, PixelSize);
            }
        }

        public UiTopicWindow(FrameStyleKind frameStyle = FrameStyleKind.One, WindowStyleKind windowStyle = WindowStyleKind.One)
        {
            FrameStyle = frameStyle;
            WindowStyle = windowStyle;

            AssetTopicWindow = GetTopicWindowAsset(FrameStyle);
            AssetWindow = Assets.Get(Protos.Asset.Window);

            TopicWindowBatch = MakeBatch();
        }

        private Love.SpriteBatch MakeBatch()
        {
            var parts = new List<AssetBatchPart>();

            for (int i = 0; i < PixelWidth / 16 - 1; i++)
            {
                parts.Add(new AssetBatchPart("top_mid", i * 16 + 16, 0));
                parts.Add(new AssetBatchPart("bottom_mid", i * 16 + 16, PixelHeight - 16));
            }

            var innerX = PixelWidth / 16 * 16 - 16;
            var innerY = PixelHeight / 16 * 16 - 16;

            parts.Add(new AssetBatchPart("top_mid2", innerX, 0));
            parts.Add(new AssetBatchPart("bottom_mid2", innerX, PixelHeight - 16));

            for (int i = 0; i < PixelHeight / 16 - 1; i++)
            {
                parts.Add(new AssetBatchPart("left_mid", 0, i * 16 + 16));
                parts.Add(new AssetBatchPart("right_mid", PixelWidth - 16, i * 16 + 16));
            }

            parts.Add(new AssetBatchPart("left_mid2", 0, innerY));
            parts.Add(new AssetBatchPart("right_mid2", PixelWidth - 16, innerY));

            parts.Add(new AssetBatchPart("top_left", 0, 0));
            parts.Add(new AssetBatchPart("bottom_left", 0, PixelHeight - 16));
            parts.Add(new AssetBatchPart("top_right", PixelWidth - 16, 0));
            parts.Add(new AssetBatchPart("bottom_right", PixelWidth - 16, PixelHeight - 16));

            AssetTopicWindow = GetTopicWindowAsset(FrameStyle);
            return AssetTopicWindow.MakeBatch(parts);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            TopicWindowBatch = MakeBatch();
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            if (WindowStyle == WindowStyleKind.Six)
            {
                GraphicsEx.SetColor(ColorStyle6);
                GraphicsEx.DrawSpriteBatchS(UIScale, TopicWindowBatch, X, Y, Width - 4, Height - 4);
            }
            else if (WindowStyle != WindowStyleKind.Five)
            {
                var rect = true;

                switch (WindowStyle)
                {
                    case WindowStyleKind.Zero:
                    default:
                        rect = false;
                        GraphicsEx.SetColor(ColorStyle0);
                        break;
                    case WindowStyleKind.One:
                        GraphicsEx.SetColor(ColorStyle1);
                        break;
                    case WindowStyleKind.Two:
                        GraphicsEx.SetColor(ColorStyle2);
                        break;
                    case WindowStyleKind.Three:
                        GraphicsEx.SetColor(ColorStyle3);
                        break;
                    case WindowStyleKind.Four:
                        GraphicsEx.SetColor(ColorStyle4);
                        break;
                }

                if (rect)
                {
                    Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X + 4, Y + 4, Width - 4, Height - 4);
                    Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
                }

                AssetWindow.DrawRegion(UIScale, "fill", X, Y, Width - 2, Height - 2);
            }

            GraphicsEx.SetColor(Love.Color.White);
            GraphicsEx.DrawSpriteBatchS(UIScale, TopicWindowBatch, X, Y);

            if (WindowStyle == WindowStyleKind.Five)
            {
                GraphicsEx.SetColor(255 - ColorStyle5.RByte, 255 - ColorStyle5.GByte, 255 - ColorStyle5.BByte, 255);
                AssetWindow.DrawRegion(UIScale, "fill", X + 2, Y + 2, Width - 4, Height - 4);

                Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                GraphicsEx.SetColor(20, 20, 20, 255);
                GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X + 2, Y + 2, Width - 4, Height - 4);
                Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            }
        }

        public override void Dispose()
        {
            TopicWindowBatch.Dispose();
        }
    }
}
