using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Content.UI;
using OpenNefia.Content.Input;
using System.Collections.Generic;
using OpenNefia.Content.Items;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Inventory
{
    public class ItemDescriptionLayer : UiLayerWithResult<ItemDescriptionLayer.Args, ItemDescriptionLayer.Result>
    {
        [Dependency] private readonly ItemDescriptionSystem _itemDescSystem = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public class Args
        {
            public Args() { }

            public Args(int selectedIndex, IReadOnlyList<EntityUid> entities)
            {
                Entities = entities;
                SelectedIndex = selectedIndex;
            }

            public Args(EntityUid ent, IReadOnlyList<EntityUid> entities)
            {
                Entities = entities;
                SelectedIndex = entities.IndexOf(ent);
            }

            public Args(EntityUid ent)
            {
                Entities = new List<EntityUid>() { ent };
                SelectedIndex = 0;
            }

            public IReadOnlyList<EntityUid> Entities { get; set; } = new List<EntityUid>();
            public int SelectedIndex { get; set; } = 0;
            public EntityUid? SelectedEntity => Entities.ElementAtOrDefault(SelectedIndex);
        }

        public class Result
        {
            public int SelectedIndexOnExit { get; set; }
        }

        private IReadOnlyList<EntityUid> _otherItems = default!;
        private int _currentIndex;
        private EntityUid _currentItem => _otherItems[_currentIndex];

        private IAssetInstance AssetEnchantmentIcons;
        private IAssetInstance AssetInheritanceIcon;

        [Child] private UiText TextTopicItemName = new UiTextTopic();
        [Child] private UiWindow Window = new();
        [Child] private UiPageText PageText;

        private readonly List<ItemDescriptionEntry> _rawEntries = new();
        private readonly UiPageModel<ItemDescriptionEntry> _wrappedEntries = new(15);

        public ItemDescriptionLayer()
        {
            AssetEnchantmentIcons = Assets.Get(Protos.Asset.EnchantmentIcons);
            AssetInheritanceIcon = Assets.Get(Protos.Asset.InheritanceIcon);

            PageText = new UiPageText(Window);

            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
        }

        public override void Initialize(Args args)
        {
            _otherItems = args.Entities;
            _currentIndex = args.SelectedIndex;
            Window.KeyHints = MakeKeyHints();

            UpdateDescription();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs obj)
        {
            if (obj.Function == EngineKeyFunctions.UICancel)
            {
                Finish(new Result() { SelectedIndexOnExit = _currentIndex });
            }
            else if (obj.Function == EngineKeyFunctions.UISelect)
            {
                Finish(new Result() { SelectedIndexOnExit = _currentIndex });
            }
            else if (obj.Function == EngineKeyFunctions.UIUp)
            {
                SelectItem(-1);
            }
            else if (obj.Function == EngineKeyFunctions.UIDown)
            {
                SelectItem(1);
            }
            else if (obj.Function == EngineKeyFunctions.UIPreviousPage)
            {
                if (_wrappedEntries.PageBackward())
                    _audio.Play(Protos.Sound.Pop1);
                PageText.UpdatePageText(_wrappedEntries.CurrentPage, _wrappedEntries.PageCount);
            }
            else if (obj.Function == EngineKeyFunctions.UINextPage)
            {
                if (_wrappedEntries.PageForward())
                    _audio.Play(Protos.Sound.Pop1);
                PageText.UpdatePageText(_wrappedEntries.CurrentPage, _wrappedEntries.PageCount);
            }
        }

        private void SelectItem(int delta)
        {
            if (_otherItems.Count == 0)
                return;

            _currentIndex = MathHelper.Wrap(_currentIndex + delta, 0, _otherItems.Count - 1);
            _audio.Play(Protos.Sound.Cursor1);

            UpdateDescription();
            WrapDescription();
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.Close, new[] { EngineKeyFunctions.UISelect, EngineKeyFunctions.UICancel }));

            if (_wrappedEntries.PageCount > 1)
                keyHints.Add(new(UiKeyHints.Page, new[] { EngineKeyFunctions.UIPreviousPage, EngineKeyFunctions.UINextPage }));

            if (_otherItems.Count > 1)
                keyHints.Add(new(UiKeyHints.Page, new[] { EngineKeyFunctions.UIUp, EngineKeyFunctions.UIDown }));

            return keyHints;
        }

        private void UpdateDescription()
        {
            var title = Loc.GetString("Elona.Inventory.ItemDescriptionLayer.WindowTitle");

            if (_otherItems.Count > 1)
                Window.Title = $"{title} ({_currentIndex + 1}/{_otherItems.Count})";
            else
                Window.Title = title;

            TextTopicItemName.Text = Loc.GetString("Elona.Common.NameWithDirectArticle", ("entity", _currentItem));

            _rawEntries.Clear();
            _itemDescSystem.GetItemDescription(_currentItem, _rawEntries);
        }

        private void WrapDescription()
        {
            var maxWidth = Width - (68 * 2) - UiFonts.ItemDescNormal.LoveFont.GetWidthV(UIScale, " ");

            var wrappedEntries = new List<ItemDescriptionEntry>();

            foreach (var entry in _rawEntries)
            {
                if (entry.Type == ItemDescriptionType.Flavor)
                {
                    var (_, wrapped) = UiFonts.ItemDescFlavor.LoveFont.GetWrapS(UIScale, entry.Text, maxWidth);
                    foreach (var text in wrapped)
                    {
                        wrappedEntries.Add(new ItemDescriptionEntry()
                        {
                            Text = text,
                            TextColor = entry.TextColor,
                            Type = entry.Type,
                        });
                    }
                }
                else
                {
                    wrappedEntries.Add(entry);
                }
            }

            _wrappedEntries.SetElements(wrappedEntries);
            PageText.UpdatePageText(_wrappedEntries.CurrentPage, _wrappedEntries.PageCount);
            Window.KeyHints = MakeKeyHints();
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);

            Window.SetSize(Width, Height);
            PageText.SetPreferredSize();
            TextTopicItemName.SetPreferredSize();
            WrapDescription();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);

            Window.SetPosition(X, Y);
            PageText.SetPosition(X, Y);
            TextTopicItemName.SetPosition(X + 28, Y + 34);
        }

        public override void OnQuery()
        {
            Sounds.Play(Protos.Sound.Pop2);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(600, 408, out bounds);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            PageText.Update(dt);
            TextTopicItemName.Update(dt);
        }

        public override void Draw()
        {
            Window.Draw();
            PageText.Draw();

            TextTopicItemName.Draw();

            for (int i = 0; i < _wrappedEntries.Count; i++)
            {
                var entry = _wrappedEntries[i];
                var x = X + 68;
                var y = Y + 68 + i * 18;
                ItemDescriptionIcon? icon = null;

                // TODO better text handling
                FontSpec font;
                switch (entry.Type)
                {
                    case ItemDescriptionType.Flavor:
                        font = UiFonts.ItemDescFlavor.WithColor(entry.TextColor);
                        break;
                    case ItemDescriptionType.FlavorItalic:
                        font = UiFonts.ItemDescFlavorItalic.WithColor(entry.TextColor);
                        x = Rect.Right - font.LoveFont.GetWidth(entry.Text) - 80;
                        break;
                    case ItemDescriptionType.Normal:
                    default:
                        font = UiFonts.ItemDescNormal.WithColor(entry.TextColor);
                        icon = entry.Icon;
                        break;
                }

                GraphicsEx.SetFont(font);
                GraphicsS.PrintS(UIScale, entry.Text, x, y);

                Love.Graphics.SetColor(Color.White);

                if (icon.HasValue)
                {
                    AssetEnchantmentIcons.DrawRegion(UIScale, ((int)icon.Value).ToString(), x - 28, y - 7);
                }

                if (entry.IsInheritable)
                {
                    AssetInheritanceIcon.Draw(UIScale, x - 53, y - 5);
                }
            }
        }
    }
}