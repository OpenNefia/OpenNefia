using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Content.CharaMake;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.IoC;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Content.PCCs;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.Charas
{
    [Localize("Elona.CharaMake.AppearanceSelect")]
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

        private CharaAppearanceWindow AppearanceWindow = new();

        public CharaAppearanceLayer()
        {
            AppearanceWindow.List_OnActivated += HandleWindowListOnActivated;
            AddChild(AppearanceWindow);
        }

        private void HandleWindowListOnActivated(object? sender, UiListEventArgs<CharaAppearanceUICellData> evt)
        {          
            // FIXME: #35
            if (evt.Handled || evt.SelectedCell is not CharaAppearanceUIListCell cell)
                return;

            if (cell.Data is CharaAppearanceUICellData.Done)
            {
                Finish(new());
            }
        }

        public override void Initialize(Args args)
        {
            _targetEntity = args.TargetEntity;

            CharaAppearanceData appearanceData = CharaAppearanceHelpers.MakeAppearanceDataFrom(args.TargetEntity, 
                _protos, _entityManager, _resourceCache, _pccs);
            AppearanceWindow.Initialize(appearanceData);
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Protos.Sound.Port);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            AppearanceWindow.GrabFocus();
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            AppearanceWindow.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size, out bounds, yOffset: -15);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            AppearanceWindow.SetSize(Width, Height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            AppearanceWindow.SetPosition(X, Y);
        }

        public override void Draw()
        {
            base.Draw();
            AppearanceWindow.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            AppearanceWindow.Update(dt);
        }

        public override void Dispose()
        {
            base.Dispose();
            AppearanceWindow.Dispose();
        }
    }
}
