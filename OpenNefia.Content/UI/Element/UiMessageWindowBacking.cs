using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
using static OpenNefia.Core.Rendering.AssetInstance;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.UI.Element
{
    public class UiMessageWindowBacking : UiElement
    {
        public enum MessageBackingType
        {
            Default,
            Expanded
        }

        private IAssetInstance AssetMessageWindow = default!;
        private Love.SpriteBatch Batch = default!;
        private Love.SpriteBatch? SideBatch = default!;
        private Love.SpriteBatch? CornerBatch = default!;
        private MessageBackingType Type;

        private const string TopBarName = "topBar";
        private const string BodyName = "body";
        private const string BottomBarName = "bottomBar";
        private const string CornerName = "corner";

        public UiMessageWindowBacking(MessageBackingType type = MessageBackingType.Default)
        {
            Type = type;
        }

        private int GetSideCount()
        {
            return PixelHeight / AssetMessageWindow.PixelWidth;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            var parts = new List<AssetBatchPart>();

            AssetMessageWindow = Assets.Get(Protos.Asset.MessageWindow);
            var mboxPixelWidth = AssetMessageWindow.PixelWidth;

            switch (Type)
            {
                default:
                    for (int x = 0; x < PixelWidth / mboxPixelWidth; x++)
                    {
                        parts.Add(new AssetBatchPart(TopBarName, (x * mboxPixelWidth), 0));
                        parts.Add(new AssetBatchPart(BodyName, (x * mboxPixelWidth), 5));
                        parts.Add(new AssetBatchPart(BottomBarName, (x * mboxPixelWidth), 5 + 62));
                    }
                    break;
                case MessageBackingType.Expanded:
                    for (int x = 0; x < PixelWidth / mboxPixelWidth; x++)
                    {
                        parts.Add(new AssetBatchPart(TopBarName, (x * mboxPixelWidth), 0));
                        for (int y = 0; y < PixelHeight / mboxPixelWidth; y++)
                        {
                            parts.Add(new AssetBatchPart(BodyName, (x * mboxPixelWidth), 5 + (62 * y)));
                        }
                    }

                    var sideParts = new List<AssetBatchPart>();
                    for (int y = 0; y < GetSideCount(); y++)
                    {
                        sideParts.Add(new AssetBatchPart(TopBarName, y * mboxPixelWidth, 0));
                    }

                    SideBatch?.Dispose();
                    SideBatch = AssetMessageWindow.MakeBatch(sideParts);

                    var corner = new List<AssetBatchPart>
                    {
                        new AssetBatchPart(CornerName, 0, 0)
                    };

                    CornerBatch?.Dispose();
                    CornerBatch = AssetMessageWindow.MakeBatch(corner);

                    break;
            }

            Batch?.Dispose();
            Batch = AssetMessageWindow.MakeBatch(parts);
        }

        public override void Draw()
        {
            GraphicsS.DrawS(UIScale, Batch, X, Y);

            if (SideBatch != null)
            {
                GraphicsS.DrawS(UIScale,
                    SideBatch,
                    X,
                    Y + (GetSideCount() * AssetMessageWindow.VirtualWidth(UIScale)),
                    (float)Angle.FromDegrees(-90).Theta);
            }

            if (CornerBatch != null)
            {
                GraphicsS.DrawS(UIScale, CornerBatch, X + 1, Y);
            }
        }

        public override void Update(float dt)
        {
            
        }
    }
}
