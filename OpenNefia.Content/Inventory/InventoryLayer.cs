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
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Inventory
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
                UiText.SetPosition(X + 40 + XOffset + 4, Y + 1);
                UiSubtext.SetPosition(X + Width - 24 - UiSubtext.TextWidth + XOffset + 4, Y + 2);
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

        [Dependency] private readonly IEntityManager _entityManager = default!;

        public bool PlaySounds { get; set; } = false;

        private UiWindow Window = new();
        private UiList<InventoryEntry> List = new();

        private IUiText TextTopicWindowName = new UiTextTopic("Inventory");
        private IUiText TextTopicDetailHeader = new UiTextTopic("Detail");
        private IUiText TextTotalWeight = new UiText(UiFonts.TextNote);
        private IUiText TextGoldCount = new UiText(UiFonts.InventoryGoldCount);

        private IAssetDrawable? CurrentIcon;

        private IAssetDrawable AssetDecoInvA;
        private IAssetDrawable AssetDecoInvB;
        private IAssetDrawable AssetDecoInvC;
        private IAssetDrawable AssetDecoInvD;
        private IAssetDrawable AssetGoldCoin;

        // private EntitySpriteBatch _spriteBatch = new();
        private InventoryContext Context;

        public InventoryLayer(InventoryContext context)
        {
            EntitySystem.InjectDependencies(this);

            Context = context;

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
            Keybinds[CoreKeybinds.Cancel] += (_) => Cancel();
            Keybinds[CoreKeybinds.Escape] += (_) => Cancel();

            Forwards += List;

            List.EventOnActivate += (o, e) =>
            {
                Cancel();
            };
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

            foreach (var source in Context.GetSources())
            {
                foreach (var ent in source.GetEntities())
                {
                    if (Context.IsAccepted(ent))
                    {
                        entries.Add(ToEntry(ent, source));
                    }
                }
            }

            return entries;
        }

        private void UpdateFiltering()
        {
            var filtered = GetFilteredEntries();

            List.Clear();
            List.AddRange(filtered.Select(e => new InventoryEntryCell(e)));

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

            AssetDecoInvA.SetPosition(X + Width - 136, Y - 6);
            AssetDecoInvB.SetPosition(X + Width - 186, Y - 6);
            AssetDecoInvC.SetPosition(X + Width - 246, Y - 6);
            AssetDecoInvD.SetPosition(X - 6, Y - 6);
            AssetGoldCoin.SetPosition(X + 340, Y + 32);

            TextTopicWindowName.SetPosition(X + 28, Y + 30);
            TextTopicDetailHeader.SetPosition(X + 526, Y + 30);
            var notePos = UiUtils.NotePosition(Bounds, TextTotalWeight);
            TextTotalWeight.SetPosition(notePos.X, notePos.Y);
            TextGoldCount.SetPosition(X + 368, Y + 37);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            List.Update(dt);

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

            List.Draw();

            if (Context.Behavior.ShowMoney)
            {
                AssetGoldCoin.Draw();
                TextGoldCount.Draw();
            }
        }
    }
}
