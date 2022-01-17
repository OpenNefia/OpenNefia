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
    public class UiMessageWindowBacking : BaseDrawable
    {
        public const int MessageBoxWidth = 192;
        public const int MessageBoxHeight = 72;
        public enum MessageBackingType
        {
            Default,
            Expanded
        }

        private IAssetInstance MessageWindowAsset = default!;
        private Love.SpriteBatch Batch = default!;
        private Love.SpriteBatch? SideBatch = default!;
        private Love.SpriteBatch? CornerBatch = default!;
        private MessageBackingType Type;
        public UiMessageWindowBacking(MessageBackingType type = MessageBackingType.Default)
        {
            Type = type;
        }

        private int GetSideCount()
        {
            return Height / MessageBoxWidth;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            var parts = new List<AssetBatchPart>();

            MessageWindowAsset = Assets.Get(Protos.Asset.MessageWindow);
            switch (Type)
            {
                default:
                    for (int x = 0; x < Width / MessageBoxWidth; x++)
                    {
                        parts.Add(new AssetBatchPart("topBar", (x * MessageBoxWidth), 0));
                        parts.Add(new AssetBatchPart("body", (x * MessageBoxWidth), 5));
                        parts.Add(new AssetBatchPart("bottomBar", (x * MessageBoxWidth), 5 + 62));
                    }
                    break;
                case MessageBackingType.Expanded:
                    for (int x = 0; x < Width / MessageBoxWidth; x++)
                    {
                        parts.Add(new AssetBatchPart("topBar", (x * MessageBoxWidth), 0));
                        for (int y = 0; y < Height / MessageBoxHeight; y++)
                        {
                            parts.Add(new AssetBatchPart("body", (x * MessageBoxWidth), 5 + (62 * y)));
                        }
                    }
                    var sideParts = new List<AssetBatchPart>();
                    for (int y = 0; y < GetSideCount(); y++)
                    {
                        sideParts.Add(new AssetBatchPart("topBar", y * MessageBoxWidth, 0));
                    }
                    SideBatch = MessageWindowAsset.MakeBatch(sideParts);
                    var corner = new List<AssetBatchPart>
                    {
                        new AssetBatchPart("corner", 0, 0)
                    };
                    CornerBatch = MessageWindowAsset.MakeBatch(corner);
                    break;
            }
            Batch = MessageWindowAsset.MakeBatch(parts);
        }


        public override void Draw()
        {
            Love.Graphics.Draw(Batch, X, Y);
            if (SideBatch != null)
                Love.Graphics.Draw(SideBatch, X, Y + (GetSideCount() * MessageBoxWidth), (float)Angle.FromDegrees(-90).Theta);
            if (CornerBatch != null)
                Love.Graphics.Draw(CornerBatch, X + 1, Y);
        }

        public override void Update(float dt)
        {
            
        }
    }
}
