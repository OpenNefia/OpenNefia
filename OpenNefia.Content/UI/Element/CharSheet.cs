using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Skills;
using OpenNefia.Content.World;
using OpenNefia.Content.Levels;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.UI.Element
{
    public class CharSheet : UiElement
    {
        public static class CharSheetHelpers
        {
            public static string GetPotentialDescription(int pot)
            {
                if (pot >= 200)
                    return Loc.GetString("Elona.CharSheet.Potential.Superb");
                else if (pot >= 150)
                    return Loc.GetString("Elona.CharSheet.Potential.Great");
                else if (pot >= 100)
                    return Loc.GetString("Elona.CharSheet.Potential.Good");
                else if (pot >= 50)
                    return Loc.GetString("Elona.CharSheet.Potential.Bad");
                else
                    return Loc.GetString("Elona.CharSheet.Potential.Hopeless");
            }
        }

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] protected readonly ISkillsSystem _skillsSys = default!;

        public const int SheetWidth = 700;
        public const int SheetHeight = 400;
        private const int ContainerSpacing = 4;
        private const int AttributeContainerSpacing = 6;
        private const int AttributeSpacing = 5;
        private const int AttributePotentialSpacing = 8;

        private IAssetInstance IeSheet;
        private EntityUid CharaEntity;
        private UiContainer NameContainer;
        private UiContainer ClassContainer;
        private UiContainer ExpContainer;
        private UiContainer AttributeContainer;
        private UiContainer BlessingContainer;
        private UiContainer TraceContainer;
        private UiContainer ExtraContainer;

        // Temporary variables that need to be replaced as soon as the containing components exist
        private string TempName = "????";
        private string TempAge = "??";
        private string TempHeight = "???";
        private string TempWeight = "??";
        private string TempGod = "Eyth of Infidel";
        private string TempGuild = "None";

        public CharSheet(EntityUid charaEntity)
        {
            EntitySystem.InjectDependencies(this);
            CharaEntity = charaEntity;
            IeSheet = Assets.Get(Protos.Asset.IeSheet);

            NameContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            ClassContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            ExpContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            AttributeContainer = new UiVerticalContainer { YSpace = AttributeContainerSpacing };
            BlessingContainer = new UiVerticalContainer();
            TraceContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            ExtraContainer = new UiVerticalContainer { YSpace = ContainerSpacing };
            Init();
        }

        private void Init()
        {
            if (!_entityManager.TryGetComponent<LevelComponent>(CharaEntity, out var level))
            {
                Logger.WarningS("charsheet", $"entity {CharaEntity} does not posess a LevelComponent");
            }
            if (!_entityManager.TryGetComponent<CharaComponent>(CharaEntity, out var chara))
            {
                Logger.WarningS("charsheet", $"entity {CharaEntity} does not posess a CharaComponent");
            }
            if (!_entityManager.TryGetComponent<SkillsComponent>(CharaEntity, out var skills))
            {
                Logger.WarningS("charsheet", $"entity {CharaEntity} does not posess a SkillsComponent");
            }
            var dict = new Dictionary<string, string>();
            if (chara != null)
            {
                dict[string.Empty] = string.Empty;
                dict[Loc.GetString("Elona.CharSheet.Name")] = TempName;
                if (!string.IsNullOrEmpty(chara.Title))
                    dict[Loc.GetString("Elona.CharSheet.Aka")] = chara.Title;
                dict[Loc.GetString("Elona.CharSheet.Race")] = Loc.GetPrototypeString(chara.Race, "Name");
                dict[Loc.GetString("Elona.CharSheet.Sex")] = Loc.GetString($"Elona.Gender.Names.{chara.Gender}.Normal").FirstCharToUpper();
                SetupContainer(NameContainer, 2, dict);
                dict.Clear();

                dict[string.Empty] = string.Empty;
                dict[Loc.GetString("Elona.CharSheet.Class")] = Loc.GetPrototypeString(chara.Class, "Name");
                dict[Loc.GetString("Elona.CharSheet.Age")] = TempAge;
                dict[Loc.GetString("Elona.CharSheet.Height")] = TempHeight;
                dict[Loc.GetString("Elona.CharSheet.Weight")] = TempWeight;
                SetupContainer(ClassContainer, 2, dict);
                dict.Clear();
            }
            if (level != null)
            {
                dict[Loc.GetString("Elona.CharSheet.Level")] = level.Level.ToString();
                dict[Loc.GetString("Elona.CharSheet.Exp")] = level.Experience.ToString();
                dict[Loc.GetString("Elona.CharSheet.NextLv")] = level.ExperienceToNext.ToString();
            }
            dict[Loc.GetString("Elona.CharSheet.God")] = TempGod;
            dict[Loc.GetString("Elona.CharSheet.Guild")] = TempGuild;
            SetupContainer(ExpContainer, 2, dict);
            dict.Clear();


            AttributeContainer.AddElement(new UiTextTopic(Loc.GetString("Elona.CharSheet.Topic.Attribute")));
            AttributeContainer.AddLayout(LayoutType.Spacer, 12);
            AttributeContainer.AddLayout(LayoutType.XOffset, 10);
            foreach (var attrProto in _skillsSys.EnumerateBaseAttributes())
            {
                if (skills == null)
                    break;
                var cont = new UiHorizontalContainer();
                var attrId = attrProto.GetStrongID();
                if (!skills.Skills.TryGetValue(attrId, out var attrLvl))
                    continue;
                string currentAmt = attrLvl.Level.Buffed.ToString();
                string orgAmt = $"({attrLvl.Level.Base})";
                var content = currentAmt
                    + new string(' ', Math.Max(1, AttributeSpacing - currentAmt.Length)) 
                    + orgAmt
                    + new string(' ', Math.Max(1, AttributePotentialSpacing - orgAmt.Length)) 
                    + CharSheetHelpers.GetPotentialDescription(attrLvl.Potential);
                cont.AddElement(new AttributeIcon(attrId));
                cont.AddLayout(LayoutType.Spacer, 14);
                cont.AddLayout(LayoutType.YOffset, -5);
                cont.AddElement(MakeInfoContainer(Loc.GetPrototypeString(attrId, "ShortName"), 1, content));
                AttributeContainer.AddElement(cont);
            }

        }

        private void SetupContainer(UiContainer cont, int xOffset, Dictionary<string, string> content)
        {
            var maxLen = content.Select(x => x.Key).Max(x => x.Length);
            foreach(var item in content)
            {
                cont.AddElement(MakeInfoContainer(item.Key, maxLen + xOffset, item.Value));
            }
        }

        private UiContainer MakeInfoContainer(string name, int xOffset, string content)
        {
            var cont = new UiHorizontalContainer();
            cont.AddElement(new UiText(UiFonts.CharSheetInfo, name 
                + (xOffset > 0 ? new string(' ', Math.Max(1, xOffset - name.Length)) : string.Empty)));
            cont.AddLayout(LayoutType.YOffset, -1);
            cont.AddElement(new UiText(UiFonts.CharSheetInfoContent, content));
            return cont;
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size.X = SheetWidth;
            size.Y = SheetHeight;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            NameContainer.SetPosition(x + 30, y + 47);
            NameContainer.Resolve();
            ClassContainer.SetPosition(x + 215, NameContainer.Y);
            ClassContainer.Resolve();
            ExpContainer.SetPosition(x + 355, NameContainer.Y);
            ExpContainer.Resolve();
            AttributeContainer.SetPosition(x + 27, y + 125);
            AttributeContainer.Resolve();
        }

        public override void Draw()
        {
            base.Draw();
            GraphicsEx.SetColor(0, 0, 0, 75);
            IeSheet.Draw(X + 4, Y + 4, SheetWidth, SheetHeight);
            GraphicsEx.SetColor(Color.White);
            IeSheet.Draw(X, Y, SheetWidth, SheetHeight);

            NameContainer.Draw();
            ClassContainer.Draw();
            ExpContainer.Draw();
            AttributeContainer.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            NameContainer.Update(dt);
            ClassContainer.Update(dt);
            ExpContainer.Update(dt);
            AttributeContainer.Update(dt);
        }
    }
}
