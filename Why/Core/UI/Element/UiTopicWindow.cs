using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Rendering;
using System.Collections.Generic;
using static OpenNefia.Core.Rendering.AssetDrawable;

namespace OpenNefia.Core.UI.Element
{
    public class UiTopicWindow : BaseDrawable
    {
        public enum FrameStyle : int
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5
        }

        public enum WindowStyle : int
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6
        }

        protected FrameStyle FrameStyle_;
        protected WindowStyle WindowStyle_;

        protected AssetDrawable AssetTopicWindow;
        protected AssetDrawable AssetWindow;
        protected ColorDef ColorTopicWindowStyle0;
        protected ColorDef ColorTopicWindowStyle1;
        protected ColorDef ColorTopicWindowStyle2;
        protected ColorDef ColorTopicWindowStyle3;
        protected ColorDef ColorTopicWindowStyle4;
        protected ColorDef ColorTopicWindowStyle5;
        protected ColorDef ColorTopicWindowStyle6;
        protected Love.SpriteBatch TopicWindowBatch;

        private AssetDrawable GetTopicWindowAsset(FrameStyle frameStyle)
        {
            switch (frameStyle)
            {
                case FrameStyle.Zero:
                default:
                    return new AssetDrawable(AssetDefOf.TopicWindow0, this.Width, this.Height);
                case FrameStyle.One:
                    return new AssetDrawable(AssetDefOf.TopicWindow1, this.Width, this.Height);
                case FrameStyle.Two:
                    return new AssetDrawable(AssetDefOf.TopicWindow2, this.Width, this.Height);
                case FrameStyle.Three:
                    return new AssetDrawable(AssetDefOf.TopicWindow3, this.Width, this.Height);
                case FrameStyle.Four:
                    return new AssetDrawable(AssetDefOf.TopicWindow4, this.Width, this.Height);
                case FrameStyle.Five:
                    return new AssetDrawable(AssetDefOf.TopicWindow5, this.Width, this.Height);
            }
        }

        public UiTopicWindow(FrameStyle frameStyle = FrameStyle.One, WindowStyle windowStyle = WindowStyle.One)
        {
            this.FrameStyle_ = frameStyle;
            this.WindowStyle_ = windowStyle;

            this.AssetTopicWindow = this.GetTopicWindowAsset(this.FrameStyle_);
            this.AssetWindow = new AssetDrawable(AssetDefOf.Window);
            this.ColorTopicWindowStyle0 = ColorDefOf.TopicWindowStyle0;
            this.ColorTopicWindowStyle1 = ColorDefOf.TopicWindowStyle1;
            this.ColorTopicWindowStyle2 = ColorDefOf.TopicWindowStyle2;
            this.ColorTopicWindowStyle3 = ColorDefOf.TopicWindowStyle3;
            this.ColorTopicWindowStyle4 = ColorDefOf.TopicWindowStyle4;
            this.ColorTopicWindowStyle5 = ColorDefOf.TopicWindowStyle5;
            this.ColorTopicWindowStyle6 = ColorDefOf.TopicWindowStyle6;

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

            this.AssetTopicWindow = this.GetTopicWindowAsset(this.FrameStyle_);
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
            if (this.WindowStyle_ == WindowStyle.Six)
            {
                GraphicsEx.SetColor(this.ColorTopicWindowStyle6);
                GraphicsEx.DrawSpriteBatch(this.TopicWindowBatch, this.X, this.Y, this.Width - 4, this.Height - 4);
            }
            else
            {
                var rect = true;

                switch (this.WindowStyle_)
                {
                    case WindowStyle.Zero:
                        rect = false;
                        GraphicsEx.SetColor(this.ColorTopicWindowStyle0);
                        break;
                    case WindowStyle.One:
                        GraphicsEx.SetColor(this.ColorTopicWindowStyle1);
                        break;
                    case WindowStyle.Two:
                        GraphicsEx.SetColor(this.ColorTopicWindowStyle2);
                        break;
                    case WindowStyle.Three:
                        GraphicsEx.SetColor(this.ColorTopicWindowStyle3);
                        break;
                    case WindowStyle.Four:
                        GraphicsEx.SetColor(this.ColorTopicWindowStyle4);
                        break;
                    case WindowStyle.Five:
                    default:
                        GraphicsEx.SetColor(this.ColorTopicWindowStyle5);
                        break;
                }

                if (rect)
                {
                    Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                    GraphicsEx.FilledRect(this.X + 4, this.Y + 4, this.Width - 4, this.Height - 4);
                    Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
                }
            }

            this.AssetWindow.DrawRegion("fill", this.X + 4, this.Y + 4, this.Width - 6, this.Height - 8);

            GraphicsEx.SetColor(Love.Color.White);
            GraphicsEx.DrawSpriteBatch(this.TopicWindowBatch, this.X, this.Y);

            if (this.WindowStyle_ == WindowStyle.Five)
            {
                GraphicsEx.SetColor(this.ColorTopicWindowStyle5);
                GraphicsEx.DrawSpriteBatch(this.TopicWindowBatch, this.X + 2, this.Y + 2, this.Width - 4, this.Height - 5);

                Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
                GraphicsEx.FilledRect(this.X + 4, this.Y + 4, this.Width - 4, this.Height - 4);
                Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            }
        }

        public override void Dispose()
        {
            this.AssetTopicWindow.Dispose();
            this.AssetWindow.Dispose();
            this.TopicWindowBatch.Dispose();
        }
    }
}
