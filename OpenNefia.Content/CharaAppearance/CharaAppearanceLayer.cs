using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.IoC;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Content.PCCs;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;

namespace OpenNefia.Content.CharaAppearance
{
    public class CharaAppearanceLayer : UiLayerWithResult<CharaAppearanceLayer.Args, UINone>
    {
        public class Args
        {
            public EntityUid TargetEntity { get; }

            public Args(EntityUid targetEntity)
            {
                TargetEntity = targetEntity;
            }
        }

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IPCCSystem _pccs = default!;

        private EntityUid _targetEntity;

        [Child] private CharaAppearanceControl AppearanceControl = new();

        public CharaAppearanceLayer()
        {
            AppearanceControl.List_OnActivated += HandleWindowListOnActivated;

            OnKeyBindDown += HandleKeyBindDown;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
        }

        private void HandleWindowListOnActivated(object? sender, UiListEventArgs<CharaAppearanceUICellData> evt)
        {
            // FIXME: #35
            if (evt.Handled || evt.SelectedCell is not CharaAppearanceUIListCell cell)
                return;

            if (cell.Data is CharaAppearanceUICellData.Done)
            {
                CharaAppearanceHelpers.ApplyAppearanceDataTo(_targetEntity, AppearanceControl.AppearanceData, _entityManager, _pccs);
                Finish(new());
            }
        }

        public override void Initialize(Args args)
        {
            _targetEntity = args.TargetEntity;

            CharaAppearanceData appearanceData = CharaAppearanceHelpers.MakeAppearanceDataFrom(_targetEntity,
                _protos, _entityManager, _resourceCache, _pccs);
            AppearanceControl.Initialize(appearanceData);
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Protos.Sound.Port);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            AppearanceControl.GrabFocus();
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            AppearanceControl.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size, out bounds, yOffset: -15);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            AppearanceControl.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            AppearanceControl.SetPosition(X, Y);
        }

        public override void Draw()
        {
            base.Draw();
            AppearanceControl.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            AppearanceControl.Update(dt);
        }

        public override void Dispose()
        {
            base.Dispose();
            AppearanceControl.Dispose();
        }
    }
}
