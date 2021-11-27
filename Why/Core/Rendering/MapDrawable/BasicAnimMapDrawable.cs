using OpenNefia.Core.Data.Types;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Config;

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

            var animeWait = Config.AnimeWait;
            var maxFrames = this.BasicAnim.Asset.CountX;
            if (this.BasicAnim.FrameCount != null)
                maxFrames = this.BasicAnim.FrameCount.Value;

            this.Counter = new FrameCounter(animeWait + this.BasicAnim.FrameDelayMillis / 2, maxFrames);
            this.AssetDrawable = new AssetDrawable(this.BasicAnim.Asset);
        }

        public override void OnEnqueue()
        {
            if (this.BasicAnim.Sound != null)
            {
                // TODO positional audio
                Sounds.PlayOneShot(this.BasicAnim.Sound);
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
                this.X + GraphicsEx.Coords.TileWidth / 2, 
                this.Y + GraphicsEx.Coords.TileHeight / 6,
                centered: true,
                rotation: this.BasicAnim.Rotation * this.Counter.Frame);
        }
    }
}
