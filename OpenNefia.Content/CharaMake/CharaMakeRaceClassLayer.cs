using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    public class CharaMakeRaceClassLayer : CharaMakeLayer
    {
        public class RaceCell : UiListCell<RacePrototype>
        {
            public RaceCell(RacePrototype data) 
                : base(data, new UiText(UiFonts.ListTitleScreenText, data.ID ?? string.Empty))
            {
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
        private UiList<RacePrototype> RaceList;
        private RaceCell[] AllData;

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
            AllData = Array.Empty<RaceCell>();

            RaceTopic = new UiTextTopic();
            DetailTopic = new UiTextTopic();
            AttributeTopic = new UiTextTopic();
            SkillTopic = new UiTextTopic();

            DetailContainer = new UiVerticalContainer();

            DetailText = new UiWrapText(450, UiFonts.ListTitleScreenText, "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.");
            DetailText.Wrap();
            DetailContainer.AddElement(DetailText);
            DetailContainer.AddElement(LayoutType.YMin, 105);
            DetailContainer.AddElement(AttributeTopic, LayoutType.XOffset, -10);

            AttributeContainer = new UiGridContainer(GridType.Horizontal, 3, xCentered: false, xSpace: 10, ySpace: 1);
            DetailContainer.AddElement(AttributeContainer);
            DetailContainer.AddElement(SkillTopic, LayoutType.XOffset, -10);
            SkillContainer = new UiVerticalContainer();
            DetailContainer.AddElement(SkillContainer);

            RaceList = new UiList<RacePrototype>();
            RaceList.EventOnActivate += (_, args) =>
            {

            };
            RaceList.EventOnSelect += (_, args) =>
            {
                SetAttributes(args.SelectedCell.Data.BaseSkills);
            };
        }

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);
            AllData = _prototypeManager.EnumeratePrototypes<RacePrototype>().Select(x => new RaceCell(x)).ToArray();

            Window.OnPageChanged += Window_OnPageChanged;
            Window.Initialize(AllData);

            AddChild(Window);
            AddChild(RaceList);
            //SetAttributes(AllData.First().Data.BaseSkills);
        }

        private void SetAttributes(Dictionary<PrototypeId<SkillPrototype>, int> skills)
        {
            AttributeContainer.Clear();
            foreach(var attr in MakeDetailAttribute(skills))
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
                var protoId = new PrototypeId<SkillPrototype>(attrId);
                //TODO add image
                var text = new UiText();
                cont.AddElement(text);
                skills.TryGetValue(protoId, out var amt);
                GetAttributeAmountDesc(amt, out var desc, out var clr);
                text.Text = $"{Loc.GetString($"{attrId}.Short")}: {desc}";
                text.Color = clr;
                yield return cont;
            }
        }


        private void GetAttributeAmountDesc(int amount, out string text, out Color color)
        {
            switch(amount)
            {
                default:
                case <= 0:
                    text = Loc.GetString("Skill.Amt.None");
                    color = UiColors.MesGray;
                    break;
                case <= 2:
                    text = Loc.GetString("Skill.Amt.Slight");
                    color = UiColors.MesLightRed;
                    break;
                case <= 3:
                    text = Loc.GetString("Skill.Amt.Little");
                    color = UiColors.MesRed;
                    break;
                case <= 6:
                    text = Loc.GetString("Skill.Amt.Normal");
                    color = UiColors.MesBlack;
                    break;
                case <= 8:
                    text = Loc.GetString("Skill.Amt.NotBad");
                    color = UiColors.MesSkyBlue;
                    break;
                case <= 10:
                    text = Loc.GetString("Skill.Amt.Good");
                    color = UiColors.MesSkyBlue;
                    break;
                case <= 12:
                    text = Loc.GetString("Skill.Amt.NotBad");
                    color = UiColors.MesBlue;
                    break;
            }
        }

        private void Window_OnPageChanged()
        {
            RaceList.Clear();
            RaceList.AddRange(Window.GetCurrentElements().Cast<RaceCell>() ?? Enumerable.Empty<RaceCell>());
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(700, 500);
            RaceList.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window);
            RaceList.SetPosition(Window.X + 35, Window.Y + 60);
            RaceTopic.SetPosition(Window.X + 30, Window.Y + 30);
            DetailTopic.SetPosition(Window.X + 190, RaceTopic.Y);
            DetailContainer.SetPosition(Window.X + 210, Window.Y + 60);
            DetailContainer.Resolve();
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            RaceList.Draw();

            RaceTopic.Draw();
            DetailTopic.Draw();
            DetailContainer.Draw();

        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            RaceList.Update(dt);

            RaceTopic.Update(dt);
            DetailTopic.Update(dt);
            DetailText.Update(dt);
            DetailContainer.Update(dt);
        }
    }
}
