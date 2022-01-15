using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
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

        public virtual string Text => "Test";

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

        public sealed class PCCPart : CharaAppearanceUICellData
        {
            private List<PCCs.PCCPart> _parts;
            private int _currentIndex;

            public override string Text => $"{CurrentValue?.Type}";

            public PCCs.PCCPart? CurrentValue
            {
                get
                {
                    if (_parts.Count == 0)
                        return null;
                    return _parts[_currentIndex];
                }
            }

            public PCCPart(IEnumerable<PCCs.PCCPart> parts)
            {
                _parts = parts.ToList();
            }

            public override void Change(int delta)
            {
                _currentIndex = (_currentIndex + delta) % _parts.Count;
            }
        }
    }

    public sealed class CharaAppearanceUIListCell : UiListCell<CharaAppearanceUICellData>
    {
        private IAssetDrawable AssetArrowLeft;
        private IAssetDrawable AssetArrowRight;

        public CharaAppearanceUIListCell(CharaAppearanceUICellData data, string text)
            : base(data, new UiText(text))
        {
            AssetArrowLeft = new AssetDrawable(Asset.ArrowLeft);
            AssetArrowRight = new AssetDrawable(Asset.ArrowRight);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            AssetArrowLeft.SetPreferredSize();
            AssetArrowRight.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            AssetArrowLeft.SetPosition(X - 30, Y - 5);
            AssetArrowRight.SetPosition(X + 115, Y - 5);
        }

        public void Change(int delta)
        {
            Data.Change(delta);
            UiText.Text = Data.Text;
        }

        public override void Update(float dt)
        {
            UiText.Update(dt);
            AssetArrowLeft.Update(dt);
            AssetArrowRight.Update(dt);
        }

        public override void Draw()
        {
            UiText.Draw();

            if (Data.DrawArrows)
            {
                AssetArrowLeft.Draw();
                AssetArrowRight.Draw();
            }
        }
    }

    public sealed class CharaAppearanceList : UiList<CharaAppearanceUICellData>
    {
        private CharaAppearanceData _data = default!;

        public CharaAppearanceList() : base()
        {
            OnActivated += HandleActivated;
        }

        public void Initialize(CharaAppearanceData data)
        {
            _data = data;
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

            switch (cell.Data)
            {
                case CharaAppearanceUICellData.CustomChara customChara:
                    _data.UsePCC = customChara.UsePCC;
                    break;
            }
  
            Sounds.Play(Sound.Cursor1);
        }
    }
}
