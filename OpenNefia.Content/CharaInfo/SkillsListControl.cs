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
using OpenNefia.Core.Prototypes;

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
            public Alignment DetailAlignment { get; }

            [Child] private UiText TextDescription;
            [Child] private UiText TextPower;
            [Child] private UiText TextDetail;
            [Child] private AttributeIcon? Icon;

            public SkillsListUIListCell(SkillsListEntry data, int indexInCategory, Alignment detailAlignment) : base(data, new UiText(UiFonts.ListText), null)
            {
                IndexInCategory = indexInCategory;
                TextDescription = new UiText(UiFonts.ListText);
                TextPower = new UiText(UiFonts.ListText);
                TextDetail = new UiText(UiFonts.ListText);
                DetailAlignment = detailAlignment;

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
                TextDescription.SetPosition(X + 272, Y);
                TextPower.SetPosition(X + 222 - TextPower.TextWidth, Y);

                switch (DetailAlignment)
                {
                    case Alignment.Left:
                    default:
                        TextDetail.SetPosition(X + 224, Y);
                        break;
                    case Alignment.Right:
                        TextDetail.SetPosition(X + 272 - TextDetail.Width - 10, Y);
                        break;
                }

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
                switch (Data)
                {
                    case SkillsListEntry.Header:
                        UiText.Draw();
                        break;
                    default:
                        base.Draw();
                        base.DrawLineTint(Width - Data.OffsetDX);
                        break;
                }

                TextDescription.Draw();
                TextPower.Draw();
                TextDetail.Draw();
                Icon?.Draw();
            }
        }

        public enum Alignment
        {
            Left,
            Right
        }

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IResistsSystem _resists = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        public int CurrentPage => List.CurrentPage;
        public int PageCount => List.PageCount;

        public Alignment DetailAlignment { get; set; } = Alignment.Left;

        private EntityUid _charaEntity;

        private IAssetInstance AssetIeSheet;
        private const int SheetWidth = 700;
        private const int SheetHeight = 400;

        [Child][Localize("Topic.Name")] private UiText TextTopicName = new UiTextTopic();
        [Child][Localize("Topic.Level")] private UiText TextTopicLevel = new UiTextTopic();
        [Child][Localize("Topic.Detail")] private UiText TextTopicDetail = new UiTextTopic();
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
            ShouldDisplaySkill = DefaultShouldDisplaySkill;
            ShouldDisplayResist = DefaultShouldDisplayResist;
            FormatSkillDetail = DefaultFormatSkillDetail;
            FormatSkillPower = DefaultFormatSkillPower;
            FormatResistPower = DefaultFormatResistPower;
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

        public Func<SkillPrototype, EntityUid, bool> ShouldDisplaySkill { get; set; } = (_, _) => true;
        public Func<ElementPrototype, EntityUid, bool> ShouldDisplayResist { get; set; } = (_, _) => true;
        public Func<SkillPrototype, EntityUid, string> FormatSkillDetail { get; set; } = (_, _) => string.Empty;
        public Func<SkillPrototype, EntityUid, string> FormatSkillPower { get; set; } = (_, _) => string.Empty;
        public Func<ElementPrototype, EntityUid, string> FormatResistPower { get; set; } = (_, _) => string.Empty;

        public bool DefaultShouldDisplaySkill(SkillPrototype proto, EntityUid charaEntity)
        {
            return proto.SkillType == SkillType.Skill && _skills.HasSkill(charaEntity, proto);
        }

        public bool DefaultShouldDisplayResist(ElementPrototype elementProto, EntityUid charaEntity)
        {
            return true;
        }

        public string DefaultFormatSkillDetail(SkillPrototype skillProto, EntityUid charaEntity)
        {
            if (!_entityManager.TryGetComponent(charaEntity, out SkillsComponent skills))
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

        public string DefaultFormatSkillPower(SkillPrototype skillProto, EntityUid charaEntity)
        {
            if (!_entityManager.TryGetComponent(charaEntity, out SkillsComponent skills)
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

        public string DefaultFormatResistPower(ElementPrototype elementProto, EntityUid charaEntity)
        {
            var grade = _resists.Grade(charaEntity, elementProto);
            var powerText = ResistHelpers.GetGradeText(grade);

            if (_config.GetCVar(CCVars.DebugShowDetailedResistPower))
            {
                var level = _resists.Level(charaEntity, elementProto);
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
                var power = FormatSkillPower(skillProto, _charaEntity);
                var detail = FormatSkillDetail(skillProto, _charaEntity);

                return new SkillsListEntry.Skill(name, desc, power, detail, skillProto);
            }

            SkillsListEntry MakeResistEntry(ElementPrototype elementProto)
            {
                var name = Loc.GetPrototypeString(elementProto, "Name");
                var desc = Loc.GetPrototypeString(elementProto, "Description");
                var power = FormatResistPower(elementProto, _charaEntity);
                var detail = string.Empty;

                return new SkillsListEntry.Resist(name, desc, power, detail, elementProto);
            }

            foreach (var skillProto in _protos.EnumeratePrototypes<SkillPrototype>().Where(p => ShouldDisplaySkill(p, _charaEntity)))
            {
                var entry = MakeSkillEntry(skillProto);
                skillEntries.Add(entry);
            }

            foreach (var skillProto in _skills.EnumerateWeaponProficiencies().Where(p => ShouldDisplaySkill(p, _charaEntity)))
            {
                var entry = MakeSkillEntry(skillProto);
                weaponProficiencyEntries.Add(entry);
            }

            foreach (var elementProto in _resists.EnumerateResistableElements().Where(p => ShouldDisplayResist(p, _charaEntity)))
            {
                var entry = MakeResistEntry(elementProto);
                resistEntries.Add(entry);
            }

            var skillsHeader = new SkillsListEntry.Header(_loc.GetString("Category.Skill"));
            var weaponProficienciesHeader = new SkillsListEntry.Header(_loc.GetString("Category.WeaponProficiency"));
            var resistsHeader = new SkillsListEntry.Header(_loc.GetString("Category.Resistance"));

            if (skillEntries.Count > 0)
            {
                cells.Add(new SkillsListUIListCell(skillsHeader, 0, DetailAlignment));
                cells.AddRange(skillEntries.Select((e, i) => new SkillsListUIListCell(e, i, DetailAlignment)));
            }

            if (weaponProficiencyEntries.Count > 0)
            {
                cells.Add(new SkillsListUIListCell(weaponProficienciesHeader, 0, DetailAlignment));
                cells.AddRange(weaponProficiencyEntries.Select((e, i) => new SkillsListUIListCell(e, i, DetailAlignment)));
            }

            if (resistEntries.Count > 0)
            {
                cells.Add(new SkillsListUIListCell(resistsHeader, 0, DetailAlignment));
                cells.AddRange(resistEntries.Select((e, i) => new SkillsListUIListCell(e, i, DetailAlignment)));
            }

            List.SetCells(cells);

            SetPage(Math.Clamp(CurrentPage, 0, Math.Max(0, PageCount - 1)));

            var isPlayerStatus = true; // TODO
            if (isPlayerStatus)
            {
                var bonusPoints = 0;
                if (_entityManager.TryGetComponent(_charaEntity, out SkillsComponent skills))
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
