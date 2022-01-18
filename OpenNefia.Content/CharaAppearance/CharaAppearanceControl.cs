using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.PCCs;
using OpenNefia.Content.UI;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Charas;

namespace OpenNefia.Content.CharaAppearance
{
    public sealed class CharaAppearanceData
    {
        public ChipPrototype ChipProto { get; set; }
        public Color ChipColor { get; set; }
        public PortraitPrototype PortraitProto { get; set; }
        public PCCDrawable PCCDrawable { get; set; }
        public bool UsePCC { get; set; }

        public CharaAppearanceData(ChipPrototype chipProto, Color chipColor, PortraitPrototype portraitProto, PCCDrawable pccDrawable, bool usePCC)
        {
            ChipProto = chipProto;
            ChipColor = chipColor;
            PortraitProto = portraitProto;
            PCCDrawable = pccDrawable;
            UsePCC = usePCC;
        }
    }

    public class CharaAppearanceControl : UiElement
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        private AssetDrawable AppearanceDeco;

        private UiWindow Window;
        private UiTextTopic TextTopicCategory;
        private CharaAppearancePreviewPanel PreviewPanel;
        private CharaAppearanceList AppearanceList;

        private LocaleScope _locScope = Loc.MakeScope("Elona.CharaAppearance.Control");

        public event UiListEventHandler<CharaAppearanceUICellData> List_OnActivated
        {
            add => AppearanceList.OnActivated += value;
            remove => AppearanceList.OnActivated -= value;
        }

        public CharaAppearanceData AppearanceData { get; private set; } = default!;

        public CharaAppearanceControl()
        {
            AppearanceDeco = new AssetDrawable(Asset.DecoMirrorA);
            TextTopicCategory = new UiTextTopic(_locScope.GetString("Topic.Category"));
            Window = new UiWindow()
            {
                KeyHints = MakeKeyHints()
            };
            AppearanceList = new CharaAppearanceList();
            PreviewPanel = new CharaAppearancePreviewPanel();

            AppearanceList.OnSelected += HandleListSelected;
            AppearanceList.OnAppearanceItemChanged += HandleListAppearanceItemChanged;

            AddChild(AppearanceDeco);
            AddChild(Window);
            AddChild(TextTopicCategory);
            AddChild(AppearanceList);
            AddChild(PreviewPanel);
        }

        public void Initialize(CharaAppearanceData data)
        {
            IoCManager.InjectDependencies(this);

            AppearanceData = data;
            Window.Title = _locScope.GetString("Window.Title.Appearance");

            var pages = BuildListPages(data);
            AppearanceList.Initialize(pages, data);
            PreviewPanel.Initialize(data);
        }

        #region Model Updating

        private void UpdatePortrait(PortraitPrototype? newPortrait)
        {
            AppearanceData.PortraitProto = newPortrait ?? _protos.Index(Portrait.Default);
        }

        private Color GetPCCPartColor(PCCPartType type)
        {
            var uiCell = AppearanceList.Select(cell => cell.Data)
                .WhereAssignable<CharaAppearanceUICellData, CharaAppearanceUICellData.PCCPartColor>()
                .Where(data => data.PCCPartTypes.Contains(type))
                .FirstOrDefault();

            return uiCell != null ? uiCell.CurrentValue : PCCConstants.DefaultPCCPartColors.First();
        }

        private void UpdatePCCPart(string partID, PCCPart? part)
        {
            if (part == null)
            {
                AppearanceData.PCCDrawable.Parts.Remove(partID);
            }
            else
            {
                AppearanceData.PCCDrawable.Parts[partID] = part;
                part.Color = GetPCCPartColor(part.Type);
            }

            AppearanceData.PCCDrawable.RebakeImage(_resourceCache);
        }

        private void UpdatePCCPartColors(IReadOnlySet<PCCPartType> pccPartTypes, Color newColor)
        {
            foreach (var part in AppearanceData.PCCDrawable.Parts.Values)
            {
                if (pccPartTypes.Contains(part.Type))
                {
                    part.Color = newColor;
                }
            }

            AppearanceData.PCCDrawable.RebakeImage(_resourceCache);
        }

        private void HandleListSelected(object? sender, UiListEventArgs<CharaAppearanceUICellData> evt)
        {
            // FIXME: #35
            if (evt.Handled || evt.SelectedCell is not CharaAppearanceUIListCell cell)
                return;

            if (cell.Data is CharaAppearanceUICellData.Portrait)
                PreviewPanel.ShowPortrait = true;
            else
                PreviewPanel.ShowPortrait = false;
        }

