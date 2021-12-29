using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Logic;
using OpenNefia.Core.IoC;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.Inventory
{
    public sealed class InventoryLayer : UiLayerWithResult<InventoryResult>
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
            private readonly EntitySpriteBatch SpriteBatch;

            public InventoryEntryCell(InventoryEntry entry, EntitySpriteBatch spriteBatch)
                : base(entry, new UiText())
            {
                SpriteBatch = spriteBatch;

                UiSubtext = new UiText();

                UiText.Text = entry.ItemNameText;
                UiSubtext.Text = entry.ItemDetailText;
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
                UiText.SetPosition(X + 30 + XOffset, Y + 1);
                UiSubtext.SetPosition(X + Width - 44 - UiSubtext.TextWidth + XOffset + 4, Y + 2);
            }

            public override void Draw()
            {
                if (IndexInList % 2 == 0)
                {
                    Love.Graphics.SetColor(UiColors.ListEntryAccent);
                    Love.Graphics.Rectangle(Love.DrawMode.Fill, X - 1, Y, Width - 30, 18);
                }

                Love.Graphics.SetColor(Color.White);
                AssetSelectKey.Draw(X, Y - 1);
                KeyNameText.Draw();

                Data.Origin.OnDraw();

                UiText.Draw();
                UiSubtext.Draw();

                SpriteBatch.Add(Data.Item, X - 21, Y + 11, color: Data.ChipColor, centered: true);
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

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMgr = default!;

        public bool PlaySounds { get; set; } = false;

        private UiWindow Window = new();
        private UiList<InventoryEntry> List = new();

        private IUiText TextTopicWindowName = new UiTextTopic("Item");
        private IUiText TextTopicDetailHeader = new UiTextTopic("Detail");
        private IUiText TextTotalWeight = new UiText(UiFonts.TextNote);
        private IUiText TextGoldCount = new UiText(UiFonts.InventoryGoldCount);

        private IAssetDrawable? CurrentIcon;

        private IAssetDrawable AssetDecoInvA;
        private IAssetDrawable AssetDecoInvB;
        private IAssetDrawable AssetDecoInvC;
        private IAssetDrawable AssetDecoInvD;
        private IAssetDrawable AssetGoldCoin;

        private EntitySpriteBatch SpriteBatch = new();
        private InventoryContext Context;

        public InventoryLayer(InventoryContext context)
        {
            EntitySystem.InjectDependencies(this);

            Context = context;
            
            Window.Title = Context.Behavior.WindowTitle;

            AssetDecoInvA = new AssetDrawable(AssetPrototypeOf.DecoInvA);
            AssetDecoInvB = new AssetDrawable(AssetPrototypeOf.DecoInvB);
            AssetDecoInvC = new AssetDrawable(AssetPrototypeOf.DecoInvC);
            AssetDecoInvD = new AssetDrawable(AssetPrototypeOf.DecoInvD);
            AssetGoldCoin = new AssetDrawable(AssetPrototypeOf.GoldCoin);

            BindKeys();

            UpdateFiltering();
        }

        private void BindKeys()
        {
            //Keybinds[CoreKeybinds.Cancel] += (_) => Cancel();
            //Keybinds[CoreKeybinds.Escape] += (_) => Cancel();
            //Keybinds[CoreKeybinds.Identify] += ShowItemDescription;

            //Forwards += List;

            List.EventOnActivate += OnSelect;
        }

        private void ShowItemDescription(GUIBoundKeyEventArgs args)
        {
            var selected = List.SelectedCell;

            if (selected == null)
                return;

            var layer = new ItemDescriptionLayer(selected.Data.Item);
            //_uiLayerManager.Query(layer);
        }

        public void OnSelect(object? sender, UiListEventArgs<InventoryEntry> e)
        {
            var entry = e.SelectedCell.Data;
            var result = Context.OnSelect(entry.Item);

            switch (result)
            {
                case InventoryResult.Finished:
                    this.Finish(result);
                    break;
                case InventoryResult.Continuing:
                default:
                    break;
            }

            // TODO smarter list rebuilding
            UpdateFiltering();
            _field.RefreshScreen();

            if (List.Count == 0)
            {
                this.Finish(new InventoryResult.Finished(TurnResult.Succeeded));
            }
        }

        public override void OnQuery()
        {
            Sounds.Play(Protos.Sound.Inv);
            ShowQueryText();
            Context.OnQuery();
        }

        private void ShowQueryText()
        {
            var text = Context.GetQueryText();
            if (!string.IsNullOrEmpty(text))
            {
                Mes.Newline();
                Mes.Display(text);
            }
        }

        private string DefaultDetailText(EntityUid ent)
        {
            int? weight = null;

            if (_entityManager.TryGetComponent(ent, out WeightComponent? weightComp))
            {
                weight = weightComp.Weight;
            }
            if (_entityManager.TryGetComponent(ent, out CargoComponent? cargoComp))
            {
                weight = cargoComp.CargoWeight;
            }

            if (weight != null)
            {
                return UiUtils.DisplayWeight(weight.Value);
            }

            return "-";
        }

        private IEnumerable<InventoryEntry> GetFilteredEntries()
        {
            InventoryEntry ToEntry(EntityUid ent, IInventorySource source)
            {
                var itemName = DisplayNameSystem.GetDisplayName(ent);
                var itemDetail = DefaultDetailText(ent);
                var chipColor = Color.White;
                if (_entityManager.TryGetComponent(ent, out ChipComponent? chip))
                    chipColor = chip.Color;

                return new InventoryEntry(ent, source, itemName, itemDetail, chipColor);
            };

            var entries = new List<InventoryEntry>();

            foreach (var (ent, source) in Context.GetFilteredEntities())
            {
                entries.Add(ToEntry(ent, source));
            }

            return entries;
        }

        private void UpdateFiltering()
        {
            var filtered = GetFilteredEntries();

            var index = List.SelectedIndex;

            List.Clear();
            List.AddRange(filtered.Select(e => new InventoryEntryCell(e, SpriteBatch)));

            List.SelectedIndex = index;

            var totalWeight = UiUtils.DisplayWeight(1500);
            var maxWeight = UiUtils.DisplayWeight(2500);
            var cargoWeight = UiUtils.DisplayWeight(3500);

            if (Context.Behavior.ShowTotalWeight)
            {
                var weightText = totalWeight;
                TextTotalWeight.Text = $"{List.Count} items  ({weightText})";
            }
            else
            {
                TextTotalWeight.Text = string.Empty;
            }
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            UiUtils.GetCenteredParams(640, 432, out bounds);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);

            Window.SetSize(Width, Height);
            List.SetSize(Width - 58, Height - 60);
            SpriteBatch.SetSize(0, 0);

            AssetDecoInvA.SetPreferredSize();
            AssetDecoInvB.SetPreferredSize();
            AssetDecoInvC.SetPreferredSize();
            AssetDecoInvD.SetPreferredSize();
            AssetGoldCoin.SetPreferredSize();

            TextTopicWindowName.SetPreferredSize();
            TextTopicDetailHeader.SetPreferredSize();
            TextTotalWeight.SetPreferredSize();
            TextGoldCount.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            Window.SetPosition(X, Y);
            List.SetPosition(X + 58, Y + 60);
            SpriteBatch.SetPosition(0, 0);

            AssetDecoInvA.SetPosition(X + Width - 136, Y - 6);
            AssetDecoInvB.SetPosition(X + Width - 186, Y - 6);
            AssetDecoInvC.SetPosition(X + Width - 246, Y - 6);
            AssetDecoInvD.SetPosition(X - 6, Y - 6);
            AssetGoldCoin.SetPosition(X + 340, Y + 32);

            TextTopicWindowName.SetPosition(X + 28, Y + 30);
            TextTopicDetailHeader.SetPosition(X + 526, Y + 30);
            var notePos = UiUtils.NotePosition(PixelBounds, TextTotalWeight);
            TextTotalWeight.SetPosition(notePos.X, notePos.Y);
            TextGoldCount.SetPosition(X + 368, Y + 37);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            List.Update(dt);
            SpriteBatch.Update(dt);

            AssetDecoInvA.Update(dt);
            AssetDecoInvB.Update(dt);
            AssetDecoInvC.Update(dt);
            AssetDecoInvD.Update(dt);
            AssetGoldCoin.Update(dt);

            TextTopicWindowName.Update(dt);
            TextTopicDetailHeader.Update(dt);
            TextTotalWeight.Update(dt);
            TextGoldCount.Update(dt);
        }

        public override void Draw()
        {
            Window.Draw();

            AssetDecoInvA.Draw();
            AssetDecoInvB.Draw();
            AssetDecoInvC.Draw();
            AssetDecoInvD.Draw();

            TextTopicWindowName.Draw();
            TextTopicDetailHeader.Draw();
            TextTotalWeight.Draw();

            // List will update the sprite batch.
            SpriteBatch.Clear();

            List.Draw();

            SpriteBatch.Draw();

            if (Context.Behavior.ShowMoney)
            {
                AssetGoldCoin.Draw();
                TextGoldCount.Draw();
            }
        }
    }
}
