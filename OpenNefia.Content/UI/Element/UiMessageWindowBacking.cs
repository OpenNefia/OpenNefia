using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element
{
    public class UiMessageWindowBacking : BaseDrawable
    {
        private IAssetInstance MessageWindowAsset;
        public UiMessageWindowBacking()
        {
            MessageWindowAsset = Assets.Get(Protos.Asset.MessageWindow);
        }

        public override void Draw()
        {
            throw new NotImplementedException();
        }

        public override void Update(float dt)
        {
            throw new NotImplementedException();
        }
    }
}
