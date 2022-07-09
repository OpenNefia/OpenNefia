using OpenNefia.Content.Audio;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;
using Love;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.Activity
{
    public abstract class BaseAutoTurnAnim : UiElement
    {
        [Dependency] protected readonly IAssetManager Assets = default!;
        [Dependency] protected readonly IAudioManager Audio = default!;

        public abstract SoundSpecifier Sound { get; set; }

        public virtual void Initialize() {}
        public virtual void OnFirstFrame() { }
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
            _assetAutoTurnMining.DrawRegion(UIScale, "1", X, Y);
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
            _assetAutoTurnFishing.DrawRegion(UIScale, "1", X, Y);
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
            _assetAutoTurnHarvesting.DrawRegion(UIScale, "1", X, Y);
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
            Audio.Play(Protos.Sound.Water);
        }

        public override void Draw()
        {
            _assetAutoTurnSearching.DrawRegion(UIScale, "1", X, Y);
        }
    }
}
