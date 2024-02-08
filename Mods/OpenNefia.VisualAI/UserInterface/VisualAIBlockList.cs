using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
using OpenNefia.VisualAI.Engine;
using OpenNefia.Core.Utility;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Dialog;
using static OpenNefia.Content.Prototypes.Protos;
using NuGet.ProjectModel;
using OpenNefia.Content.ConfigMenu.UICell;
using OpenNefia.Content.ConfigMenu;
using OpenNefia.Core.Configuration;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIBlockList : UiList<VisualAIBlockList.Item>
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public delegate void CategoryChangedDelegate(VisualAIBlockType blockType);
        public event CategoryChangedDelegate? OnCategoryChanged;

        public sealed class Item
        {
            public Item(PrototypeId<VisualAIBlockPrototype> blockID, string text, Core.Maths.Color color, PrototypeId<AssetPrototype>? icon)
            {
                BlockID = blockID;
                Text = text;
                Color = color;
                Icon = icon;
            }

            public PrototypeId<VisualAIBlockPrototype> BlockID { get; }
            public string Text { get; }
            public Core.Maths.Color Color { get; }
            public PrototypeId<AssetPrototype>? Icon { get; }
        }

        public sealed class ListCell : UiListCell<Item>
        {
            [Child] private VisualAIBlockCard Card { get; }

            public PrototypeId<VisualAIBlockPrototype> BlockID { get; }

            public bool IsSelected { get => Card.IsSelected; set => Card.IsSelected = value; }

            public ListCell(Item data, UiListChoiceKey? key = null) : base(data, new UiText(UiFonts.ListText), key)
            {
                Card = new VisualAIBlockCard(data.Text, data.Color, data.Icon);
                IsSelected = false;
                BlockID = data.BlockID;
            }

            public override void SetPosition(float x, float y)
            {
                base.SetPosition(x, y);
                Card.SetPosition(X, Y);
            }

            public override void SetSize(float width, float height)
            {
                base.SetSize(width, height);
                Card.SetSize(Width, Height);
            }

            public override void Update(float dt)
            {
                Card.Update(dt);
            }

            public override void Draw()
            {
                Card.Draw();
            }
        }

        private static Dictionary<VisualAIBlockType, List<Item>> _itemsByCategory = new();

        private VisualAIBlockType[] _categories = new VisualAIBlockType[]
        {
            VisualAIBlockType.Target,
            VisualAIBlockType.Condition,
            VisualAIBlockType.Action,
            VisualAIBlockType.Special
        };
        private VisualAIBlockType _selectedCategory = VisualAIBlockType.Target;
        public VisualAIBlockType SelectedCategory => _selectedCategory;

        private float _offsetY = 0;

        public VisualAIBlockList()
        {
            // TODO
            EntitySystem.InjectDependencies(this);

            AutoCalculateHeight = false;
            ItemHeight = 80;

            InitListItems();
            SetCategory(_selectedCategory);
        }

        protected override void HandleSelect(UiListEventArgs<Item> e)
        {
            base.HandleSelect(e);
            RecalcLayout();
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var hints = base.MakeKeyHints();
            hints.Add(new(UiKeyHints.Page, new[] { EngineKeyFunctions.UIPreviousPage, EngineKeyFunctions.UINextPage }));
            return hints;
        }

        private void InitListItems()
        {
            if (_itemsByCategory.Count > 0)
                return;

            Item ToListItem(VisualAIBlockPrototype proto)
            {
                var text = VisualAIHelpers.FormatBlockDescription(proto);
                return new Item(proto.GetStrongID(), text, proto.Color, proto.Icon);
            }

            _itemsByCategory = _protos.EnumeratePrototypes<VisualAIBlockPrototype>()
                .GroupBy(p => p.Type)
                .ToDictionary(group => group.Key, group => group.Select(ToListItem).ToList());

            SetCategory(_selectedCategory);
        }

        private void RecalcLayout()
        {
            _offsetY = 0;
            var selectedY = SelectedIndex * ItemHeight + 10;

            var thresholdY = (selectedY + ItemHeight * 1.75f);

            if (thresholdY > Height)
            {
                _offsetY = float.Max(Height - thresholdY,
                                    (Count - (Height / ItemHeight)) * -ItemHeight - 54);
            }

            var pos = Position + (10, 18 + _offsetY);

            foreach (var cell in DisplayedCells)
            {
                cell.SetPosition(pos.X, pos.Y);
                cell.SetSize(Width - 40, ItemHeight);
                (cell as ListCell)!.IsSelected = SelectedCell == cell;

                pos += (0, ItemHeight);
            }
        }

        protected override void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.HandleKeyBindDown(args);

            if (args.Function == EngineKeyFunctions.UIPreviousPage)
            {
                _audio.Play(Protos.Sound.Pop1);
                ChangeCategory(-1);
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UINextPage)
            {
                _audio.Play(Protos.Sound.Pop1);
                ChangeCategory(1);
                args.Handle();
            }
        }

        public void ChangeCategory(int delta)
        {
            var index = Array.IndexOf(_categories, _selectedCategory);
            DebugTools.Assert(index != -1);
            var newIndex = MathHelper.Wrap(index + delta, 0, _categories.Length - 1);
            SetCategory(_categories[newIndex]);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            RecalcLayout();
        }

        public void SetCategory(VisualAIBlockType category)
        {
            if (!_itemsByCategory.TryGetValue(category, out var items))
                return;

            _selectedCategory = category;

            var cells = items.Select(i => new ListCell(i));
            SetCells(cells);
            RecalcLayout();

            OnCategoryChanged?.Invoke(category);
        }

        public void SelectBlock(PrototypeId<VisualAIBlockPrototype> blockID)
        {
            var blockProto = _protos.Index(blockID);
            SetCategory(blockProto.Type);
            var cell = DisplayedCells.FirstOrDefault(c => c.Data.BlockID == blockID);
            if (cell != null)
            {
                Select(cell.IndexInList);
            }
        }

        public override void Draw()
        {
            GraphicsS.SetScissorS(UIScale, X, Y + 10 + 8, Width, Height - 28 - 16);

            for (var i = 0; i < DisplayedCells.Count; i++)
            {
                DisplayedCells[i].Draw();
            }

            Love.Graphics.SetScissor();
        }
    }
}