        private void HandleListAppearanceItemChanged(CharaAppearanceUIListCell cell, int delta)
        {
            switch (cell.Data)
            {
                case CharaAppearanceUICellData.PCCPart pccPart:
                    UpdatePCCPart(pccPart.PartID, pccPart.CurrentValue);
                    break;
                case CharaAppearanceUICellData.PCCPartColor pccPartColor:
                    UpdatePCCPartColors(pccPartColor.PCCPartTypes, pccPartColor.CurrentValue);
                    break;
                case CharaAppearanceUICellData.Portrait portrait:
                    UpdatePortrait(portrait.CurrentValue);
                    break;
                case CharaAppearanceUICellData.CustomChara customChara:
                    AppearanceData.UsePCC = customChara.UsePCC;
                    break;
                case CharaAppearanceUICellData.ChangePage:
                    // Handled internally in the list.
                    break;
                case CharaAppearanceUICellData.Done:
                default:
                    break;
            }
        }

        #endregion

        private CharaAppearanceList.Pages BuildListPages(CharaAppearanceData data)
        {
            CharaAppearanceUIListCell MakePCCPartCell(Dictionary<PCCPartType, List<PCCPart>> parts, PCCPartType partType, string partID, LocaleKey keySuffix)
            {
                var pccParts = parts[partType];
                var cellData = new CharaAppearanceUICellData.PCCPart(pccParts, partID);
                var text = _locScope.GetString($"Choices.{keySuffix}");
                return new CharaAppearanceUIListCell(cellData, text);
            };

            CharaAppearanceUIListCell MakePCCPartColorCell(IEnumerable<Color> partColors, PCCPartType[] pccPartTypes, LocaleKey keySuffix)
            {
                var cellData = new CharaAppearanceUICellData.PCCPartColor(partColors, pccPartTypes);
                var text = _locScope.GetString($"Choices.{keySuffix}");
                return new CharaAppearanceUIListCell(cellData, text);
            }

            var parts = PCCHelpers.GetGroupedPCCParts(_protos);
            var partColors = PCCConstants.DefaultPCCPartColors;
            var portraits = _protos.EnumeratePrototypes<PortraitPrototype>();

            var pages = new CharaAppearanceList.Pages();

            pages[CharaAppearancePage.Basic] = new()
            {
                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.Done(), _locScope.GetString("Choices.Done")),
                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.Portrait(portraits), _locScope.GetString("Choices.Basic.Portrait")),

                MakePCCPartCell(parts, PCCPartType.Hair, PCCPartSlots.Hair, "Basic.Hair"),
                MakePCCPartCell(parts, PCCPartType.Subhair, PCCPartSlots.SubHair, "Basic.SubHair"),
                MakePCCPartColorCell(partColors, new[] { PCCPartType.Hair, PCCPartType.Subhair }, "Basic.HairColor"),
                MakePCCPartCell(parts, PCCPartType.Body, PCCPartSlots.Body, "Basic.Body"),
                MakePCCPartCell(parts, PCCPartType.Cloth, PCCPartSlots.Cloth, "Basic.Cloth"),
                MakePCCPartCell(parts, PCCPartType.Pants, PCCPartSlots.Pants, "Basic.Pants"),

                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.ChangePage(CharaAppearancePage.Detail), _locScope.GetString("Choices.Basic.SetDetail")),
                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.CustomChara(), _locScope.GetString("Choices.Basic.CustomChara"))
            };

            pages[CharaAppearancePage.Detail] = new()
            {
                MakePCCPartColorCell(partColors, new[] { PCCPartType.Body, PCCPartType.Eye }, "Detail.BodyColor"),
                MakePCCPartColorCell(partColors, new[] { PCCPartType.Cloth }, "Detail.ClothColor"),
                MakePCCPartColorCell(partColors, new[] { PCCPartType.Pants }, "Detail.PantsColor"),
                MakePCCPartCell(parts, PCCPartType.Etc, PCCPartSlots.Etc1, "Detail.Etc1"),
                MakePCCPartCell(parts, PCCPartType.Etc, PCCPartSlots.Etc2, "Detail.Etc2"),
                MakePCCPartCell(parts, PCCPartType.Etc, PCCPartSlots.Etc3, "Detail.Etc3"),
                MakePCCPartCell(parts, PCCPartType.Eye, PCCPartSlots.Eye, "Detail.Eyes"),

                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.ChangePage(CharaAppearancePage.Basic), _locScope.GetString("Choices.Detail.SetBasic")),
            };

            return pages;
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.Change, UiKeyNames.LeftRight));
            keyHints.Add(new(UiKeyHints.Close, UiKeyNames.Cancel));

            return keyHints;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            AppearanceList.GrabFocus();
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = new(380, 340);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            TextTopicCategory.SetPreferredSize();
            AppearanceList.SetPreferredSize();
            PreviewPanel.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            TextTopicCategory.SetPosition(Window.X + 34, Window.Y + 36);
            AppearanceList.SetPosition(Window.X + 30, Window.Y + 65);
            AppearanceDeco.SetPosition(Window.X + Window.Width - 40, Window.Y);
            PreviewPanel.SetPosition(Window.X + 230, Window.Y + 70);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            TextTopicCategory.Draw();
            AppearanceList.Draw();
            AppearanceDeco.Draw();
            PreviewPanel.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            TextTopicCategory.Update(dt);
            AppearanceList.Update(dt);
            AppearanceDeco.Update(dt);
            PreviewPanel.Update(dt);
        }
    }
}