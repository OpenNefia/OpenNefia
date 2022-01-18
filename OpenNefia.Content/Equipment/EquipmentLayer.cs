using NetVips;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Input;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Equipment
{
    [Localize("Elona.Equipment.Layer")]
    public class EquipmentLayer : UiLayerWithResult<EquipmentLayer.Args, EquipmentLayer.Result>
    {
        public class Args
        {
            /// <summary>
            /// Character doing the equipping. **Not** necessarily the same as the <see cref="EquipTarget"/>!
            /// </summary>
            public EntityUid Equipee { get; set; }

            /// <summary>
            /// Character being equipped.
            /// </summary>
            public EntityUid EquipTarget { get; set; }

            public Args(EntityUid equipper, EntityUid equipTarget)
            {
                Equipee = equipper;
                EquipTarget = equipTarget;
            }

            public Args(EntityUid equipper) : this(equipper, equipper)
            {
            }
        }

        public new class Result
        {
            /// <summary>
            /// If true, the equip target changed equipment. This means the equipee's
            /// turn should be passed when the menu is exited.
            /// </summary>
            public bool ChangedEquipment { get; set; }

            public Result(bool changedEquipment)
            {
                ChangedEquipment = changedEquipment;
            }
        }

        public class CellData
        {
            private const string DefaultEquipSlotIcon = "0";

            public EquipSlotInstance EquipSlot { get; set; }
            public string EquipSlotText { get; set; } = string.Empty;
            public IUiElement? EquipSlotIcon { get; set; }
            public EntityUid? ItemEntityUid { get; set; }
            public Color ItemTextColor { get; set; }
            public string ItemNameText { get; set; } = string.Empty;
            public string ItemSubnameText { get; set; } = string.Empty;

            public CellData(EquipSlotInstance equipSlot)
            {
                EquipSlot = equipSlot;
            }
        }

        public class ListCell : UiListCell<CellData>
        {
            private IUiElement? Icon;
            private readonly IUiText TextEquipSlotName = new UiText(UiFonts.EquipmentEquipSlotName);
            private readonly IUiText TextSubtext = new UiText();

            private readonly EntitySpriteBatch SpriteBatch;

            public ListCell(CellData data, EntitySpriteBatch spriteBatch) 
                : base(data, new UiText())
            {
                SpriteBatch = spriteBatch;

                OnCellDataChanged();
            }

            protected override void OnCellDataChanged()
            {
                UiText.Text = Data.ItemNameText;
                UiText.Color = Data.ItemTextColor;
                TextSubtext.Text = Data.ItemSubnameText;
                TextEquipSlotName.Text = Data.EquipSlotText;

                Icon = Data.EquipSlotIcon;
            }

            public override void SetPosition(float x, float y)
            {
                XOffset = 30f;
                base.SetPosition(x, y);
                Icon?.SetPosition(X - 66, Y - 2);
                TextEquipSlotName.SetPosition(X - 42, Y + 3);
                TextSubtext.SetPosition(X + Width - 44 - TextSubtext.TextPixelWidth, Y + 2);
            }

            public override void SetSize(float width, float height)
            {
                base.SetSize(width, height);
                Icon?.SetPreferredSize();
                TextEquipSlotName.SetPreferredSize();
                TextSubtext.SetPreferredSize();
            }

            public override void Update(float dt)
            {
                Icon?.Update(dt);
                TextEquipSlotName.Update(dt);
                TextSubtext.Update(dt);
            }

            public override void Draw()
            {
                if (IndexInList % 2 == 0)
                {
                    Love.Graphics.SetColor(UiColors.ListEntryAccent);
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X - 1, Y, Width - 30, 18);
                }

                Love.Graphics.SetColor(Color.White);
                AssetSelectKey.Draw(X, Y - 1);
                KeyNameText.Draw();

                Icon?.Draw();
                TextEquipSlotName.Draw();

                TextEquipSlotName.Draw();
                UiText.Draw();
                TextSubtext.Draw();

                if (Data.ItemEntityUid != null)
                    SpriteBatch.Add(Data.ItemEntityUid.Value, PixelX + 12 + 28, PixelY + 10, centered: true);
            }

            public override void Dispose()
            {
                base.Dispose();

                Icon?.Dispose();
                TextEquipSlotName.Dispose();
                TextSubtext.Dispose();
            }
        }

        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly DisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IStackSystem _stackSystem = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        protected IAssetDrawable AssetInventoryIcons;
        protected IAssetDrawable AssetDecoWearA;
        protected IAssetDrawable AssetDecoWearB;

        /// <summary>
        /// "Category/Name"
        /// </summary>
        [Localize("Topic.CategoryName")]
        protected IUiText TextTopicCategoryName = new UiTextTopic();

        /// <summary>
        /// "Weight"
        /// </summary>
        [Localize("Topic.Weight")]
        protected IUiText TextTopicWeight = new UiTextTopic();

        /// <summary>
        /// Total equipment weight, armor class, hit/damage bonuses.
        /// </summary>
        protected IUiText TextNoteEquipStats = new UiText(UiFonts.TextNote);

        [Localize] protected UiWindow Window = new(keyHintXOffset: 64);
        protected UiPagedList<CellData> List = new(itemsPerPage: 14);

        private EntitySpriteBatch _spriteBatch = new();

        private EntityUid _equipee;
        private EntityUid _equipTarget;

        private bool _changedEquipment = false;

        public EquipmentLayer()
        {
            AssetInventoryIcons = InventoryHelpers.MakeIcon(InventoryIcon.Equip);
            AssetDecoWearA = new AssetDrawable(Protos.Asset.DecoWearA);
            AssetDecoWearB = new AssetDrawable(Protos.Asset.DecoWearB);

            OnKeyBindDown += HandleKeyBindDown;
            EventFilter = UIEventFilterMode.Stop;
            CanControlFocus = true;

            List.PageTextElement = Window;
            List.OnActivated += HandleListOnActivate;

            AddChild(Window);
            AddChild(List);
        }

        public override void Initialize(Args args)
        {
            _equipee = args.Equipee;
            _equipTarget = args.EquipTarget;

            UpdateFromEquipTarget();
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
                // Need to finish instead of cancel in case equipment was changed.
                Finish(new Result(_changedEquipment));
            }
            else if (args.Function == ContentKeyFunctions.UIIdentify)
            {
                if (List.SelectedCell != null)
                {
                    var item = List.SelectedCell.Data.ItemEntityUid;

                    if (_entityManager.IsAlive(item))
                        UserInterfaceManager.Query<ItemDescriptionLayer, EntityUid>(item.Value);
                }
            }
            else if (args.Function == ContentKeyFunctions.UIMode)
            {
                // TODO resistance/detail views
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.KnownInfo, ContentKeyFunctions.UIIdentify));
            keyHints.Add(new(UiKeyHints.Mode, ContentKeyFunctions.UIMode));
            keyHints.AddRange(List.MakeKeyHints());
            keyHints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));

            return keyHints;
        }

        private void HandleListOnActivate(object? sender, UiListEventArgs<CellData> e)
        {
            var cellData = e.SelectedCell.Data;

            if (cellData.ItemEntityUid != null)
            {
                DoUnequip(cellData.EquipSlot);
            }
            else
            {
                DoEquip(cellData.EquipSlot);
            }
        }

        private void DoUnequip(EquipSlotInstance equipSlot)
        {
            // Move the unequipped item to the target's inventory, if possible.
            IContainer? container = null;
            if (_entityManager.TryGetComponent(_equipTarget, out InventoryComponent inventory))
                container = inventory.Container;

            if (_equipSlots.TryUnequip(_equipee, _equipTarget, equipSlot, out var unequippedItem, placeInto: container, silent: false))
            {
                Sounds.Play(Sound.Equip1);
                UpdateFromEquipTarget();
            }
            else
            {
                Sounds.Play(Sound.Fail1);
            }
        }

        private void DoEquip(EquipSlotInstance equipSlot)
        {
            var equipBehavior = new EquipInventoryBehavior(equipSlot.ID);
            var invContext = new InventoryContext(_equipTarget, equipBehavior);
            var result = UserInterfaceManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(invContext);

            if (result.HasValue && _entityManager.IsAlive(result.Value.SelectedItem))
            {
                var item = result.Value.SelectedItem.Value;

                if (!_stackSystem.TrySplit(item, 1, out var splitItem))
                    return;

                if (!_equipSlots.TryEquip(_equipee, _equipTarget, splitItem, equipSlot, silent: false))
                {
                    Sounds.Play(Sound.Fail1);
                    return;
                }

                Sounds.Play(Sound.Equip1);
                _changedEquipment = true;

                // Display messages relating to curse state, weapon suitability, etc.
                var ev = new GotEquippedInMenuEvent(_equipee, _equipTarget, equipSlot);
                _entityManager.EventBus.RaiseLocalEvent(splitItem, ev);

                UpdateFromEquipTarget();
            }
        }

        private void UpdateFromEquipTarget()
        {
            var listData = BuildListData(_equipTarget);

            List.Clear();
            List.AddRange(listData.Select(d => new ListCell(d, _spriteBatch)));

            TextNoteEquipStats.Text = MakeEquipStatsText(_equipTarget);
            Window.KeyHints = MakeKeyHints();
        }

        private IEnumerable<CellData> BuildListData(EntityUid equipTarget)
        {
            if (!_equipSlots.TryGetEquipSlots(equipTarget, out var equipSlots))
                yield break;

            foreach (var equipSlot in equipSlots)
            {
                var equipSlotProto = _protos.Index(equipSlot.ID);

                var cellData = new CellData(equipSlot)
                {
                    EquipSlotText = Loc.GetPrototypeString(equipSlot.ID, "Name"),
                    EquipSlotIcon = EquipmentHelpers.MakeEquipSlotIcon(equipSlotProto.Icon),
                    ItemEntityUid = null,
                    ItemTextColor = UiColors.EquipmentItemTextDefault,
                    ItemNameText = "-    ",
                    ItemSubnameText = "-"
                };

                if (_equipSlots.TryGetContainerForEquipSlot(equipTarget, equipSlot, out var containerSlot))
                {
                    if (_entityManager.IsAlive(containerSlot.ContainedEntity))
                    {
                        var item = containerSlot.ContainedEntity.Value;

                        cellData.ItemEntityUid = item;
                        cellData.ItemTextColor = InventoryHelpers.GetItemTextColor(item, _entityManager);
                        cellData.ItemNameText = _displayNames.GetDisplayName(item);

                        var weight = 0;
                        if (_entityManager.TryGetComponent(item, out WeightComponent weightComp))
                            weight = weightComp.Weight;

                        cellData.ItemSubnameText = UiUtils.DisplayWeight(weight);
                    }
                    else if (containerSlot.ContainedEntity.HasValue)
                    {
                        Logger.WarningS("equipment", $"Removing dead equipment entity {containerSlot.ContainedEntity}");
                        containerSlot.ForceRemove(containerSlot.ContainedEntity.Value);
                    }
                }

                yield return cellData;
            }
        }

        private string MakeEquipStatsText(EntityUid equipTarget)
        {
            var dv = "-";
            var pv = "-";
            var hitBonus = "-";
            var damageBonus = "-";

            if (_entityManager.TryGetComponent(equipTarget, out SkillsComponent skills))
            {
                dv = skills.DV.Buffed.ToString();
                pv = skills.PV.Buffed.ToString();
                hitBonus = skills.HitBonus.Buffed.ToString();
                damageBonus = skills.DamageBonus.Buffed.ToString();
            }

            var weight = EquipmentHelpers.GetTotalEquipmentWeight(equipTarget, _entityManager, _equipSlots);

            return string.Format("{0}: {1}{2} {3}:{4} {5}:{6}  DV/PV:{7}/{8}",
                Loc.GetString("Elona.Equipment.Layer.Stats.EquipWeight"),
                UiUtils.DisplayWeight(weight),
                EquipmentHelpers.DisplayArmorClass(weight),
                Loc.GetString("Elona.Equipment.Layer.Stats.HitBonus"),
                hitBonus,
                Loc.GetString("Elona.Equipment.Layer.Stats.DamageBonus"),
                damageBonus,
                dv,
                pv);
        }

        public override void OnQuery()
        {
            Sounds.Play(Sound.Wear);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(690, 428, out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            List.SetSize(Window.Width - 96, Window.Height - 60);
            _spriteBatch.SetSize(0, 0);

            TextTopicCategoryName.SetPreferredSize();
            TextTopicWeight.SetPreferredSize();
            AssetInventoryIcons.SetPreferredSize();
            AssetDecoWearA.SetPreferredSize();
            AssetDecoWearB.SetPreferredSize();
            TextNoteEquipStats.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            List.SetPosition(Window.X + 88, Window.Y + 60);
            _spriteBatch.SetPosition(0, 0);

            TextTopicCategoryName.SetPosition(Window.X + 28, Window.Y + 30);
            TextTopicWeight.SetPosition(Window.X + Window.Width - 160, Window.Y + 30);
            AssetInventoryIcons.SetPosition(Window.X + 46, Window.Y - 16);
            AssetDecoWearA.SetPosition(Window.X + Window.Width - 106, Window.Y);
            AssetDecoWearB.SetPosition(Window.X, Window.Y + Window.Height - 164);
            var notePos = UiUtils.NotePosition(Rect, TextNoteEquipStats);
            TextNoteEquipStats.SetPosition(notePos.X, notePos.Y);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);

            TextTopicCategoryName.Update(dt);
            TextTopicWeight.Update(dt);
            AssetDecoWearA.Update(dt);
            AssetDecoWearB.Update(dt);
            TextNoteEquipStats.Update(dt);

            _spriteBatch.Update(dt);
            List.Update(dt);
        }

        public override void Draw()
        {
            Window.Draw();

            TextTopicCategoryName.Draw();
            TextTopicWeight.Draw();
            AssetInventoryIcons.Draw();
            AssetDecoWearA.Draw();
            AssetDecoWearB.Draw();
            TextNoteEquipStats.Draw();

            // List will update the sprite batch.
            _spriteBatch.Clear();
            List.Draw();
            _spriteBatch.Draw();
        }

        public override void Dispose()
        {
            Window.Dispose();

            TextTopicCategoryName.Dispose();
            TextTopicWeight.Dispose();
            AssetDecoWearA.Dispose();
            AssetDecoWearA.Dispose();
            TextNoteEquipStats.Dispose();

            _spriteBatch.Dispose();
            List.Dispose();
        }
    }

    public class GotEquippedInMenuEvent : EntityEventArgs
    {
        public EntityUid Equipee { get; }
        public EntityUid EquipTarget { get; }
        public EquipSlotInstance EquipSlot { get; }

        public GotEquippedInMenuEvent(EntityUid equipee, EntityUid equipTarget, EquipSlotInstance equipSlot)
        {
            Equipee = equipee;
            EquipTarget = equipTarget;
            EquipSlot = equipSlot;
        }
    }
}
