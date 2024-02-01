using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.VisualAI.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIEditorTrail : UiElement
    {
        public VisualAIPlan RootPlan { get; set; } = new();

        public sealed record Data(IReadOnlyList<VisualAITile.Block> Blocks, int SelectedIndex);

        private Data _data = new(new List<VisualAITile.Block>(), 0);
        private float _offsetY = 0f;

        private const float ItemHeight = 80;

        [Child] private UiWindowBacking WindowBacking = new(Protos.Asset.Window);

        private List<VisualAIBlockCard> _trailCards = new();

        public void Refresh(Data data)
        {
            _data = data;

            foreach (var card in _trailCards)
            {
                RemoveChild(card);
            }
            _trailCards.Clear();

            for (var i = 0; i < _data.Blocks.Count; i++)
            {
                var blockTile = _data.Blocks[i];
                var blockProto = blockTile.BlockValue.Proto;
                var text = Loc.GetPrototypeString(blockTile.BlockValue.ProtoID, "Name");
                var indexText = (i + 1).ToString();
                var card = new VisualAIBlockCard(text, blockProto.Color, blockProto.Icon, indexText);
                UiHelpers.AddChildrenRecursive(this, card);
                _trailCards.Add(card);
            }

            RecalcLayout();
        }

        private void RecalcLayout()
        {
            _offsetY = 0;
            var selectedY = _data.SelectedIndex * ItemHeight + 10;

            var thresholdY = (selectedY + ItemHeight * 1.75f);

            if (thresholdY > Height)
            {
                _offsetY = float.Max(Height - thresholdY,
                                    (_data.Blocks.Count - (Height / ItemHeight)) * -ItemHeight - 54);
            }

            var pos = Position + (10, 18 + _offsetY);

            for (var i = 0; i < _trailCards.Count; i++)
            {
                var card = _trailCards[i];
                card.SetPosition(pos.X, pos.Y);
                card.SetSize(Width - 30, ItemHeight);
                card.IsSelected = i == _data.SelectedIndex;

                pos += (0, card.Height);
            }
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            WindowBacking.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            WindowBacking.SetPosition(X, Y);

            RecalcLayout();
        }

        public override void Update(float dt)
        {
            WindowBacking.Update(dt);
            foreach (var card in _trailCards)
            {
                card.Update(dt);
            }
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White);
            WindowBacking.Draw();

            //GraphicsS.SetScissorS(UIScale, X + 10, Y + 10 + 8, Width, Height - 54);
            foreach (var card in _trailCards)
            {
                card.Draw();
            }
            //Love.Graphics.SetScissor();
        }
    }
}
