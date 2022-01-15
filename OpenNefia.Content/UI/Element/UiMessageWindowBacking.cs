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
        private MessageBackingType Type;
        public UiMessageWindowBacking(MessageBackingType type = MessageBackingType.Default)
        {
            Type = type;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            var parts = new List<AssetBatchPart>();

            switch (Type)
            {
                default:
                    for (int x = 0; x < Width / MessageBoxWidth; x++)
                    {
                        parts.Add(new AssetBatchPart("topBar", X + (x * MessageBoxWidth), Y));
                        parts.Add(new AssetBatchPart("body", X + (x * MessageBoxWidth), Y + 5));
                        parts.Add(new AssetBatchPart("bottomBar", X + (x * MessageBoxWidth), Y + 5 + 62));
                    }
                    break;
            }

            MessageWindowAsset = Assets.Get(Protos.Asset.MessageWindow);
            Batch = MessageWindowAsset.MakeBatch(parts);
        }

        public override void Draw()
        {
            Love.Graphics.Draw(Batch, X, Y);
        }

        public override void Update(float dt)
        {
            
        }
    }
}
