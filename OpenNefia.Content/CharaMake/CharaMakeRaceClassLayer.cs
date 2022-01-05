using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.RaceSelect")]
    public class CharaMakeRaceClassLayer : CharaMakeLayer
    {
        public class RaceClass
        {
            private IPrototype Data;

            public RaceClass(IPrototype data)
            {
                Data = data;
            }
            public string GetString(string suffix = "")
            {
                switch (Data)
                {
                    case RacePrototype race:
                        return Loc.GetPrototypeString<RacePrototype>(new(race.ID), suffix)!;
                    case ClassPrototype classP:
                        return Loc.GetPrototypeString<ClassPrototype>(new(classP.ID), suffix)!;
                    default:
                        return string.Empty;
                }
            }

            public Dictionary<PrototypeId<SkillPrototype>, int> GetSkills()
            {
                switch (Data)
                {
                    case RacePrototype race:
                        return race.BaseSkills;
                    case ClassPrototype classP:
                        return classP.BaseSkills;
                    default:
                        return new()!;
                }
            }
        }
        public class RaceClassCell : UiListCell<RaceClass>
        {
            
            public RaceClassCell(RaceClass wrapper) 
                : base(wrapper, new UiText(UiFonts.ListTitleScreenText))
            {
                Text = Data.GetString("Name");
            }
        }



        public class AttributeIcon : UiElement
        {
            private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>
            {
                { "Elona.StatStrength", "0" },
                { "Elona.StatConstitution", "1" },
                { "Elona.StatDexterity", "2" },
                { "Elona.StatPerception", "3" },
                { "Elona.StatLearning", "4" },
                { "Elona.StatWill", "5" },
                { "Elona.StatMagic", "6" },
                { "Elona.StatCharisma", "7" }

            };
            private IAssetInstance AssetAttributeIcons;
            private string Type;

            public AttributeIcon(string type)
            {
                AssetAttributeIcons = Assets.Get(AssetPrototypeOf.AttributeIcons);
                Type = type;
            }

            public override void Draw()
            {
                base.Draw();
                GraphicsEx.SetColor(Color.White);
                _attributes.TryGetValue(Type, out var iconId);
                AssetAttributeIcons.DrawRegion($"{iconId ?? "2"}", X, Y, centered: true);
            }

            public override void GetPreferredSize(out Vector2i size)
            {
                size = new Vector2i(10, 10);
            }
        }

        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        [Localize] private PagedUiWindow Window;
        [Localize] private UiTextTopic RaceTopic;
        [Localize] private UiTextTopic DetailTopic;
        [Localize] private UiTextTopic AttributeTopic;
        [Localize] private UiTextTopic SkillTopic;
        private UiVerticalContainer DetailContainer;
        private UiVerticalContainer SkillContainer;
        private UiWrapText DetailText;

        private UiGridContainer AttributeContainer;
        private UiList<RaceClass> List;
        private RaceClassCell[] AllData;

        private readonly string[] AttributeIds = new[]
        {
            "Elona.StatStrength",
            "Elona.StatConstitution",
            "Elona.StatDexterity",
            "Elona.StatPerception",
            "Elona.StatLearning",
            "Elona.StatWill",
            "Elona.StatMagic",
            "Elona.StatCharisma"
        };
        

        public CharaMakeRaceClassLayer()
        {
            Window = new PagedUiWindow();
            AllData = Array.Empty<RaceClassCell>();

            RaceTopic = new UiTextTopic();
            DetailTopic = new UiTextTopic();
            AttributeTopic = new UiTextTopic();
            SkillTopic = new UiTextTopic();

            DetailContainer = new UiVerticalContainer();

            DetailText = new UiWrapText(450, UiFonts.ListTitleScreenText);
            DetailContainer.AddElement(DetailText);
            DetailContainer.AddElement(LayoutType.YMin, 110);
            DetailContainer.AddElement(AttributeTopic, LayoutType.XOffset, -10);

            DetailContainer.AddElement(LayoutType.Spacer, 18);

            AttributeContainer = new UiGridContainer(GridType.Horizontal, 3, xCentered: false, xSpace: 50, ySpace: 3);
            DetailContainer.AddElement(AttributeContainer);

            DetailContainer.AddElement(LayoutType.Spacer, 5);

            DetailContainer.AddElement(SkillTopic, LayoutType.XOffset, -10);
            SkillContainer = new UiVerticalContainer();
            DetailContainer.AddElement(SkillContainer);

            OnKeyBindDown += CharaMakeRaceClassLayer_OnKeyBindDown;

            List = new UiList<RaceClass>();
            List.EventOnActivate += (_, args) =>
            {
                
            };
            List.EventOnSelect += (_, args) =>
            {
                SelectData(args.SelectedCell.Data);
            };

            AddChild(List);
            AddChild(Window);
        }

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);
            AllData = _prototypeManager.EnumeratePrototypes<RacePrototype>().Select(x => new RaceClassCell(new RaceClass(x))).ToArray();

            Window.OnPageChanged += Window_OnPageChanged;
            Window.Initialize(AllData);

            SelectData(AllData.First().Data);
        }

        private void Window_OnPageChanged()
        {
            List.Clear();
            List.AddRange(Window.GetCurrentElements().Cast<RaceClassCell>() ?? Enumerable.Empty<RaceClassCell>());
            List.Select(List.SelectedIndex);
        }

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        private void CharaMakeRaceClassLayer_OnKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UINextPage)
            {
                Window.PageForward();
            }
            if (args.Function == EngineKeyFunctions.UIPreviousPage)
            {
                Window.PageBackward();
            }
        }

        protected virtual void Select()
        {

        }

        private void SelectData(RaceClass data)
        {
            SetAttributes(data.GetSkills());
            DetailText.Text = data.GetString("Description");
        }

        private void SetAttributes(Dictionary<PrototypeId<SkillPrototype>, int> skills)
        {
            AttributeContainer.Clear();
            AttributeContainer.AddElement(LayoutType.XMin, 100);
            
            foreach (var attr in MakeDetailAttribute(skills))
            {
                AttributeContainer.AddElement(attr);
            }
            DetailContainer.Resolve();
        }

        private IEnumerable<UiElement> MakeDetailAttribute(Dictionary<PrototypeId<SkillPrototype>, int> skills)
        {
            foreach(var attrId in AttributeIds)
            {
                var cont = new UiHorizontalContainer();
                cont.AddElement(LayoutType.XOffset, 7);
                cont.AddElement(new AttributeIcon(attrId), LayoutType.YOffset, 3);
                cont.AddElement(LayoutType.XOffset, -7);


                skills.TryGetValue(new PrototypeId<SkillPrototype>(attrId), out var amt);
                GetAttributeAmountDesc(amt, out var desc, out var clr);
                var text = new UiText
                {
                    Text = $"{Loc.GetString($"SkillShort.{attrId}")}: {desc}",
                    Color = clr
                };
                cont.AddElement(text);
                yield return cont;
            }
        }


        private void GetAttributeAmountDesc(int amount, out string text, out Color color)
        {
            switch (amount)
            {
                case <= 0:
                    text = Loc.GetString("Skill.Amt.None");
                    color = UiColors.CharaMakeStatLevelNone;
                    break;
                case <= 2:
                    text = Loc.GetString("Skill.Amt.Slight");
                    color = UiColors.CharaMakeStatLevelSlight;
                    break;
                case <= 4:
                    text = Loc.GetString("Skill.Amt.Little");
                    color = UiColors.CharaMakeStatLevelLittle;
                    break;
                case <= 6:
                    text = Loc.GetString("Skill.Amt.Normal");
                    color = UiColors.CharaMakeStatLevelNormal;
                    break;
                case <= 8:
                    text = Loc.GetString("Skill.Amt.NotBad");
                    color = UiColors.CharaMakeStatLevelNotBad;
                    break;
                case <= 10:
                    text = Loc.GetString("Skill.Amt.Good");
                    color = UiColors.CharaMakeStatLevelGood;
                    break;
                case <= 12:
                    text = Loc.GetString("Skill.Amt.Great");
                    color = UiColors.CharaMakeStatLevelGreat;
                    break;
                default:
                    text = Loc.GetString("Skill.Amt.Best");
                    color = UiColors.CharaMakeStatLevelBest;
                    break;
            }
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(700, 500);
            List.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window);
            List.SetPosition(Window.X + 35, Window.Y + 60);
            RaceTopic.SetPosition(Window.X + 30, Window.Y + 30);
            DetailTopic.SetPosition(Window.X + 190, RaceTopic.Y);
            DetailContainer.SetPosition(Window.X + 210, Window.Y + 60);
            DetailContainer.Resolve();
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            List.Draw();

            RaceTopic.Draw();
            DetailTopic.Draw();
            DetailContainer.Draw();

        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            List.Update(dt);

            RaceTopic.Update(dt);
            DetailTopic.Update(dt);
            DetailText.Update(dt);
            DetailContainer.Update(dt);
        }
    }
}
