using OpenNefia.Content.UI.Element;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.CharSheet")]
    public class CharaMakeCharSheetLayer : CharaMakeLayer
    {
        private CharSheet Sheet;
        public CharaMakeCharSheetLayer(EntityUid entity)
        {
            Sheet = new CharSheet(entity);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Sheet.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Sheet, -10);
        }

        public override void Draw()
        {
            base.Draw();
            Sheet.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Sheet.Update(dt);
        }
    }
}
