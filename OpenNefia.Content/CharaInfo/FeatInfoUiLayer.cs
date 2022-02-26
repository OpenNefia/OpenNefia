using OpenNefia.Content.Feats;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Input;
using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaInfo
{
    public sealed class FeatInfoUiLayer : CharaGroupUiLayer
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;

        [Child] private FeatWindow FeatWindow;

        private EntityUid _charaEntity;

        public FeatInfoUiLayer()
        {
            FeatWindow = new FeatWindow(new FeatInfoFeatWindowBehavior(this));

            OnKeyBindDown += HandleKeyBindDown;
        }

        public override void Initialize(CharaGroupSublayerArgs args)
        {
            _charaEntity = args.CharaEntity;

            FeatWindow.RefreshData();
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            FeatWindow.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs evt)
        {
            if (evt.Function == EngineKeyFunctions.UICancel)
            {
                Finish(SharedSublayerResult);
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            return base.MakeKeyHints();
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Sound.Chara);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            FeatWindow.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size.X, size.Y, out bounds, yOffset: 10);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            FeatWindow.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            FeatWindow.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            FeatWindow.Update(dt);
        }

        public override void Draw()
        {
            FeatWindow.Draw();
        }

        private class FeatInfoFeatWindowBehavior : IFeatWindowBehavior
        {
            private FeatInfoUiLayer Layer;

            public FeatInfoFeatWindowBehavior(FeatInfoUiLayer layer)
            {
                Layer = layer;
            }

            public int GetNumberOfFeatsAcquirable()
            {
                if (!Layer._entityManager.TryGetComponent(Layer._charaEntity, out FeatsComponent feats))
                    return 0;

                return feats.NumberOfFeatsAcquirable;
            }

            public IReadOnlyDictionary<PrototypeId<FeatPrototype>, FeatLevel> GetGainedFeats()
            {
                if (!Layer._entityManager.TryGetComponent(Layer._charaEntity, out FeatsComponent feats))
                    return new Dictionary<PrototypeId<FeatPrototype>, FeatLevel>();

                return feats.Feats;
            }

            public void OnFeatSelected(FeatWindow.FeatNameAndDesc.Feat feat)
            {
                if (!Layer._entityManager.TryGetComponent(Layer._charaEntity, out FeatsComponent feats))
                    return;

                feats.NumberOfFeatsAcquirable--;
                Layer._feats.AddLevel(Layer._charaEntity, feat.Prototype.GetStrongID(), 1);
                Layer._refresh.Refresh(Layer._charaEntity);
            }
        }
    }
}