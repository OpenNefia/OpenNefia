using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Input;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.CharaMake.CharaMakeAttributeRerollLayer;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.RandomText
{
    [Localize("Elona.RandomText.Alias.Layer")]
    public class PickRandomAliasPrompt : UiLayerWithResult<PickRandomAliasPrompt.Args, PickRandomAliasPrompt.Result>
    {
        public class Args
        {
            public AliasType AliasType { get; set; } = AliasType.Chara;
        }

        public class Result
        {
            public Result(string alias)
            {
                Alias = alias;
            }

            public string Alias { get; }
        }

        public class PickAliasData
        {
            public PickAliasData(string alias, bool isReroll = false, bool isLocked = false)
            {
                Alias = alias;
                IsReroll = isReroll;
                IsLocked = isLocked;
            }

            public string Alias { get; set; }
            public bool IsReroll { get; set; }
            public bool IsLocked { get; set; }
        }
        public class CreateCharaAliasCell : UiListCell<PickAliasData>
        {
            [Child] private UiText LockedText;

            public CreateCharaAliasCell(PickAliasData data, string text)
                : base(data, new UiText(text))
            {
                LockedText = new UiText(UiFonts.CharaMakeRerollLocked, Loc.GetString("Elona.CharaMake.Common.Locked"));
            }

            public override void SetPosition(float x, float y)
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

        [Child][Localize] private UiWindow Window;
        [Child][Localize] private UiText AliasTopic;
        [Child] private UiList<PickAliasData> List;
        private AliasType _aliasType;
        public IAssetInstance AssetBG { get; set; }

        public PickRandomAliasPrompt()
        {
            CanControlFocus = true;

            Window = new UiWindow();

            List = new UiList<PickAliasData>();
            List.OnActivated += HandleListOnActivate;
            AliasTopic = new UiTextTopic();

            OnKeyBindDown += HandleKeyBindDown;

            AssetBG = Assets.Get(Protos.Asset.G1);
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs obj)
        {
            if (obj.Function == ContentKeyFunctions.UIMode2)
            {
                var data = List.SelectedCell?.Data;
                if (data != null && !data.IsReroll)
                    SetLock(data);
            }
            else if (obj.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(new LocaleKey("Elona.RandomText.Alias.Layer.KeyHints.LockAlias"), ContentKeyFunctions.UIMode2));

            return keyHints;
        }

        public override void Initialize(Args args)
        {
            _aliasType = args.AliasType;

            Window.KeyHints = MakeKeyHints();
            Reroll();
        }

        private void HandleListOnActivate(object? sender, UiListEventArgs<PickAliasData> args)
        {
            if (args.SelectedCell.Data.IsReroll)
            {
                Sounds.Play(Sound.Dice);
                Reroll();
            }
            else
            {
                base.Result = new Result(args.SelectedCell.Data.Alias);
                base.Finish(base.Result);
            }
        }

        private void Reroll()
        {
            var items = new CreateCharaAliasCell[17];
            items[0] = new CreateCharaAliasCell(new PickAliasData(string.Empty, true), Loc.GetString("Elona.RandomText.Alias.Layer.Reroll"));

            for (int i = 1; i < items.Length; i++)
            {
                var isLocked = false;
                if (i < List.DisplayedCells.Count)
                    isLocked = List.DisplayedCells[i].Data.IsLocked;

                string alias;
                if (isLocked)
                    alias = List.DisplayedCells[i].Data.Alias;
                else
                    alias = _aliasGenerator.GenerateRandomAlias(_aliasType);

                items[i] = new CreateCharaAliasCell(new PickAliasData(alias, isReroll: false, isLocked: isLocked), alias);
            }

            List.SetCells(items);
        }

        private void SetLock(PickAliasData data)
        {
            Sounds.Play(Sound.Ok1);
            data.IsLocked = !data.IsLocked;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(400, 470, out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            AliasTopic.SetPreferredSize();
            List.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            AliasTopic.SetPosition(Window.X + 25, Window.Y + 30);
            List.SetPosition(Window.X + 40, Window.Y + 68);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            GraphicsEx.SetColor(255, 255, 255, 30);
            AssetBG.Draw(UIScale, Window.X + 40, Window.Y + 30, 300, 405);
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
