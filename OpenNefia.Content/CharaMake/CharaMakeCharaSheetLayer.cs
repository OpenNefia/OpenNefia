using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.CharaSheet")]
    public class CharaMakeCharaSheetLayer : CharaMakeLayer
    {
        private CharaSheet Sheet;
        public CharaMakeCharaSheetLayer(EntityUid entity)
        {
            Sheet = new CharaSheet(entity);
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Protos.Sound.Chara);
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
