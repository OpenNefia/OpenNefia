using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public class CharaMakeLayer : UiLayerWithResult<CharaMakeData, CharaMakeResult>, ICharaMakeLayer
    {
        protected IAssetInstance AssetBG;
        private IAssetInstance[] AssetWindows;

        protected CharaMakeData Data = default!;

        public CharaMakeLayer()
        {
            AssetBG = Assets.Get(AssetPrototypeOf.Void);

            AssetWindows = new[]
            {
                Assets.Get(AssetPrototypeOf.G1),
                Assets.Get(AssetPrototypeOf.G2),
                Assets.Get(AssetPrototypeOf.G3),
                Assets.Get(AssetPrototypeOf.G4)
            };
        }

        public override void Initialize(CharaMakeData args)
        {
            Data = args;
        }

        protected void Center(UiWindow window)
        {
            window.SetPosition((Width - window.Width) / 2, (Height - window.Height) / 2);
        }

        public override void Draw()
        {
            AssetBG.Draw(X, Y, Width, Height);
        }

        //will be used to actually make the change to the character after creation
        public virtual void ApplyStep()
        {

        }
    }
}
