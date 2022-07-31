using OpenNefia.Content.Audio;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;
using Love;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;

namespace OpenNefia.Content.Activity
{
    public abstract class BaseAutoTurnAnim : UiElement
    {
        [Dependency] protected readonly IAssetManager Assets = default!;
        [Dependency] protected readonly IAudioManager Audio = default!;

        public abstract SoundSpecifier Sound { get; set; }

        public virtual void Initialize() {}
        public virtual void OnFirstFrame() {}
        public abstract IGlobalDrawable MakeGlobalDrawable();
    }

    public class MiningAutoTurnAnim : BaseAutoTurnAnim
    {
        public override SoundSpecifier Sound { get; set; } = new SoundPathSpecifier(Protos.Sound.Dig1);

        private IAssetInstance _assetAutoTurnMining = default!;

        public override void Initialize()
        {
            _assetAutoTurnMining = Assets.GetAsset(Protos.Asset.AutoTurnMining);
        }

        public override void Draw()
        {
            _assetAutoTurnMining.DrawRegion(UIScale, "0", X, Y);
        }

        public class GlobalDrawable : BaseGlobalDrawable
        {
            private FrameCounter _counter = new(delaySecs: 40f * 0.001f, maxFrames: 10);
            private bool _playedSound = false;
            private IAssetInstance _assetAutoTurnMining;

            public GlobalDrawable(IAssetInstance assetAutoTurnMining)
            {
                _assetAutoTurnMining = assetAutoTurnMining;
            }

            public override void Draw()
            {
                var region = ((_counter.FrameInt / 2) % 5);
                _assetAutoTurnMining.DrawRegion(UIScale, region.ToString(), X, Y);
            }

            public override void Update(float dt)
            {
                _counter.Update(dt);

                if (!_playedSound && _counter.FrameInt >= 2)
                {
                    IoCManager.Resolve<IAudioManager>().Play(Protos.Sound.Dig1);
                    _playedSound = true;
                }
                
                if (_counter.IsFinished)
                    Finish();
            }
        }

        public override IGlobalDrawable MakeGlobalDrawable()
        {
            return new GlobalDrawable(_assetAutoTurnMining);
        }
    }

    public class FishingAutoTurnAnim : BaseAutoTurnAnim
    {
        public override SoundSpecifier Sound { get; set; } = new SoundPathSpecifier(Protos.Sound.Water);

        private IAssetInstance _assetAutoTurnFishing = default!;

        public override void Initialize()
        {
            _assetAutoTurnFishing = Assets.GetAsset(Protos.Asset.AutoTurnFishing);
        }

        public override void Draw()
        {
            _assetAutoTurnFishing.DrawRegion(UIScale, "0", X, Y);
        }

        public class GlobalDrawable : BaseGlobalDrawable
        {
            private FrameCounter _counter = new(delaySecs: 50f * 0.001f, maxFrames: 10);
            private IAssetInstance _assetAutoTurnFishing;

            public GlobalDrawable(IAssetInstance assetAutoTurnFishing)
            {
                _assetAutoTurnFishing = assetAutoTurnFishing;
            }

            public override void Draw()
            {
                var region = ((_counter.FrameInt / 3) % 3);
                _assetAutoTurnFishing.DrawRegion(UIScale, region.ToString(), X, Y);
            }

            public override void Update(float dt)
            {
                _counter.Update(dt);
                if (_counter.IsFinished)
                    Finish();
            }
        }

        public override IGlobalDrawable MakeGlobalDrawable()
        {
            return new GlobalDrawable(_assetAutoTurnFishing);
        }
    }

    public class HarvestingAutoTurnAnim : BaseAutoTurnAnim
    {
        public override SoundSpecifier Sound { get; set; } = new SoundPathSpecifier(Protos.Sound.Bush1);

        private IAssetInstance _assetAutoTurnHarvesting = default!;

        public override void Initialize()
        {
            _assetAutoTurnHarvesting = Assets.GetAsset(Protos.Asset.AutoTurnHarvesting);
        }

        public override void Draw()
        {
            _assetAutoTurnHarvesting.DrawRegion(UIScale, "0", X, Y);
        }

        public class GlobalDrawable : BaseGlobalDrawable
        {
            private FrameCounter _counter = new(delaySecs: 55f * 0.001f, maxFrames: 10);
            private bool _playedSound = false;
            private IAssetInstance _assetAutoTurnHarvesting;

            public GlobalDrawable(IAssetInstance assetAutoTurnHarvesting)
            {
                _assetAutoTurnHarvesting = assetAutoTurnHarvesting;
            }

            public override void Draw()
            {
                var region = ((_counter.FrameInt / 2) % 3);
                _assetAutoTurnHarvesting.DrawRegion(UIScale, region.ToString(), X, Y);
            }

            public override void Update(float dt)
            {
                _counter.Update(dt);

                if (!_playedSound && _counter.FrameInt >= 3)
                {
                    IoCManager.Resolve<IAudioManager>().Play(Protos.Sound.Bush1);
                    _playedSound = true;
                }

                if (_counter.IsFinished)
                    Finish();
            }
        }

        public override IGlobalDrawable MakeGlobalDrawable()
        {
            return new GlobalDrawable(_assetAutoTurnHarvesting);
        }
    }

    public class SearchingAutoTurnAnim : BaseAutoTurnAnim
    {
        public override SoundSpecifier Sound { get; set; } = new SoundPathSpecifier(Protos.Sound.Dig2);

        private IAssetInstance _assetAutoTurnSearching = default!;

        public override void Initialize()
        {
            _assetAutoTurnSearching = Assets.GetAsset(Protos.Asset.AutoTurnSearching);
        }

        public override void OnFirstFrame()
        {
        }

        public override void Draw()
        {
            _assetAutoTurnSearching.DrawRegion(UIScale, "0", X, Y);
        }

        public class GlobalDrawable : BaseGlobalDrawable
        {
            private FrameCounter _counter = new(delaySecs: 60f * 0.001f, maxFrames: 10);
            private bool _playedSound = false;
            private IAssetInstance _assetAutoTurnSearching;

            public GlobalDrawable(IAssetInstance assetAutoTurnSearching)
            {
                _assetAutoTurnSearching = assetAutoTurnSearching;
            }

            public override void Draw()
            {
                var region = ((_counter.FrameInt / 2) % 4);
                _assetAutoTurnSearching.DrawRegion(UIScale, region.ToString(), X, Y);
            }

            public override void Update(float dt)
            {
                _counter.Update(dt);

                if (!_playedSound && _counter.FrameInt >= 2)
                {
                    IoCManager.Resolve<IAudioManager>().Play(Protos.Sound.Dig2);
                    _playedSound = true;
                }

                if (_counter.IsFinished)
                    Finish();
            }
        }

        public override IGlobalDrawable MakeGlobalDrawable()
        {
            return new GlobalDrawable(_assetAutoTurnSearching);
        }
    }
}
