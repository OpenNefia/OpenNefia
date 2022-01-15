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
        private IAssetDrawable AppearanceDeco;

        private UiWindow Window;
        private UiTextTopic Category;
        private CharaAppearancePreviewPanel PreviewPanel;
        private CharaAppearanceList List;

        public event UiListEventHandler<CharaAppearanceUICellData> List_OnActivated 
        {
            add => List.OnActivated += value;
            remove => List.OnActivated -= value;
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
            List = new CharaAppearanceList()
            {
                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.Done(), Loc.GetString("Elona.CharaMake.AppearanceSelect.Done")),
                new CharaAppearanceUIListCell(new CharaAppearanceUICellData.CustomChara(), "Customchara")
            };
            PreviewPanel = new CharaAppearancePreviewPanel();

            AddChild(List);
        }

        public void Initialize(CharaAppearanceData data)
        {
            _data = data;
            List.Initialize(data);
            PreviewPanel.Initialize(data);
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
            List.GrabFocus();
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
            List.SetPreferredSize();
            PreviewPanel.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            Category.SetPosition(Window.X + 30, Window.Y + 35);
            List.SetPosition(Window.X + 30, Window.Y + 65);
            AppearanceDeco.SetPosition(Window.X + Window.Width - 40, Window.Y);
            PreviewPanel.SetPosition(Window.X + 230, Window.Y + 70);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
            Category.Draw();
            List.Draw();
            AppearanceDeco.Draw();
            PreviewPanel.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            Category.Update(dt);
            List.Update(dt);
            AppearanceDeco.Update(dt);
            PreviewPanel.Update(dt);
        }
    }
}