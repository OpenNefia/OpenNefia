﻿using OpenNefia.Content.UI.Element;
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
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using OpenNefia.Content.Input;
using OpenNefia.Core.Log;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Pickable;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Cargo;
using static OpenNefia.Content.Equipment.EquipmentLayer;
using OpenNefia.Content.Weight;
using OpenNefia.Content.Currency;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Prototypes;

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
            Source = origin;
            ItemNameText = itemNameText;
            ItemDetailText = itemDetailText;
            ChipColor = chipColor;
        }

        public EntityUid ItemEntityUid { get; set; }
        public IInventorySource Source { get; set; }
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
                UiSubtext.SetPosition(X + Width - 44 - UiSubtext.TextWidth + XOffset + 4, Y);
            }

            public override void Draw()
            {
                base.Draw();
                base.DrawLineTint(Width - 30);
                UiSubtext.Draw();

                Data.Source.OnDraw(UIScale, X, Y);

                SpriteBatch.Add(Data.ItemEntityUid, X - 40, Y - 12, centering: BatchCentering.AlignBottom, color: Data.ChipColor);
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
        }

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IInventorySystem _invSys = default!;
        [Dependency] private readonly ICargoSystem _cargoSys = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public bool PlaySounds { get; set; } = false;

        public int SelectedIndex => List.SelectedIndex;
        public InventoryEntry? SelectedEntry => List.SelectedCell?.Data;

        [Child] private UiWindow Window = new();
        [Child] private UiPagedList<InventoryEntry> List = new(itemsPerPage: 16, textOffset: new Vector2(0, 3f));

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

        private static readonly Dictionary<Type, int> PreviousSelectedItemIndices = new();

        public override void Initialize(InventoryContext context)
        {
            Context = context;
            Window.Title = Context.Behavior.WindowTitle;
            CurrentIcon = Context.Behavior.MakeIcon();
            if (CurrentIcon != null)
                AddChild(CurrentIcon);

            UpdateFiltering();

            var behaviorType = context.Behavior.GetType();
            if (context.Behavior.RestorePreviousListIndex && PreviousSelectedItemIndices.TryGetValue(behaviorType, out var index))
                List.Select(index);
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
                var behaviorType = Context.Behavior.GetType();
                PreviousSelectedItemIndices[behaviorType] = List.SelectedIndex;
                Cancel();
            }
            else if (args.Function == ContentKeyFunctions.UIIdentify)
            {
                ShowItemDescription();
            }
            else
            {
                var behaviorType = Context.Behavior.GetType();
                PreviousSelectedItemIndices[behaviorType] = List.SelectedIndex;

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

            var entities = List.Where(c => _entityManager.IsAlive(c.Data.ItemEntityUid))
                .Select(c => c.Data.ItemEntityUid)
                .ToList();

            var result = UserInterfaceManager.Query<ItemDescriptionLayer, ItemDescriptionLayer.Args, ItemDescriptionLayer.Result>(new(selected.Data.ItemEntityUid, entities));

            if (result.HasValue)
            {
                List.SelectAcrossAllPages(result.Value.SelectedIndexOnExit);
            }
        }

        public void OnSelect(object? sender, UiListEventArgs<InventoryEntry> e)
        {
            var entry = e.SelectedCell.Data;
            var result = Context.OnSelect(entry.ItemEntityUid);

            switch (result)
            {
                case InventoryResult.Finished:
                    var behaviorType = Context.Behavior.GetType();
                    PreviousSelectedItemIndices[behaviorType] = List.SelectedIndex;

                    this.Finish(new(result, entry.ItemEntityUid));
                    break;
                case InventoryResult.Continuing continuing:
                    if (continuing.SelectedItem != null)
                    {
                        var index = List.FindIndex(c => c.Data.ItemEntityUid == continuing.SelectedItem.Value);
                        if (index != -1)
                            List.SelectAcrossAllPages(index);
                    }
                    break;
                default:
                    break;
            }

            // TODO smarter list rebuilding
            UpdateFiltering();
            _field.RefreshScreen();
            
            // TODO merge with AfterFilter?
            if (List.Count == 0 && Context.Behavior.TurnResultAfterSelectionIfEmpty != null && !HasResult)
            {
                Finish(new(new InventoryResult.Finished(Context.Behavior.TurnResultAfterSelectionIfEmpty.Value), null));
            }

            Context.ShowInventoryWindow = true;
        }

        public override void OnQuery()
        {
            // Filtering before the layer is queried can pre-emptively set the result before any
            // interaction takes place. (see the call to Context.Behavior.AfterFilter())
            if (HasResult)
                return;

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

        private IEnumerable<InventoryEntry> GetFilteredEntries()
        {
            InventoryEntry ToEntry(EntityUid item, IInventorySource source)
            {
                var itemName = Context.Behavior.GetItemName(Context, item);

                if (Context.Behavior.ApplyNameModifiers)
                    source.ModifyEntityName(ref itemName);

                var itemDetail = Context.Behavior.GetItemDetails(Context, item);
                var chipColor = Color.White;
                if (_entityManager.TryGetComponent(item, out ChipComponent? chip))
                    chipColor = chip.Color;

                return new InventoryEntry(item, source, itemName, itemDetail, chipColor);
            };

            var sourceIndices = new Dictionary<IInventorySource, int>();
            var sourceIndex = 0;
            var entries = new List<InventoryEntry>();

            foreach (var (ent, source) in Context.GetFilteredEntities())
            {
                entries.Add(ToEntry(ent, source));

                if (!sourceIndices.ContainsKey(source))
                {
                    sourceIndices[source] = sourceIndex;
                    sourceIndex++;
                }
            }

            // TODO sort by category
            var comparer = _protos.GetComparator<EntityPrototype>();
            entries.Sort((a, b) =>
            {
                if (a.Source != b.Source)
                {
                    var aIndex = sourceIndices[a.Source];
                    var bIndex = sourceIndices[b.Source];
                    return aIndex.CompareTo(bIndex);
                }

                var protoA = _entityManager.GetComponent<MetaDataComponent>(a.ItemEntityUid).EntityPrototype;
                var protoB = _entityManager.GetComponent<MetaDataComponent>(b.ItemEntityUid).EntityPrototype;

                return comparer.Compare(protoA, protoB);
            });

            return entries;
        }

        private void UpdateFiltering()
        {
            var filtered = GetFilteredEntries();

            var index = List.SelectedIndex;

            List.SetCells(filtered.Select(e => new InventoryEntryCell(e, _itemSpriteBatch)));

            List.SelectedIndex = index;

            TextNoteTotalWeight.Text = Context.Behavior.GetTotalWeightDetails(Context);

            if (Context.Behavior.ShowMoney && _entityManager.TryGetComponent<MoneyComponent>(Context.Target, out var money))
            {
                TextGoldCount.Text = $"{money.Gold} gp";
            }

            Context.AllInventoryEntries = filtered.ToList();

            var filteredItems = List.Select(i => i.Data.ItemEntityUid);
            var afterFilterResult = Context.Behavior.AfterFilter(Context, Context.AllInventoryEntries);
            if (afterFilterResult is not InventoryResult.Continuing)
            {
                Finish(new(afterFilterResult, null));
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
            _itemSpriteBatch.SetSize(Width, Height);

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
    }
}
