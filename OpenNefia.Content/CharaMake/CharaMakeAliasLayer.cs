using OpenNefia.Content.Input;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.CharaMake.CharaMakeAttributeRerollLayer;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.AliasSelect")]
    public class CharaMakeAliasLayer : CharaMakeLayer
    {
        public class CreateCharaAliasData
        {
            public CreateCharaAliasData(string alias, bool isReroll = false, bool isLocked = false)
            {
                Alias = alias;
                IsReroll = isReroll;
                IsLocked = isLocked;
            }

            public string Alias { get; set; }
            public bool IsReroll { get; set; }
            public bool IsLocked { get; set; }
        }
        public class CreateCharaAliasCell : UiListCell<CreateCharaAliasData>
        {
            private UiText LockedText;

            public CreateCharaAliasCell(CreateCharaAliasData data, string text)
                : base(data, new UiText(UiFonts.ListTitleScreenText, text))
            {
                LockedText = new UiText(UiFonts.CharaMakeRerollLocked, Loc.GetString("Elona.CharaMake.AttributeReroll.Locked"));
            }

            public override void SetPosition(int x, int y)
            {
                base.SetPosition(x, y);
                LockedText.SetPosition(x + 216, y + 2);
            }

            public override void Draw()
            {
                base.Draw();
                if (Data.IsLocked)
                    LockedText.Draw();
            }
        }

        [Dependency] private readonly IRandomAliasGenerator _aliasGenerator = default!;

        public const string ResultName = "alias";

        [Localize]
        private UiWindow Window;
        [Localize]
        private IUiText AliasTopic;

        private UiList<CreateCharaAliasData> List;

        public CharaMakeAliasLayer()
        {
            Window = new UiWindow();

            List = new UiList<CreateCharaAliasData>();
            List.EventOnActivate += HandleListOnActivate;
            AliasTopic = new UiTextTopic();

            OnKeyBindDown += HandleKeyBindDown;

            AddChild(Window);
            AddChild(List);
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs obj)
        {
            if (obj.Function == ContentKeyFunctions.UIMode2)
            {
                var data = List.SelectedCell?.Data;
                if (data != null && !data.IsReroll)
                    SetLock(data);
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.Back, EngineKeyFunctions.UICancel));
            keyHints.Add(new(new LocaleKey("Elona.CharaMake.AliasSelect.KeyHints.LockAlias"), ContentKeyFunctions.UIMode2));

            return keyHints;
        }

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);
            Window.KeyHints = MakeKeyHints();
            Reroll();
        }

        private void HandleListOnActivate(object? sender, UiListEventArgs<CreateCharaAliasData> args)
        {
            if (args.SelectedCell.Data.IsReroll)
            {
                Sounds.Play(Sound.Dice);
                Reroll();
            }
            else
            {
                Result = new CharaMakeResult(new Dictionary<string, object>
                {
                    { ResultName, args.SelectedCell.Data.Alias }
                });
                Finish(Result);
            }
        }

        private void Reroll()
        {
            var items = new CreateCharaAliasCell[17];
            items[0] = new CreateCharaAliasCell(new CreateCharaAliasData(string.Empty, true), Loc.GetString("Elona.CharaMake.AliasSelect.Reroll"));

            for (int i = 1; i < items.Length; i++)
            {
                var isLocked = false;
                if (i < List.DisplayedCells.Count)
                    isLocked = List.DisplayedCells[i].Data.IsLocked;

                string alias;
                if (isLocked)
                    alias = List.DisplayedCells[i].Data.Alias;
                else
                    alias = _aliasGenerator.GenerateRandomAlias(AliasType.Chara);
                
                items[i] = new CreateCharaAliasCell(new CreateCharaAliasData(alias, isReroll: false, isLocked: isLocked), alias);
            }

            List.Clear();
            List.AddRange(items);
        }

        private void SetLock(CreateCharaAliasData data)
        {
            Sounds.Play(Sound.Ok1);
            data.IsLocked = !data.IsLocked;
        }

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(400, 470);
            AliasTopic.SetPreferredSize();
            List.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window);
            AliasTopic.SetPosition(Window.X + 25, Window.Y + 30);
            List.SetPosition(Window.X + 40, Window.Y + 68);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            GraphicsEx.SetColor(255, 255, 255, 30);
            CurrentWindowBG.Draw(Window.X + 40, Window.Y + 30, 300, 405);
            List.Draw();
            AliasTopic.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            AliasTopic.Update(dt);
            List.Update(dt);
        }
    }
}
