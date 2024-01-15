using OpenNefia.Content.PCCs;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Utility;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI;
using OpenNefia.Content.Portraits;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.CharaAppearance
{
    public enum CharaAppearancePage
    {
        Basic,
        Detail
    }

    public abstract class CharaAppearanceUICellData
    {
        public virtual bool DrawArrows => true;

        public virtual string Text => string.Empty;

        public virtual void Change(int delta)
        {
        }

        public sealed class Done : CharaAppearanceUICellData
        {
            public override bool DrawArrows => false;
        }

        public sealed class ChangePage : CharaAppearanceUICellData
        {
            public override bool DrawArrows => false;
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

            public override string Text
            {
                get
                {
                    if (CurrentValue == null || CurrentValue.GetStrongID() == Protos.Portrait.Default)
                        return "N/A";

                    // the default portrait should be the first in the list.
                    return (_currentIndex - 1).ToString();
                }
            }

            public PortraitPrototype? CurrentValue
            {
                get
                {
                    if (_portraits.Count == 0)
                        return null;
                    return _portraits[_currentIndex];
                }
                set
                {
                    if (value == null)
                        _currentIndex = 0;
                    else
                        _currentIndex = Math.Clamp(_portraits.IndexOf(value), 0, _portraits.Count - 1);
                }
            }

            public Portrait(IEnumerable<PortraitPrototype> portraits)
            {
                _portraits = portraits.ToList();
                _portraits.MoveElementWhere(x => x.GetStrongID() == Protos.Portrait.Default, 0);
            }

            public override void Change(int delta)
            {
                _currentIndex = MathHelper.Wrap(_currentIndex + delta, 0, _portraits.Count - 1);
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
                    // index 0 counts as "no part in this slot."
                    if (_parts.Count == 0 || _currentIndex == 0)
                        return null;

                    return _parts[_currentIndex - 1];
                }
                set
                {
                    if (value == null)
                        _currentIndex = 0;
                    else
                        _currentIndex = Math.Clamp(_parts.FindIndex(x => x.ImagePath == value.ImagePath), 0, _parts.Count - 1) + 1;
                }
            }

            public PCCPart(IEnumerable<PCCs.PCCPart> parts, string partID)
            {
                PartID = partID;
                _parts = parts.ToList();
            }

            public override void Change(int delta)
            {
                // +1 extra for the "no part here" choice in index 0
                _currentIndex = MathHelper.Wrap(_currentIndex + delta, 0, _parts.Count - 1 + 1);
            }
        }

        public sealed class PCCPartColor : CharaAppearanceUICellData
        {
            public HashSet<PCCPartType> PCCPartTypes { get; }

            private List<Color> _colors;
            private int _currentIndex;

            public override string Text => _currentIndex.ToString();

            public Color CurrentValue
            {
                get => _colors.ElementAtOrDefault(_currentIndex);
                set
                {
                    _currentIndex = Math.Clamp(_colors.IndexOf(value), 0, _colors.Count - 1);
                }
            }

            public PCCPartColor(IEnumerable<Color> colors, IEnumerable<PCCPartType> pccPartTypes)
            {
                _colors = colors.ToList();
                PCCPartTypes = pccPartTypes.ToHashSet();
            }

            public override void Change(int delta)
            {
                _currentIndex = MathHelper.Wrap(_currentIndex + delta, 0, _colors.Count - 1);
            }
        }

        public sealed class CustomChara : CharaAppearanceUICellData
        {
            public bool UsePCC { get; set; }

            public override string Text => UsePCC ? "1" : "0";

            public override void Change(int delta)
            {
                if (delta < 0)
                    UsePCC = false;
                else
                    UsePCC = true;
            }
        }

        public sealed class PCCFullSize : CharaAppearanceUICellData
        {
            public bool IsFullSize { get; set; }

            public override string Text => IsFullSize ? "1" : "0";

            public override void Change(int delta)
            {
                if (delta < 0)
                    IsFullSize = false;
                else
                    IsFullSize = true;
            }
        }
    }

    public sealed class CharaAppearanceUIListCell : UiListCell<CharaAppearanceUICellData>
    {
        [Child] private AssetDrawable AssetArrowLeft;
        [Child] private AssetDrawable AssetArrowRight;
        [Child] private UiText TextValue;

        public CharaAppearanceUIListCell(CharaAppearanceUICellData data, string text)
            : base(data, new UiText(text))
        {
            AssetArrowLeft = new AssetDrawable(Asset.ArrowLeft);
            AssetArrowRight = new AssetDrawable(Asset.ArrowRight);
            TextValue = new UiText(UiFonts.ListText);

            RebuildText();
        }

        public void RebuildText()
        {
            TextValue.Text = Data.Text;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            AssetArrowLeft.SetPreferredSize();
            TextValue.SetPreferredSize();
            AssetArrowRight.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            AssetArrowLeft.SetPosition(X + 115, Y - 5);
            TextValue.SetPosition(AssetArrowLeft.Rect.Right + 5, Y);
            AssetArrowRight.SetPosition(X + 173, Y - 5);
        }

        public void Change(int delta)
        {
            Data.Change(delta);
            RebuildText();
        }

        public override void Update(float dt)
        {
            UiText.Update(dt);
            AssetArrowLeft.Update(dt);
            TextValue.Update(dt);
            AssetArrowRight.Update(dt);
        }

        public override void Draw()
        {
            base.Draw();
            TextValue.Draw();

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

        public event AppearanceListItemChangedDelegate? OnAppearanceItemChanged;

        public CharaAppearanceList() : base()
        {
            OnActivated += HandleActivated;
        }

        public void Initialize(Pages pages, CharaAppearanceData data)
        {
            IoCManager.InjectDependencies(this);

            _pages = pages;

            SetListValuesFromAppearanceData(data);

            ChangePage(CharaAppearancePage.Basic);
        }

        public void ChangePage(CharaAppearancePage page)
        {
            var index = SelectedIndex;
            SetCells(_pages[page], dispose: false);
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

        /// <summary>
        /// Binds the values in the appearance data to their UI elements.
        /// </summary>
        private void SetListValuesFromAppearanceData(CharaAppearanceData data)
        {
            foreach (var cell in _pages.Values.SelectMany(x => x))
            {
                switch (cell.Data)
                {
                    case CharaAppearanceUICellData.Portrait portrait:
                        portrait.CurrentValue = data.PortraitProto;
                        break;
                    case CharaAppearanceUICellData.PCCPart pccPart:
                        if (data.PCCDrawable.Parts.TryGetValue(pccPart.PartID, out var part))
                        {
                            pccPart.CurrentValue = part;
                        }
                        break;
                    case CharaAppearanceUICellData.PCCPartColor pccPartColor:
                        // Find the first PCC part with the target type.
                        var firstPCCPart = data.PCCDrawable.Parts.Values.FirstOrDefault(part => pccPartColor.PCCPartTypes.Contains(part.Type));
                        if (firstPCCPart != null)
                        {
                            pccPartColor.CurrentValue = firstPCCPart.Color;
                        }
                        break;
                    case CharaAppearanceUICellData.CustomChara customChara:
                        customChara.UsePCC = data.UsePCC;
                        break;
                    case CharaAppearanceUICellData.PCCFullSize pccFullSize:
                        pccFullSize.IsFullSize = data.IsFullSize;
                        break;
                }

                cell.RebuildText();
            }
        }
    }
}
