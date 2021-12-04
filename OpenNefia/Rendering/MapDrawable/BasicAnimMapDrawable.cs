using OpenNefia.Core.Audio;
using OpenNefia.Core.Config;
using OpenNefia.Core.Game;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    public class BasicAnimMapDrawable : BaseMapDrawable
    {
        public BasicAnimPrototype BasicAnim { get; }

        private FrameCounter Counter;
        private IAssetDrawable AssetDrawable;

        public BasicAnimMapDrawable(PrototypeId<BasicAnimPrototype> basicAnimId)
        {
            this.BasicAnim = basicAnimId.ResolvePrototype();

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
                this.X + GameSession.Coords.TileSize.X / 2, 
                this.Y + GameSession.Coords.TileSize.Y / 6,
                centered: true,
                rotation: this.BasicAnim.Rotation * this.Counter.Frame);
        }
    }
}
