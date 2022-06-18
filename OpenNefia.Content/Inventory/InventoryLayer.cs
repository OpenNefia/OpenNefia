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
using static OpenNefia.Content.Equipment.EquipmentLayer;

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

    public sealed class InventoryLayer : GroupableUiLayer<InventoryContext, InventoryLayer.Result>, IInventoryLayer
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
            [Child] private readonly UiText UiSubtext;
            [Child] private readonly EntitySpriteBatch SpriteBatch;

            public InventoryEntryCell(InventoryEntry entry, EntitySpriteBatch spriteBatch)
                : base(entry, new UiText())
            {
                SpriteBatch = spriteBatch;

                UiSubtext = new UiText();

                UiText.Text = entry.ItemNameText;
                UiSubtext.Text = entry.ItemDetailText;
            }

            public override void SetSize(float width, float height)
            {
                UiText.SetSize(width, height);
                UiSubtext.SetSize(width, height);
                base.SetSize(Math.Max(width, UiText.Width), height);
            }

            public override void SetPosition(float x, float y)
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
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X - 1, Y, Width - 30, 18);
                }

                Love.Graphics.SetColor(Color.White);
                AssetSelectKey.Draw(UIScale, X, Y - 1);
                KeyNameText.Draw();

                Data.Origin.OnDraw();

                UiText.Draw();
                UiSubtext.Draw();

                SpriteBatch.Add(Data.ItemEntityUid, X - 40, Y - 12, color: Data.ChipColor);
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
        [Dependency] private readonly IMessagesManager _mes = default!;

        public bool PlaySounds { get; set; } = false;

        public int SelectedIndex => List.SelectedIndex;
        public InventoryEntry? SelectedEntry => List.SelectedCell?.Data;

        [Child] private UiWindow Window = new();
        [Child] private UiPagedList<InventoryEntry> List = new(itemsPerPage: 16);

        [Child] private UiText TextTopicItemName = new UiTextTopic();
        [Child] private UiText TextTopicItemDetail = new UiTextTopic();
        [Child] private UiText TextNoteTotalWeight = new UiText(UiFonts.TextNote);
        [Child] private UiText TextGoldCount = new UiText(UiFonts.InventoryGoldCount);

        [Child] private AssetDrawable AssetDecoInvA;
        [Child] private AssetDrawable AssetDecoInvB;
        [Child] private AssetDrawable AssetDecoInvC;
        [Child] private AssetDrawable AssetDecoInvD;
        [Child] private AssetDrawable AssetGoldCoin;

        [Child] private EntitySpriteBatch _itemSpriteBatch = new();

        private UiElement? CurrentIcon;

        public InventoryContext Context { get; private set; } = default!;

        public InventoryLayer()
        {
            AssetDecoInvA = new AssetDrawable(Protos.Asset.DecoInvA);
            AssetDecoInvB = new AssetDrawable(Protos.Asset.DecoInvB);
            AssetDecoInvC = new AssetDrawable(Protos.Asset.DecoInvC);
            AssetDecoInvD = new AssetDrawable(Protos.Asset.DecoInvD);
            AssetGoldCoin = new AssetDrawable(Protos.Asset.GoldCoin);

            TextTopicItemName.Text = Loc.GetString("Elona.Inventory.Layer.Topic.ItemName");

            // TODO IInventoryBehavior can set this.
            var topicText = Loc.GetString("Elona.Inventory.Layer.Topic.ItemWeight");
            TextTopicItemDetail.Text = topicText;

            List.PageTextElement = Window;

            OnKeyBindDown += HandleKeyBindDown;
            List.OnActivated += OnSelect;
            EventFilter = UIEventFilterMode.Pass;
        }

        public override void Initialize(InventoryContext context)
        {
            Context = context;
            Window.Title = Context.Behavior.WindowTitle;
            CurrentIcon = Context.Behavior.MakeIcon();
            if (CurrentIcon != null)
                AddChild(CurrentIcon);

            UpdateFiltering();
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
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

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.KnownInfo, ContentKeyFunctions.UIIdentify));
            keyHints.Add(new(UiKeyHints.Mode, ContentKeyFunctions.UIMode));
            keyHints.AddRange(List.MakeKeyHints());
            keyHints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));

            if (Context.Behavior.EnableShortcuts)
            {
                keyHints.Add(new(UiKeyHints.Shortcut, UiKeyNames.Shortcut));
            }

            keyHints.AddRange(Context.Behavior.MakeKeyHints());

            return keyHints;
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
                _mes.Newline();
                _mes.Display(text);
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
                var itemName = Loc.GetString("Elona.Common.NameWithDirectArticle", ("entity", ent));
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

            List.SetCells(filtered.Select(e => new InventoryEntryCell(e, _itemSpriteBatch)));

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
            Window.KeyHints = MakeKeyHints();
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

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(640, 432, out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);

            Window.SetSize(Width, Height);
            List.SetSize(Width - 58, Height - 60);
            _itemSpriteBatch.SetSize(0, 0);

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

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);

            Window.SetPosition(X, Y);
            List.SetPosition(Window.X + 58, Window.Y + 60);
            _itemSpriteBatch.SetPosition(0, 0);

            AssetDecoInvA.SetPosition(Window.X + Window.Width - 136, Y - 6);
            AssetDecoInvB.SetPosition(Window.X + Window.Width - 186, Y - 6);
            AssetDecoInvC.SetPosition(Window.X + Window.Width - 246, Y - 6);
            AssetDecoInvD.SetPosition(Window.X - 6, Window.Y - 6);
            AssetGoldCoin.SetPosition(Window.X + 340, Window.Y + 32);

            CurrentIcon?.SetPosition(Window.X + 46, Window.Y - 16);

            TextTopicItemName.SetPosition(Window.X + 28, Window.Y + 30);
            TextTopicItemDetail.SetPosition(Window.X + 526, Window.Y + 30);
            var notePos = UiUtils.NotePosition(Rect, TextNoteTotalWeight);
            TextNoteTotalWeight.SetPosition(notePos.X, notePos.Y);
            TextGoldCount.SetPosition(Window.X + 368, Window.Y + 37);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            List.Update(dt);
            _itemSpriteBatch.Update(dt);

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
            _itemSpriteBatch.Clear();
            List.Draw();
            _itemSpriteBatch.Draw();

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
            _itemSpriteBatch.Dispose();

            AssetGoldCoin.Dispose();
            TextGoldCount.Dispose();
        }
    }
}
