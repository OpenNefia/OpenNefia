using OpenNefia.Content.Charas;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Input;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Input;
using OpenNefia.Core;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Log;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.GenderSelect")]
    public class CharaMakeGenderSelectLayer : CharaMakeLayer
    {
        public class GenderCell : UiListCell<Gender>
        {
            public override string? LocalizeKey => Data switch
            {
                Gender.Male => "Elona.Gender.Names.Male.Polite",
                _ => "Elona.Gender.Names.Female.Polite",
            };
            public GenderCell(Gender data) 
                : base(data, new UiText(UiFonts.ListText))
            {
                Text = Loc.GetString(LocalizeKey!).FirstCharToUpper();
            }
        }

        [Localize] private UiWindow Window = new();
        [Localize] private UiTextTopic GenderTopic = new();

        private const string ResultName = "gender";

        private UiList<Gender> List = new();

        public CharaMakeGenderSelectLayer()
        {
            List.AddRange(new[] 
            {
                new GenderCell(Gender.Male),
                new GenderCell(Gender.Female),
            });
            List.OnActivated += (_, args) =>
            {
                Sounds.Play(Protos.Sound.Ok1);
                Finish(new CharaMakeResult(new Dictionary<string, object>
                {
                    { ResultName, args.SelectedCell.Data }
                }));
            };
            
            AddChild(List);

            Window.KeyHints = MakeKeyHints();
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Protos.Sound.Spell);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            UiUtils.GetCenteredParams(380, 180, out bounds, yOffset: -20);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            GenderTopic.SetPosition(Window.X + 30, Window.Y + 30);
            List.SetPosition(Window.X + 35, Window.Y + 60);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            GraphicsEx.SetColor(255, 255, 255, 40);
            AssetWindows[0].Draw(Window.X + 100, Window.Y + 30, 180, 110);
            GenderTopic.Draw();
            List.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            GenderTopic.Update(dt);
            List.Update(dt);
        }

        public override void ApplyStep(EntityUid entity)
        {
            base.ApplyStep(entity);
            if (!Data.TryGetValue<Gender>(ResultName, out var gender))
            {
                Logger.WarningS("charamake", "No gender in CharaMakeData");
                return;
            }

            if (!_entityManager.TryGetComponent<CharaComponent>(entity, out var chara))
            {
                Logger.WarningS("charamake", "No CharaComponent present on entity");
                return;
            }

            chara.Gender = gender;
        }
    }
}
