using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.AttributeReroll")]
    public class CharaMakeAttributeRerollLayer : CharaMakeLayer
    {
        public enum AttributeRerollChoice
        {
            Reroll,
            Proceed
        }
        public abstract record AttributeRerollData
        {
            public abstract string Text { get; }

            public sealed record ListChoice(AttributeRerollChoice Choice) : AttributeRerollData
            {
                public override string Text => Loc.GetString($"Elona.CharaMake.AttributeReroll.{Choice}");
            }
            public sealed record Attribute(PrototypeId<SkillPrototype> Id) : AttributeRerollData
            {
                public override string Text => Loc.GetPrototypeString(Id, "Name")!;

                public IUiText AmountText = default!;
                public bool Locked;
                private int _amount;
                public int Amount
                {
                    get => _amount;
                    set
                    {
                        _amount = value;
                        AmountText.Text = $"{value}";
                    }
                }
            }
        }

        public class AttributeRerollCell : UiListCell<AttributeRerollData>
        {
            private UiText LockedText;
            private UiText AmountText;
            private AttributeIcon Icon;

            public AttributeRerollCell(AttributeRerollData data) 
                : base(data, new UiText(UiFonts.ListText))
            {
                Text = Data.Text;

                LockedText = new UiText(UiFonts.CharaMakeRerollLocked);
                AmountText = new UiText(UiFonts.CharaMakeRerollAttrAmount);
                LockedText.Text = Loc.GetString("Elona.CharaMake.AttributeReroll.Locked");

                switch (data)
                {
                    default:
                    case AttributeRerollData.ListChoice choice:
                        Icon = new AttributeIcon("");
                        break;
                    case AttributeRerollData.Attribute attr:
                        attr.AmountText = AmountText;
                        attr.Amount = new System.Random().Next(1, 14);
                        Icon = new AttributeIcon(attr.Id.ToString());
                        break;
                }
            }

            public override void SetSize(int width, int height)
            {
                height = 22;
                base.SetSize(width, height);
            }

            public override void SetPosition(int x, int y)
            {
                base.SetPosition(x, y);
                Icon.SetPosition(x + 150, y + 9);
                AmountText.SetPosition(x + 162, y);
                LockedText.SetPosition(x + 185, y + 3);
            }

            public override void Draw()
            {
                base.Draw();
                if (Data is AttributeRerollData.Attribute attr && attr.Locked)
                    LockedText.Draw();
                Icon.Draw();
                AmountText.Draw();
            }
        }

        [Localize] private UiWindow Window = new();
        [Localize] private UiTextTopic AttrTopic = new();
        [Localize] private UiWrapText AttrInfo;
        private UiText LockAmt = new();
        private UiList<AttributeRerollData> List;

        private int LockCount = 2;
        private bool IsInitialized;
        private const string ResultName = "attributes";

        public CharaMakeAttributeRerollLayer()
        {
            AttrInfo = new UiWrapText(115, UiFonts.CharaMakeLockInfo);
            List = new UiList<AttributeRerollData>();
            LockAmt = new UiText(UiFonts.CharaMakeLockInfo);
            AttrInfo.Text = Loc.GetString("Elona.CharaMake.AttributeReroll.AttrInfo");
            SetLockCountText();
            AddChild(List);
        }

        public override void Initialize(CharaMakeData args)
        {
            // gets called when the ui is loaded, so this could potentially run multiple times 
            //      if the user backtracks in creation
            if (IsInitialized) 
                return;
            base.Initialize(args);
            var data = AttributeIds.Select(x => new AttributeRerollCell(new AttributeRerollData.Attribute(new PrototypeId<SkillPrototype>(x)))).ToList();
            data.InsertRange(0, new[]
            {
                new AttributeRerollCell(new AttributeRerollData.ListChoice(AttributeRerollChoice.Reroll)),
                new AttributeRerollCell(new AttributeRerollData.ListChoice(AttributeRerollChoice.Proceed)),
            });
            List.AddRange(data);
            List.EventOnActivate += (_, args) =>
            {
                switch(args.SelectedCell.Data)
                {
                    case AttributeRerollData.ListChoice choice:
                        switch (choice.Choice)
                        {
                            case AttributeRerollChoice.Proceed:
                                Finish(new CharaMakeResult(new Dictionary<string, object>
                                {
                                    { ResultName, List.Select(x => x.Data)
                                        .Where(x => x is AttributeRerollData.Attribute)
                                        .Cast<AttributeRerollData.Attribute>()
                                        .ToDictionary(x => x.Id, y => y.Amount) }
                                }));
                                break;
                            default:
                                Reroll();
                                break;
                        }
                        break;
                    case AttributeRerollData.Attribute attr:
                        SetLock(attr);
                        break;
                }
            };
            IsInitialized = true;
        }

        private void Reroll()
        {
            Sounds.Play(Protos.Sound.Dice);
            //TODO add rerolling
        }

        private void SetLock(AttributeRerollData.Attribute data)
        {
            Sounds.Play(Protos.Sound.Ok1);
            if (data.Locked)
            {
                data.Locked = false;
                LockCount++;
            }
            else if (LockCount > 0)
            {
                data.Locked = true;
                LockCount--;
            }
            SetLockCountText();
        }
        private void SetLockCountText()
        {
            LockAmt.Text = $"{Loc.GetString("Elona.CharaMake.AttributeReroll.LockAmt")}: {LockCount}";
        }

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(360, 355);
            List.SetPreferredSize();
            LockAmt.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window, -10);
            AttrInfo.SetPosition(Window.X + 165, Window.Y + 50);
            LockAmt.SetPosition(Window.X + 172, Window.Y + 80);
            AttrTopic.SetPosition(Window.X + 30, Window.Y + 30);
            List.SetPosition(Window.X + 40, Window.Y + 65);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            GraphicsEx.SetColor(255, 255, 255, 30);
            AssetWindows[0].Draw(Window.X + 15, Window.Y + 50, 150, 265);
            AttrTopic.Draw();
            List.Draw();
            LockAmt.Draw();
            AttrInfo.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            AttrTopic.Update(dt);
            List.Update(dt);
            LockAmt.Update(dt);
            AttrInfo.Update(dt);
        }
    }
}
