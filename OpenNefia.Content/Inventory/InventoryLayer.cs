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
using OpenNefia.Core.Input;
using OpenNefia.Content.Input;
using OpenNefia.Core.Log;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.GameObjects.Pickable;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Cargo;

namespace OpenNefia.Content.Inventory
{
    public interface IInventoryLayer
    {
        int SelectedIndex { get; }
        InventoryEntry? SelectedEntry { get; }
        InventoryContext Context { get; }

        void RefreshList(InventoryRefreshListKind kind);
    }

    public enum InventoryRefreshListKind
    {
        Redisplay,
        RebuildList
    }

    public class InventoryEntry
    {
        public InventoryEntry(EntityUid item, IInventorySource origin, string itemNameText, string itemDetailText, Color chipColor)
        {
            ItemEntityUid = item;
            Origin = origin;
            ItemNameText = itemNameText;
            ItemDetailText = itemDetailText;
            ChipColor = chipColor;
        }

        public EntityUid ItemEntityUid { get; set; }
        public IInventorySource Origin { get; set; }
        public string ItemNameText { get; set; }
        public string ItemDetailText { get; set; }
        public Color ChipColor { get; set; }
    }
    
    public sealed class InventoryLayer : UiLayerWithResult<InventoryContext, InventoryLayer.Result>, IInventoryLayer
    {
        public new class Result
        {
            public InventoryResult Data;
            public EntityUid? SelectedItem;

            public Result(InventoryResult data, EntityUid? selectedItem)
            {
                Data = data;
                SelectedItem = selectedItem;
            }
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

                SpriteBatch.Add(Data.ItemEntityUid, X - 21, Y + 11, color: Data.ChipColor, centered: true);
            }

            public override void Update(float dt)
            {
                KeyNameText.Update(dt);
                UiText.Update(dt);
                UiSubtext.Update(dt);
            }

            public void RefreshFromItem(IEntityManager entityManager)
            {
                UiText.Color = InventoryHelpers.GetItemTextColor(Data.ItemEntityUid, entityManager);
            }

            public override void Dispose()
            {
                base.Dispose();
                UiSubtext.Dispose();
            }
        }

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMgr = default!;
        [Dependency] private readonly IInventorySystem _invSys = default!;
        [Dependency] private readonly ICargoSystem _cargoSys = default!;

        public bool PlaySounds { get; set; } = false;

        private UiWindow Window = new();
        private UiList<InventoryEntry> List = new();

        public int SelectedIndex => List.SelectedIndex;
        public InventoryEntry? SelectedEntry => List.SelectedCell?.Data;

        private IUiText TextTopicItemName = new UiTextTopic();
        private IUiText TextTopicItemDetail = new UiTextTopic();
        private IUiText TextNoteTotalWeight = new UiText(UiFonts.TextNote);
        private IUiText TextGoldCount = new UiText(UiFonts.InventoryGoldCount);

        private IAssetDrawable AssetDecoInvA;
        private IAssetDrawable AssetDecoInvB;
        private IAssetDrawable AssetDecoInvC;
        private IAssetDrawable AssetDecoInvD;
        private IAssetDrawable AssetGoldCoin;

        private IUiElement? CurrentIcon;

        private EntitySpriteBatch _spriteBatch = new();

        public InventoryContext Context { get; private set; } = default!;

        public InventoryLayer()
        {
            AssetDecoInvA = new AssetDrawable(AssetPrototypeOf.DecoInvA);
            AssetDecoInvB = new AssetDrawable(AssetPrototypeOf.DecoInvB);
            AssetDecoInvC = new AssetDrawable(AssetPrototypeOf.DecoInvC);
            AssetDecoInvD = new AssetDrawable(AssetPrototypeOf.DecoInvD);
            AssetGoldCoin = new AssetDrawable(AssetPrototypeOf.GoldCoin);

            TextTopicItemName.Text = Loc.GetString("Elona.Inventory.Layer.Topic.ItemName"); 

            // TODO IInventoryBehavior can set this.
            var topicText = Loc.GetString("Elona.Inventory.Layer.Topic.ItemWeight");
            TextTopicItemDetail.Text = topicText;

            AddChild(Window);
            AddChild(List);

            OnKeyBindDown += HandleKeyBindDown;
            List.EventOnActivate += OnSelect;
            EventFilter = UIEventFilterMode.Pass;        
        }

        public override void Initialize(InventoryContext context)
        {
            Context = context;
            Window.Title = Context.Behavior.WindowTitle;
            CurrentIcon = Context.Behavior.MakeIcon();

            UpdateFiltering();
        }

        public override void OnFocused()
        {
            base.OnFocused();
            List.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
            else if (args.Function == ContentKeyFunctions.UIIdentify)
            {
                ShowItemDescription();
            }
            else
            {
                Context.Behavior.OnKeyBindDown(this, args);
            }
        }

        private void ShowItemDescription()
        {
            var selected = List.SelectedCell;

            if (selected == null)
                return;

            UserInterfaceManager.Query<ItemDescriptionLayer, EntityUid>(selected.Data.ItemEntityUid);
        }

