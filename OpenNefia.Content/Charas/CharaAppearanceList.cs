using OpenNefia.Content.PCCs;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Charas.CharaAppearanceWindow;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Charas
{
    public enum CharaAppearancePage
    {
        Basic,
        Detail
    }

    public abstract class CharaAppearanceUICellData
    {
        public virtual bool DrawArrows => true;

        public virtual string Text => "";

        public virtual void Change(int delta)
        {
        }

        public sealed class Done : CharaAppearanceUICellData
        {
            public override bool DrawArrows => false;
        }

        public sealed class ChangePage : CharaAppearanceUICellData
        {
            public CharaAppearancePage Page { get; set; }

            public ChangePage(CharaAppearancePage page)
            {
                Page = page;
            }
        }

        public sealed class Portrait : CharaAppearanceUICellData
        {
            private List<PortraitPrototype> _portraits;
            private int _currentIndex;

            public override string Text => _currentIndex.ToString();

            public PortraitPrototype? CurrentValue
            {
                get
                {
                    if (_portraits.Count == 0)
                        return null;
                    return _portraits[_currentIndex];
                }
            }

            public Portrait(IEnumerable<PortraitPrototype> portraits)
            {
                _portraits = portraits.ToList();
            }

            public override void Change(int delta)
            {
                _currentIndex = (_currentIndex + delta) % _portraits.Count;
            }
        }

        public sealed class PCCPart : CharaAppearanceUICellData
        {
            public string PartID { get; }

            private List<PCCs.PCCPart> _parts;
            private int _currentIndex;

            public override string Text => _currentIndex.ToString();

            public PCCs.PCCPart? CurrentValue
            {
                get
                {
                    if (_parts.Count == 0)
                        return null;
                    return _parts[_currentIndex];
                }
            }

            public PCCPart(IEnumerable<PCCs.PCCPart> parts, string partID)
            {
                PartID = partID;
                _parts = parts.ToList();
            }

            public override void Change(int delta)
            {
                _currentIndex = (_currentIndex + delta) % _parts.Count;
            }
        }

        public sealed class PCCPartColor : CharaAppearanceUICellData
        {
            public PCCPartType[] PCCPartTypes { get; }

            private List<Color> _colors;
            private int _currentIndex;

            public override string Text => _currentIndex.ToString();

            public Color CurrentValue
            {
                get => _colors.ElementAtOrDefault(_currentIndex);
            }

            public PCCPartColor(IEnumerable<Color> colors, PCCPartType[] pccPartTypes)
            {
                _colors = colors.ToList();
                PCCPartTypes = pccPartTypes;
            }

            public override void Change(int delta)
            {
                _currentIndex = (_currentIndex + delta) % _colors.Count;
            }
        }

        public sealed class CustomChara : CharaAppearanceUICellData
        {
            public bool UsePCC { get; set; }

            public override void Change(int delta)
            {
                if (delta < 0)
                    UsePCC = false;
                else
                    UsePCC = true;
            }
        }
    }

    public sealed class CharaAppearanceUIListCell : UiListCell<CharaAppearanceUICellData>
    {
        private IAssetDrawable AssetArrowLeft;
        private IAssetDrawable AssetArrowRight;

        private IUiText ValueText = new UiText();

        public CharaAppearanceUIListCell(CharaAppearanceUICellData data, string text)
            : base(data, new UiText(text))
        {
            AssetArrowLeft = new AssetDrawable(Asset.ArrowLeft);
            AssetArrowRight = new AssetDrawable(Asset.ArrowRight);

            RebuildText();
        }

        private void RebuildText()
        {
            ValueText.Text = Data.Text;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            AssetArrowLeft.SetPreferredSize();
            UiText.SetSize(55, UiText.Height);
            ValueText.SetSize(55, UiText.Height);
            AssetArrowRight.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            AssetArrowLeft.SetPosition(X, Y - 2);
            UiText.SetPosition(AssetArrowLeft.GlobalPixelBounds.Right + 5, Y + 2);
            var padding = UiText.Font.LoveFont.GetWidth(new string(' ', 8));
            ValueText.SetPosition(UiText.X + padding, Y + 2);
            AssetArrowRight.SetPosition(ValueText.GlobalPixelBounds.Right + 5 + 1, Y - 2);
        }

        public void Change(int delta)
        {
            Data.Change(delta);
            RebuildText();
        }

        public override void Update(float dt)
        {
            UiText.Update(dt);
            ValueText.Update(dt);
            AssetArrowLeft.Update(dt);
            AssetArrowRight.Update(dt);
        }

        public override void Draw()
        {
            UiText.Draw();
            ValueText.Draw();

            if (Data.DrawArrows)
            {
                AssetArrowLeft.Draw();
                AssetArrowRight.Draw();
            }
        }
    }

    public delegate void AppearanceListItemChangedDelegate(CharaAppearanceUIListCell cell, int delta);

    public sealed class CharaAppearanceList : UiList<CharaAppearanceUICellData>
    {
        public sealed class Pages : Dictionary<CharaAppearancePage, List<CharaAppearanceUIListCell>>
        {
        }

        private Pages _pages = new();

        private CharaAppearanceData _data = default!;

        public event AppearanceListItemChangedDelegate? OnAppearanceItemChanged;

        public CharaAppearanceList() : base()
        {
            OnActivated += HandleActivated;
        }

        public void Initialize(CharaAppearanceData data, Pages pages)
        {
            IoCManager.InjectDependencies(this);

            _data = data;
            _pages = pages;
            ChangePage(CharaAppearancePage.Basic);
        }

        public void ChangePage(CharaAppearancePage page)
        {
            Clear();
            AddRange(_pages[page]);
        }

        protected override void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.HandleKeyBindDown(args);

            if (args.Handled || SelectedCell is not CharaAppearanceUIListCell cell)
                return;

            if (args.Function == EngineKeyFunctions.UILeft)
            {
                Change(cell, -1);
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UIRight)
            {
                Change(cell, 1);
                args.Handle();
            }
        }

        private void HandleActivated(object? sender, UiListEventArgs<CharaAppearanceUICellData> evt)
        {
            // FIXME: #35
            if (evt.Handled || evt.SelectedCell is not CharaAppearanceUIListCell cell)
                return;

            if (cell.Data is not CharaAppearanceUICellData.Done)
            {
                Change(cell, 1);
                evt.Handle();
            }
        }

        private void Change(CharaAppearanceUIListCell cell, int delta)
        {
            cell.Change(delta);

            if (cell.Data is CharaAppearanceUICellData.ChangePage changePage)
            {
                ChangePage(changePage.Page);
            }

            OnAppearanceItemChanged?.Invoke(cell, delta);

            Sounds.Play(Sound.Cursor1);
        }
    }
}
