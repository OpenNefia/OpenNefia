using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    public class BasicAnimMapDrawable : BaseMapDrawable
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IAssetManager _assets = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IAudioManager _sounds = default!;

        public BasicAnimPrototype BasicAnim { get; }

        private FrameCounter Counter;
        private IAssetInstance AssetDrawable;

        public BasicAnimMapDrawable(PrototypeId<BasicAnimPrototype> basicAnimId)
        {
            IoCManager.InjectDependencies(this);

            this.BasicAnim = _protos.Index(basicAnimId);
            var asset = _protos.Index(BasicAnim.Asset);

            var animeWait = _config.GetCVar(CVars.AnimeAnimationWait);
            var maxFrames = asset.CountX;
            if (this.BasicAnim.FrameCount != null)
                maxFrames = this.BasicAnim.FrameCount.Value;

            this.Counter = new FrameCounter(animeWait + this.BasicAnim.FrameDelayMillis / 2, maxFrames);
            this.AssetDrawable = _assets.GetAsset(this.BasicAnim.Asset);
        }

        public override void OnEnqueue()
        {
            if (this.BasicAnim.Sound != null)
            {
                _sounds.Play(this.BasicAnim.Sound.Value, this.ScreenLocalPos);
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
            this.AssetDrawable.DrawRegionUnscaled(Counter.FrameInt.ToString(), 
                this.X + _coords.TileSize.X / 2, 
                this.Y + _coords.TileSize.Y / 6,
                centered: true,
                rotationRads: this.BasicAnim.Rotation * this.Counter.Frame);
        }
    }
}
