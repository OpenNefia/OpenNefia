using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Layer.Inventory
{
    public sealed class InventoryLayer : BaseUiLayer<InventoryResult>
    {
        public class InventoryEntry
        {
            public InventoryEntry(EntityUid item, IInventorySource origin, string itemNameText, string itemDetailText, Color chipColor)
            {
                Item = item;
                Origin = origin;
                ItemNameText = itemNameText;
                ItemDetailText = itemDetailText;
                ChipColor = chipColor;
            }

            public EntityUid Item { get; set; }
            public IInventorySource Origin { get; set; }
            public string ItemNameText { get; set; }
            public string ItemDetailText { get; set; }
            public Color ChipColor { get; set; }
        }

        public class InventoryEntryCell : UiListCell<InventoryEntry>
        {
            private readonly IUiText UiSubtext;

            public InventoryEntryCell(InventoryEntry entry)
                : base(entry, new UiText())
            {
                UiSubtext = new UiText();
            }

            public override void SetSize(int width, int height)
            {
                UiText.SetSize(width, height);
                UiSubtext.SetSize(width, height);
                base.SetSize(Math.Max(width, UiText.Width), height);
            }

            public override void SetPosition(int x, int y)
            {
                base.SetPosition(x, y);
                UiText.SetPosition(X + 40 + XOffset + 4, Y + 1);
                UiSubtext.SetPosition(X + Width - 24 - UiSubtext.Width + XOffset + 4, Y + 2);
            }

            public override void Draw()
            {
                if (IndexInList % 2 == 0)
                {
                    Love.Graphics.SetColor(UiColors.ListEntryAccent);
                    Love.Graphics.Rectangle(Love.DrawMode.Fill, X - 1, Y, 540, 18);
                }

                Love.Graphics.SetColor(Color.White);
                AssetSelectKey.Draw(X, Y - 1);
                KeyNameText.Draw();

                Data.Origin.OnDraw();

                UiText.Draw();
                UiSubtext.Draw();
            }

            public override void Update(float dt)
            {
                KeyNameText.Update(dt);
                UiText.Update(dt);
                UiSubtext.Update(dt);
            }

            public override void Dispose()
            {
                base.Dispose();
            }
        }

        public bool PlaySounds { get; set; } = false;

        private UiWindow _window = new();
        private UiList<InventoryEntry> _list = new();
        
        private IUiText TextTotalWeight = new UiText();

        // private EntitySpriteBatch _spriteBatch = new();
        private InventoryContext _context;

        public InventoryLayer(InventoryContext context)
        {
            _context = context;
        }

        public override void OnQuery()
        {
            UpdateFiltering(playSounds: true);
        }

        private IEnumerable<InventoryEntry> GetFilteredEntries()
        {
            return Enumerable.Empty<InventoryEntry>();
        }

        private void UpdateFiltering(bool playSounds)
        {
            var filtered = GetFilteredEntries();

            _list.Clear();
            _list.AddRange(filtered.Select(e => new InventoryEntryCell(e)));

            var total_weight = UiUtils.DisplayWeight(1500);
            var max_weight = UiUtils.DisplayWeight(2500);
            var cargo_weight = UiUtils.DisplayWeight(3500);


        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
        }
    }
}
