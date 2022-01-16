using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.CustomName;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.CharaSheet")]
    public class CharaMakeCharaSheetLayer : CharaMakeLayer
    {
        private CharaSheet Sheet;

        private EntityUid _entity;

        public CharaMakeCharaSheetLayer(EntityUid entity)
        {
            _entity = entity;

            Reroll(playSound: false);

            Sheet = new CharaSheet(entity);
        }

        private void Reroll(bool playSound)
        {
            if (playSound)
                Sounds.Play(Sound.Dice);

            var customName = EntityManager.EnsureComponent<CustomNameComponent>(_entity);
            customName.CustomName = "????";
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Protos.Sound.Chara);
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            Sheet.GetPreferredSize(out var size);
            UiUtils.GetCenteredParams(size.X, size.Y, out bounds, yOffset: -10);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Sheet.SetSize(Width, Height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Sheet.SetPosition(X, Y);
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
