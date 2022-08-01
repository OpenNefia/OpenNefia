using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public class DialogArgs
    {
    }

    public class DialogResult
    {
    }
    
    public sealed class DialogChoice
    {
        public string Text { get; set; } = string.Empty;
    }

    public interface IDialogLayer : IUiLayerWithResult<DialogArgs, DialogResult>
    {
        void SetDialogData(string text, List<DialogChoice> choices);
    }

    public sealed class DialogLayer : UiLayerWithResult<DialogArgs, DialogResult>, IDialogLayer
    {
        private IAssetInstance _assetIeChat = default!;
        private IAssetInstance _assetImpressionIcon = default!;
        
        private TileAtlasBatch _chipBatch = new TileAtlasBatch(AtlasNames.Chip);
        private TileAtlasBatch _portraitBatch = new TileAtlasBatch(ContentAtlasNames.Portrait);

        [Child] private UiText TopicImpress = new UiTextTopic();
        [Child] private UiText TopicAttract = new UiTextTopic();
        [Child] private UiText TextSpeakerName = new();
        [Child] private UiText TextImpression = new();
        [Child] private UiText TextImpression2 = new();
        [Child] private UiWrappedText TextBody = new(UiFonts.ListText);

        public override void Initialize(DialogArgs args)
        {
            Sounds.Play(Protos.Sound.Chat);

            _assetIeChat = Assets.Get(Protos.Asset.IeChat);
            _assetImpressionIcon = Assets.Get(Protos.Asset.ImpressionIcon);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(new(600, 380), out bounds);
        }

        public void SetDialogData(string text, List<DialogChoice> choices)
        {
            throw new NotImplementedException();
        }
    }
}