        public void OnSelect(object? sender, UiListEventArgs<InventoryEntry> e)
        {
            var entry = e.SelectedCell.Data;
            var result = Context.OnSelect(entry.ItemEntityUid);

            switch (result)
            {
                case InventoryResult.Finished:
                    this.Finish(new(result, entry.ItemEntityUid));
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
                this.Finish(new(new InventoryResult.Finished(TurnResult.Succeeded), null));
            }

            Context.ShowInventoryWindow = true;
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
            List.AddRange(filtered.Select(e => new InventoryEntryCell(e, _spriteBatch)));

            List.SelectedIndex = index;

            var totalWeight = _invSys.GetTotalInventoryWeight(Context.User);
            var totalWeightStr = UiUtils.DisplayWeight(1500);

            var maxWeight = _invSys.GetMaxInventoryWeight(Context.User);
            var maxWeightStr = maxWeight != null ? UiUtils.DisplayWeight(maxWeight.Value) : "-";

            var cargoWeight = _cargoSys.GetTotalCargoWeight(Context.User);
            var cargoWeightStr = UiUtils.DisplayWeight(cargoWeight);

            var maxCargoWeight = _cargoSys.GetMaxCargoWeight(Context.User);
            var maxCargoWeightStr = maxCargoWeight != null ? UiUtils.DisplayWeight(maxCargoWeight.Value) : "-";

            if (Context.Behavior.ShowTotalWeight)
            {
                var weightText = Loc.GetString("Elona.Inventory.Layer.Note.TotalWeight", 
                    ("totalWeight", totalWeightStr),
                    ("maxWeight", maxWeightStr),
                    ("cargoWeight", cargoWeightStr),
                    ("maxCargoWeight", maxCargoWeightStr));
                TextNoteTotalWeight.Text = $"{List.Count} items  ({weightText})";
            }
            else
            {
                TextNoteTotalWeight.Text = string.Empty;
            }

            RedisplayList();
        }

        private void RedisplayList()
        {
            foreach (var cell in List.Cast<InventoryEntryCell>())
            {
                cell.RefreshFromItem(_entityManager);
            }
        }

        public void RefreshList(InventoryRefreshListKind kind)
        {
            switch (kind)
            {
                case InventoryRefreshListKind.RebuildList:
                    UpdateFiltering();
                    break;
                case InventoryRefreshListKind.Redisplay:
                    RedisplayList();
                    break;
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
            _spriteBatch.SetSize(0, 0);

            AssetDecoInvA.SetPreferredSize();
            AssetDecoInvB.SetPreferredSize();
            AssetDecoInvC.SetPreferredSize();
            AssetDecoInvD.SetPreferredSize();
            AssetGoldCoin.SetPreferredSize();

            CurrentIcon?.SetPreferredSize();

            TextTopicItemName.SetPreferredSize();
            TextTopicItemDetail.SetPreferredSize();
            TextNoteTotalWeight.SetPreferredSize();
            TextGoldCount.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            Window.SetPosition(X, Y);
            List.SetPosition(Window.X + 58, Window.Y + 60);
            _spriteBatch.SetPosition(0, 0);

            AssetDecoInvA.SetPosition(Window.X + Window.Width - 136, Y - 6);
            AssetDecoInvB.SetPosition(Window.X + Window.Width - 186, Y - 6);
            AssetDecoInvC.SetPosition(Window.X + Window.Width - 246, Y - 6);
            AssetDecoInvD.SetPosition(Window.X - 6, Window.Y - 6);
            AssetGoldCoin.SetPosition(Window.X + 340, Window.Y + 32);

            CurrentIcon?.SetPosition(Window.X + 46, Window.Y - 16);

            TextTopicItemName.SetPosition(Window.X + 28, Window.Y + 30);
            TextTopicItemDetail.SetPosition(Window.X + 526, Window.Y + 30);
            var notePos = UiUtils.NotePosition(GlobalPixelBounds, TextNoteTotalWeight);
            TextNoteTotalWeight.SetPosition(notePos.X, notePos.Y);
            TextGoldCount.SetPosition(Window.X + 368, Window.Y + 37);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            List.Update(dt);
            _spriteBatch.Update(dt);

            AssetDecoInvA.Update(dt);
            AssetDecoInvB.Update(dt);
            AssetDecoInvC.Update(dt);
            AssetDecoInvD.Update(dt);
            AssetGoldCoin.Update(dt);

            CurrentIcon?.Update(dt);

            TextTopicItemName.Update(dt);
            TextTopicItemDetail.Update(dt);
            TextNoteTotalWeight.Update(dt);
            TextGoldCount.Update(dt);
        }

        public override void Draw()
        {
            if (!Context.ShowInventoryWindow)
                return;

            Window.Draw();

            AssetDecoInvA.Draw();
            AssetDecoInvB.Draw();
            AssetDecoInvC.Draw();
            AssetDecoInvD.Draw();

            CurrentIcon?.Draw();

            TextTopicItemName.Draw();
            TextTopicItemDetail.Draw();
            TextNoteTotalWeight.Draw();

            // List will update the sprite batch.
            _spriteBatch.Clear();
            List.Draw();
            _spriteBatch.Draw();

            if (Context.Behavior.ShowMoney)
            {
                AssetGoldCoin.Draw();
                TextGoldCount.Draw();
            }
        }

        public override void Dispose()
        {
            Window.Dispose();

            AssetDecoInvA.Dispose();
            AssetDecoInvB.Dispose();
            AssetDecoInvC.Dispose();
            AssetDecoInvD.Dispose();

            CurrentIcon?.Dispose();

            TextTopicItemName.Dispose();
            TextTopicItemDetail.Dispose();
            TextNoteTotalWeight.Dispose();

            List.Dispose();
            _spriteBatch.Dispose();

            AssetGoldCoin.Dispose();
            TextGoldCount.Dispose();
        }
    }
}
