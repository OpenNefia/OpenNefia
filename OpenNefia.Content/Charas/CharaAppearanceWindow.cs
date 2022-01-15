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

namespace OpenNefia.Content.Charas
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

    public class CharaAppearanceWindow : UiElement
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        private IAssetDrawable AppearanceDeco;

        private UiWindow Window;
        private UiTextTopic Category;
        private CharaAppearancePreviewPanel PreviewPanel;
        private CharaAppearanceList AppearanceList;

        public event UiListEventHandler<CharaAppearanceUICellData> List_OnActivated
        {
            add => AppearanceList.OnActivated += value;
            remove => AppearanceList.OnActivated -= value;
        }

        private CharaAppearanceData _data = default!;

        public CharaAppearanceWindow()
        {
            AppearanceDeco = new AssetDrawable(Protos.Asset.DecoMirrorA);
            Category = new UiTextTopic(Loc.GetString("Elona.CharaMake.AppearanceSelect.Topic.Category"));
            Window = new UiWindow()
            {
                KeyHints = MakeKeyHints()
            };
            AppearanceList = new CharaAppearanceList();
            PreviewPanel = new CharaAppearancePreviewPanel();

            AppearanceList.OnAppearanceItemChanged += HandleListAppearanceItemChanged;

            AddChild(AppearanceList);
        }

        public void Initialize(CharaAppearanceData data)
        {
            IoCManager.InjectDependencies(this);

            _data = data;
            Window.Title = Loc.GetString("Elona.CharaMake.AppearanceSelect.Window.Title");

            var pages = BuildListPages(data);
            AppearanceList.Initialize(data, pages);
            PreviewPanel.Initialize(data);
        }

        #region Model Updating

        private void UpdatePortrait(PortraitPrototype? currentValue)
        {
            _data.PortraitProto = currentValue ?? _protos.Index(Portrait.Default);
        }

        private void UpdatePCCPart(string partID, PCCPart? part)
        {
            if (part == null)
                return;


        }

        private void UpdatePCCPartColors(PCCPartType[] pCCPartTypes, Color currentValue)
        {
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
                    _data.UsePCC = customChara.UsePCC;
                    break;
                case CharaAppearanceUICellData.ChangePage changePage: // handled within the list
                case CharaAppearanceUICellData.Done done:
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
                var text = Loc.GetString($"Elona.CharaAppearance.Choices.{keySuffix}");
                return new CharaAppearanceUIListCell(cellData, text);
            };

            CharaAppearanceUIListCell MakePCCPartColorCell(IEnumerable<Color> partColors, PCCPartType[] pccPartTypes, LocaleKey keySuffix)
            {
                var cellData = new CharaAppearanceUICellData.PCCPartColor(partColors, pccPartTypes);
                var text = Loc.GetString($"Elona.CharaAppearance.Choices.{keySuffix}");
                return new CharaAppearanceUIListCell(cellData, text);
            }

            var parts = PCCHelpers.GetGroupedPCCParts(_protos);
            var partColors = PCCConstants.DefaultPartColors;
            var portraits = _protos.EnumeratePrototypes<PortraitPrototype>();

            var pages = new CharaAppearanceList.Pages();

            pages[CharaAppearancePage.Basic] = new()
            {
                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.Done(), Loc.GetString("Elona.CharaAppearance.Choices.Done")),
                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.Portrait(portraits), Loc.GetString("Elona.CharaAppearance.Choices.Basic.Portrait")),

                MakePCCPartCell(parts, PCCPartType.Hair, PCCPartSlots.Hair, "Basic.Hair"),
                MakePCCPartCell(parts, PCCPartType.Subhair, PCCPartSlots.SubHair, "Basic.SubHair"),
                MakePCCPartColorCell(partColors, new[] { PCCPartType.Hair, PCCPartType.Subhair }, "Basic.HairColor"),
                MakePCCPartCell(parts, PCCPartType.Body, PCCPartSlots.Body, "Basic.Body"),
                MakePCCPartCell(parts, PCCPartType.Cloth, PCCPartSlots.Cloth, "Basic.Cloth"),
                MakePCCPartCell(parts, PCCPartType.Pants, PCCPartSlots.Pants, "Basic.Pants"),

                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.ChangePage(CharaAppearancePage.Detail), Loc.GetString("Elona.CharaAppearance.Choices.Basic.SetDetail")),
                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.CustomChara(), Loc.GetString("Elona.CharaAppearance.Choices.Basic.CustomChara"))
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

                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.ChangePage(CharaAppearancePage.Basic), Loc.GetString("Elona.CharaAppearance.Choices.Detail.SetBasic")),
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

        public override void GetPreferredSize(out Vector2i size)
        {
            size = new(380, 340);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            Category.SetPreferredSize();
            AppearanceList.SetPreferredSize();
            PreviewPanel.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            Category.SetPosition(Window.X + 34, Window.Y + 36);
            AppearanceList.SetPosition(Window.X + 30, Window.Y + 65);
            AppearanceDeco.SetPosition(Window.X + Window.Width - 40, Window.Y);
            PreviewPanel.SetPosition(Window.X + 230, Window.Y + 70);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            Category.Draw();
            AppearanceList.Draw();
            AppearanceDeco.Draw();
            PreviewPanel.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            Category.Update(dt);
            AppearanceList.Update(dt);
            AppearanceDeco.Update(dt);
            PreviewPanel.Update(dt);
        }
    }
}