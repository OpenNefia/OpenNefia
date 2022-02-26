using OpenNefia.Content.Resists;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.UI;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.CharaInfo
{
    [Localize("Elona.CharaInfo.SkillsList")]
    public class SkillsListControl : UiElement, IUiPaged
    {
        public abstract class SkillsListEntry
        {
            public string Text { get; }
            public string Description { get; }

            public virtual int OffsetX => 12;
            public virtual int OffsetDX => 0;

            protected SkillsListEntry(string text, string description = "")
            {
                Text = text;
                Description = description;
            }

            public sealed class Header : SkillsListEntry
            {
                public Header(string text) : base(text, "")
                {
                }
            }

            public abstract class SkillOrResist : SkillsListEntry
            {
                public string PowerText { get; }
                public string DetailText { get; }

                public SkillOrResist(string text, string description, string power, string detail) : base(text, description)
                {
                    PowerText = power;
                    DetailText = detail;
                }
            }

            public sealed class Skill : SkillOrResist
            {
                public SkillPrototype SkillPrototype { get; }

                public override int OffsetX => -6;
                public override int OffsetDX => 18;

                public Skill(string text, string description, string power, string detail, SkillPrototype skillPrototype)
                    : base(text, description, power, detail)
                {
                    SkillPrototype = skillPrototype;
                }
            }

            public sealed class Resist : SkillOrResist
            {
                public ElementPrototype ElementPrototype { get; }

                public Resist(string text, string description, string power, string detail, ElementPrototype elementPrototype)
                    : base(text, description, power, detail)
                {
                    ElementPrototype = elementPrototype;
                }
            }
        }

        private sealed class SkillsListUIListCell : UiListCell<SkillsListEntry>
        {
            public int IndexInCategory { get; }

            [Child] private UiText TextDescription;
            [Child] private UiText TextPower;
            [Child] private UiText TextDetail;
            [Child] private AttributeIcon? Icon;

            public SkillsListUIListCell(SkillsListEntry data, int indexInCategory) : base(data, new UiText(UiFonts.ListText), null)
            {
                IndexInCategory = indexInCategory;
                TextDescription = new UiText(UiFonts.ListText);
                TextPower = new UiText(UiFonts.ListText);
                TextDetail = new UiText(UiFonts.ListText);

                switch (data)
                {
                    case SkillsListEntry.Header:
                        UiText.Font = UiFonts.SkillsListHeader;
                        Icon = null;
                        break;
                    case SkillsListEntry.Skill skill:
                        Icon = new AttributeIcon(skill.SkillPrototype.RelatedSkill);
                        break;
                    case SkillsListEntry.Resist:
                        Icon = new AttributeIcon(AttributeIcon.Regions.IconResistance);
                        break;
                }

                UpdateText();
            }

            private void UpdateText()
            {
                var text = Data.Text;

                if (Data is SkillsListEntry.Resist)
                {
                    text = Loc.GetString("Elona.CharaInfo.SkillsList.Resist.Name", ("elementName", text));
                }

                UiText.Text = text;
                TextDescription.Text = Data.Description;

                if (Data is SkillsListEntry.SkillOrResist skill)
                {
                    TextPower.Text = skill.PowerText;
                    TextDetail.Text = skill.DetailText;
                }
            }

            public override void GetPreferredSize(out Vector2 size)
            {
                size = (595, 18);
            }

            public override void SetSize(float width, float height)
            {
                base.SetSize(width, height);
                TextDescription.SetPreferredSize();
                TextPower.SetPreferredSize();
                TextDetail.SetPreferredSize();
                Icon?.SetPreferredSize();
            }

            public override void SetPosition(float x, float y)
            {
                base.SetPosition(x, y);
                TextDescription.SetPosition(X + 272, Y + 2);
                TextPower.SetPosition(X + 222 - TextPower.TextWidth, Y + 2);
                TextDetail.SetPosition(X + 224, Y + 2);

                switch (Data)
                {
                    case SkillsListEntry.Header:
                        UiText.SetPosition(X + 30, Y);
                        break;
                }

                Icon?.SetPosition(X - 20, Y + 9);
            }

            public override void Update(float dt)
            {
                base.Update(dt);
                TextDescription.Update(dt);
                TextPower.Update(dt);
                TextDetail.Update(dt);
                Icon?.Update(dt);
            }

            public override void Draw()
            {
                if (IndexInCategory % 2 == 0 && Data is not SkillsListEntry.Header)
                {
                    Love.Graphics.SetColor(UiColors.ListEntryAccent);
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X + Data.OffsetX, Y, Width - Data.OffsetDX, Height);
                }

                switch (Data)
                {
                    case SkillsListEntry.Header:
                        UiText.Draw();
                        break;
                    default:
                        base.Draw();
                        break;
                }

                TextDescription.Draw();
                TextPower.Draw();
                TextDetail.Draw();
                Icon?.Draw();
            }
        }

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IResistsSystem _resists = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        public int CurrentPage => List.CurrentPage;
        public int PageCount => List.PageCount;

        private EntityUid _charaEntity;

        private IAssetInstance AssetIeSheet;
        private const int SheetWidth = 700;
        private const int SheetHeight = 400;

        [Child] [Localize("Topic.Name")] private UiText TextTopicName = new UiTextTopic();
        [Child] [Localize("Topic.Level")] private UiText TextTopicLevel = new UiTextTopic();
        [Child] [Localize("Topic.Detail")] private UiText TextTopicDetail = new UiTextTopic();
        [Child] private UiText TextBonusPoints = new UiText(UiFonts.SkillsListBonusPoints);
        [Child] private UiPagedList<SkillsListEntry> List = new();

        [Localize] private LocaleScope _loc = Loc.MakeScope();

        public event UiListEventHandler<SkillsListEntry>? OnListItemActivated
        {
            add => List.OnActivated += value;
            remove => List.OnActivated -= value;
        }

        public SkillsListControl()
        {
            AssetIeSheet = Assets.Get(Protos.Asset.IeSheet);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            return keyHints;
        }

        public void Initialize(EntityUid charaEntity)
        {
            EntitySystem.InjectDependencies(this);

            _charaEntity = charaEntity;
        }

        private bool ShouldDisplaySkill(SkillPrototype proto)
        {
            return _skills.HasSkill(_charaEntity, proto);
        }

        private bool ShouldDisplayResist(ElementPrototype proto)
        {
            return true;
        }

        private string FormatSkillDetail(SkillPrototype skillProto)
        {
            if (!_entityManager.TryGetComponent(_charaEntity, out SkillsComponent skills))
                return string.Empty;

            var baseLevel = skills.BaseLevel(skillProto);
            var level = skills.Level(skillProto);

            if (level != baseLevel)
            {
                var grade = (level - baseLevel) / 5;
                return UiHelpers.FormatPowerText(grade, noBrackets: true);
            }

            return string.Empty;
        }

        private string FormatSkillPower(SkillPrototype skillProto)
        {
            if (!_entityManager.TryGetComponent(_charaEntity, out SkillsComponent skills)
                || !skills.TryGetKnown(skillProto, out var skill))
                return string.Empty;

            var baseLevel = skill.Level.Base;
            var level = skill.Level.Buffed;
            var exp = skill.Experience;
            var potential = skill.Potential;

            var powerText = $"{level}{exp / 1000f:.000}";

            if (_config.GetCVar(CCVars.DebugShowDetailedSkillPower) && level != baseLevel)
            {
                var diff = level - baseLevel;
                powerText += diff.ToString("+0;-#");
            }

            powerText += $"({potential}%)";

            return powerText;
        }

        private string FormatResistPower(ElementPrototype elementProto)
        {
            var grade = _resists.Grade(_charaEntity, elementProto);
            var powerText = ResistHelpers.GetGradeText(grade);

            if (_config.GetCVar(CCVars.DebugShowDetailedResistPower))
            {
                var level = _resists.Level(_charaEntity, elementProto);
                powerText += $" {level}";
            }

            return powerText;
        }

        public void RefreshFromEntity()
        {
            var cells = new List<SkillsListUIListCell>();

            var skillEntries = new List<SkillsListEntry>();
            var weaponProficiencyEntries = new List<SkillsListEntry>();
            var resistEntries = new List<SkillsListEntry>();

            SkillsListEntry MakeSkillEntry(SkillPrototype skillProto)
            {
                var name = Loc.GetPrototypeString(skillProto, "Name");
                var desc = Loc.GetPrototypeString(skillProto, "Description");
                var power = FormatSkillPower(skillProto);
                var detail = FormatSkillDetail(skillProto);

                return new SkillsListEntry.Skill(name, desc, power, detail, skillProto);
            }

            SkillsListEntry MakeResistEntry(ElementPrototype elementProto)
            {
                var name = Loc.GetPrototypeString(elementProto, "Name");
                var desc = Loc.GetPrototypeString(elementProto, "Description");
                var power = FormatResistPower(elementProto);
                var detail = string.Empty;

                return new SkillsListEntry.Resist(name, desc, power, detail, elementProto);
            }

            if (_entityManager.TryGetComponent(_charaEntity, out SkillsComponent skills))
            {
                foreach (var skillProto in _skills.EnumerateRegularSkills().Where(ShouldDisplaySkill))
                {
                    var entry = MakeSkillEntry(skillProto);
                    skillEntries.Add(entry);
                }

                foreach (var skillProto in _skills.EnumerateWeaponProficiencies().Where(ShouldDisplaySkill))
                {
                    var entry = MakeSkillEntry(skillProto);
                    weaponProficiencyEntries.Add(entry);
                }

                foreach (var elementProto in _resists.EnumerateResistableElements().Where(ShouldDisplayResist))
                {
                    var entry = MakeResistEntry(elementProto);
                    resistEntries.Add(entry);
                }
            }

            var skillsHeader = new SkillsListEntry.Header(_loc.GetString("Category.Skill"));
            var weaponProficienciesHeader = new SkillsListEntry.Header(_loc.GetString("Category.WeaponProficiency"));
            var resistsHeader = new SkillsListEntry.Header(_loc.GetString("Category.Resistance"));

            cells.Add(new SkillsListUIListCell(skillsHeader, 0));
            cells.AddRange(skillEntries.Select((e, i) => new SkillsListUIListCell(e, i)));

            cells.Add(new SkillsListUIListCell(weaponProficienciesHeader, 0));
            cells.AddRange(weaponProficiencyEntries.Select((e, i) => new SkillsListUIListCell(e, i)));

            cells.Add(new SkillsListUIListCell(resistsHeader, 0));
            cells.AddRange(resistEntries.Select((e, i) => new SkillsListUIListCell(e, i)));

            List.SetCells(cells);

            SetPage(Math.Clamp(CurrentPage, 0, Math.Max(0, PageCount - 1)));

            var isPlayerStatus = true; // TODO
            if (isPlayerStatus)
            {
                var bonusPoints = 0;
                if (skills != null)
                    bonusPoints = skills.BonusPoints;

                TextBonusPoints.Text = _loc.GetString("BonusPointsRemaining", ("bonusPoints", bonusPoints));
            }
            else
            {
                TextBonusPoints.Text = string.Empty;
            }
        }

        public event PageChangedDelegate? OnPageChanged
        {
            add => List.OnPageChanged += value;
            remove => List.OnPageChanged -= value;
        }

        public bool SetPage(int page)
        {
            var result = List.SetPage(page);

            return result;
        }

        public bool PageBackward()
        {
            return List.PageBackward();
        }

        public bool PageForward()
        {
            return List.PageForward();
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = AssetIeSheet.VirtualSize(UIScale);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            List.SetPreferredSize();
            TextTopicName.SetPreferredSize();
            TextTopicLevel.SetPreferredSize();
            TextTopicDetail.SetPreferredSize();
            TextBonusPoints.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            List.SetPosition(X + 58, Y + 64);
            TextTopicName.SetPosition(X + 28, Y + 36);
            TextTopicLevel.SetPosition(X + 200, Y + 36);
            TextTopicDetail.SetPosition(X + 350, Y + 36);
            TextBonusPoints.SetPosition(X + Width - TextBonusPoints.TextWidth - 140, Y + Height - 24 - Height % 8);
        }

        public override void Update(float dt)
        {
            TextTopicName.Update(dt);
            TextTopicLevel.Update(dt);
            TextTopicDetail.Update(dt);
            TextBonusPoints.Update(dt);
            List.Update(dt);
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(0, 0, 0, 75);
            AssetIeSheet.DrawUnscaled(PixelX + 4, PixelY + 4, SheetWidth * UIScale, SheetHeight * UIScale);
            GraphicsEx.SetColor(Color.White);
            AssetIeSheet.DrawUnscaled(PixelX, PixelY, SheetWidth * UIScale, SheetHeight * UIScale);
            TextTopicName.Draw();
            TextTopicLevel.Draw();
            TextTopicDetail.Draw();
            TextBonusPoints.Draw();
            List.Draw();
        }
    }
}
