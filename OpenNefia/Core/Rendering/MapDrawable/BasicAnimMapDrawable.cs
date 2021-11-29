using OpenNefia.Core.Audio;
using OpenNefia.Core.Config;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Game;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class BasicAnimMapDrawable : BaseMapDrawable
    {
        public BasicAnimPrototype BasicAnim { get; }

        private FrameCounter Counter;
        private AssetDrawable AssetDrawable;

        public BasicAnimMapDrawable(BasicAnimPrototype basicAnim)
        {
            this.BasicAnim = basicAnim;

            var animeWait = ConfigVars.AnimeWait;
            var maxFrames = this.BasicAnim.Asset.ResolvePrototype().CountX;
            if (this.BasicAnim.FrameCount != null)
                maxFrames = this.BasicAnim.FrameCount.Value;

            this.Counter = new FrameCounter(animeWait + this.BasicAnim.FrameDelayMillis / 2, maxFrames);
            this.AssetDrawable = IoCManager.Resolve<IAssetManager>().GetAsset(this.BasicAnim.Asset);
        }

        public override void OnEnqueue()
        {
            if (this.BasicAnim.Sound != null)
            {
                // TODO positional audio
                Sounds.Play(this.BasicAnim.Sound.Value, this.ScreenLocalPos);
            }
        }

        public override void Update(float dt)
        {
            Counter.Update(dt);
            if (Counter.IsFinished)
            {
                this.Finish();
            }
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Love.Color.White);
            this.AssetDrawable.DrawRegion(Counter.FrameInt.ToString(), 
                this.Left + GameSession.Coords.TileWidth / 2, 
                this.Top + GameSession.Coords.TileHeight / 6,
                centered: true,
                rotation: this.BasicAnim.Rotation * this.Counter.Frame);
        }
    }
}
