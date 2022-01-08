﻿using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            List.EventOnActivate += (_, args) =>
            {
                Finish(new CharaMakeResult(new Dictionary<string, object>
                {
                    { ResultName, args.SelectedCell.Data }
                }));
            };
            
            AddChild(List);
        }

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(380, 180);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window, -20);
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
    }
}