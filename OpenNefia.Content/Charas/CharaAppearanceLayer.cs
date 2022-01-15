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

namespace OpenNefia.Content.Charas
{
    [Localize("Elona.CharaMake.AppearanceSelect")]
    public class CharaAppearanceLayer : UiLayerWithResult<UINone, UINone>
    {
        private CharaAppearanceWindow AppearanceWindow = new();

        public CharaAppearanceLayer()
        {
            AddChild(AppearanceWindow);
        }

        public override void Initialize(UINone args)
        {
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
