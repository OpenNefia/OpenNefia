using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.BaseAnim
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

            BasicAnim = _protos.Index(basicAnimId);
            var asset = _protos.Index(BasicAnim.Asset);

            var animeWait = _config.GetCVar(CCVars.AnimeAnimationWait);
            var maxFrames = asset.CountX;
            if (BasicAnim.FrameCount != null)
                maxFrames = BasicAnim.FrameCount.Value;

            Counter = new FrameCounter(animeWait / 4 + BasicAnim.FrameDelayMillis / 1000, maxFrames);
            AssetDrawable = _assets.GetAsset(BasicAnim.Asset);
        }

        public override void OnEnqueue()
        {
            if (BasicAnim.Sound != null)
            {
                _sounds.Play(BasicAnim.Sound.Value, ScreenLocalPos);
            }
        }

        public override void Update(float dt)
        {
            Counter.Update(dt);
            if (Counter.IsFinished)
            {
                Finish();
            }
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Love.Color.White);
            AssetDrawable.DrawRegion(_coords.TileScale, 
                Counter.FrameInt.ToString(),
                X + _coords.TileSize.X / 2,
                Y + _coords.TileSize.Y / 6,
                centered: true,
                rotationRads: BasicAnim.Rotation * Counter.Frame);
        }
    }
}
