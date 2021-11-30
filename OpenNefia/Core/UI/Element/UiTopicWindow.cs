using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using static OpenNefia.Core.Rendering.AssetDrawable;

namespace OpenNefia.Core.UI.Element
{
    public class UiTopicWindow : BaseDrawable
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

        protected IAssetDrawable AssetTopicWindow;
        protected IAssetDrawable AssetWindow;

        protected Love.SpriteBatch TopicWindowBatch;

        private IAssetDrawable GetTopicWindowAsset(FrameStyleKind frameStyle)
        {
            switch (frameStyle)
            {
                case FrameStyleKind.Zero:
                default:
                    return Assets.GetSized(AssetPrototypeOf.TopicWindow0, this.Size);
                case FrameStyleKind.One:
                    return Assets.GetSized(AssetPrototypeOf.TopicWindow1, this.Size);
                case FrameStyleKind.Two:
                    return Assets.GetSized(AssetPrototypeOf.TopicWindow2, this.Size);
                case FrameStyleKind.Three:
                    return Assets.GetSized(AssetPrototypeOf.TopicWindow3, this.Size);
                case FrameStyleKind.Four:
                    return Assets.GetSized(AssetPrototypeOf.TopicWindow4, this.Size);
                case FrameStyleKind.Five:
                    return Assets.GetSized(AssetPrototypeOf.TopicWindow5, this.Size);
            }
        }

        public UiTopicWindow(FrameStyleKind frameStyle = FrameStyleKind.One, WindowStyleKind windowStyle = WindowStyleKind.One)
        {
            this.FrameStyle = frameStyle;
            this.WindowStyle = windowStyle;

            this.AssetTopicWindow = this.GetTopicWindowAsset(this.FrameStyle);
            this.AssetWindow = Assets.Get(AssetPrototypeOf.Window);

            this.TopicWindowBatch = this.MakeBatch();
        }

        private Love.SpriteBatch MakeBatch()
        {
            var parts = new List<AssetBatchPart>();

            for (int i = 0; i < this.Width / 16 - 1; i++)
            {
                parts.Add(new AssetBatchPart("top_mid", i * 16 + 16, 0));
                parts.Add(new AssetBatchPart("bottom_mid", i * 16 + 16, this.Height - 16));
            }

            var innerX = this.Width / 16 * 16 - 16;
            var innerY = this.Height / 16 * 16 - 16;

            parts.Add(new AssetBatchPart("top_mid2", innerX, 0));
            parts.Add(new AssetBatchPart("bottom_mid2", innerX, this.Height - 16));

            for (int i = 0; i < this.Height / 16 - 1; i++)
            {
                parts.Add(new AssetBatchPart("left_mid", 0, i * 16 + 16));
                parts.Add(new AssetBatchPart("right_mid", this.Width - 16, i * 16 + 16));
            }

            parts.Add(new AssetBatchPart("left_mid2", 0, innerY));
            parts.Add(new AssetBatchPart("right_mid2", this.Width - 16, innerY));

            parts.Add(new AssetBatchPart("top_left", 0, 0));
            parts.Add(new AssetBatchPart("bottom_left", 0, this.Height - 16));
            parts.Add(new AssetBatchPart("top_right", this.Width - 16, 0));
            parts.Add(new AssetBatchPart("bottom_right", this.Width - 16, this.Height - 16));

            this.AssetTopicWindow = this.GetTopicWindowAsset(this.FrameStyle);
            return this.AssetTopicWindow.MakeBatch(parts);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            this.TopicWindowBatch = this.MakeBatch();
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            if (this.WindowStyle == WindowStyleKind.Six)
            {
                GraphicsEx.SetColor(this.ColorStyle6);
                GraphicsEx.DrawSpriteBatch(this.TopicWindowBatch, this.X, this.Y, this.Width - 4, this.Height - 4);
            }
            else
            {
                var rect = true;

                switch (this.WindowStyle)
                {
                    case WindowStyleKind.Zero:
                        rect = false;
                        GraphicsEx.SetColor(this.ColorStyle0);
                        break;
                    case WindowStyleKind.One:
                        GraphicsEx.SetColor(this.ColorStyle1);
                        break;
                    case WindowStyleKind.Two:
                        GraphicsEx.SetColor(this.ColorStyle2);
                        break;
                    case WindowStyleKind.Three:
                        GraphicsEx.SetColor(this.ColorStyle3);
                        break;
                    case WindowStyleKind.Four:
                        GraphicsEx.SetColor(this.ColorStyle4);
                        break;
                    case WindowStyleKind.Five:
                    default:
                        GraphicsEx.SetColor(this.ColorStyle5);
                        break;
                }

                if (rect)
                {
                    Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                    Love.Graphics.Rectangle(Love.DrawMode.Fill, this.X + 4, this.Y + 4, this.Width - 4, this.Height - 4);
                    Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
                }
            }

            this.AssetWindow.DrawRegion("fill", this.X + 4, this.Y + 4, this.Width - 6, this.Height - 8);

            GraphicsEx.SetColor(Love.Color.White);
            GraphicsEx.DrawSpriteBatch(this.TopicWindowBatch, this.X, this.Y);

            if (this.WindowStyle == WindowStyleKind.Five)
            {
                GraphicsEx.SetColor(this.ColorStyle5);
                GraphicsEx.DrawSpriteBatch(this.TopicWindowBatch, this.X + 2, this.Y + 2, this.Width - 4, this.Height - 5);

                Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, this.X + 4, this.Y + 4, this.Width - 4, this.Height - 4);
                Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            }
        }

        public override void Dispose()
        {
            this.TopicWindowBatch.Dispose();
        }
    }
}
