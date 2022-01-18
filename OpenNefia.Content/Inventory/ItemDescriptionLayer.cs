using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Content.UI;
using OpenNefia.Content.Input;
using System.Collections.Generic;

namespace OpenNefia.Content.Inventory
{
    public class ItemDescriptionLayer : UiLayerWithResult<EntityUid, UINone>
    {
        [Dependency] private readonly ItemDescriptionSystem _itemDescSystem = default!;
        [Dependency] private readonly DisplayNameSystem _displayNames = default!;

        private EntityUid _item;

        private IAssetInstance AssetEnchantmentIcons;
        private IAssetInstance AssetInheritanceIcon;

        private UiText TextTopicItemName = new UiTextTopic();

        private UiWindow Window = new();

        private readonly List<ItemDescriptionEntry> _rawEntries = new();
        private readonly List<ItemDescriptionEntry> _wrappedEntries = new();

        public ItemDescriptionLayer()
        {
            AssetEnchantmentIcons = Assets.Get(Protos.Asset.EnchantmentIcons);
            AssetInheritanceIcon = Assets.Get(Protos.Asset.InheritanceIcon);

            AddChild(TextTopicItemName);
            AddChild(Window);

            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
        }

        public override void Initialize(EntityUid item)
        {
            _item = item;
            TextTopicItemName.Text = Loc.GetString("Elona.Common.NameWithDirectArticle", ("entity", _item));

            GetDescription();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs obj)
        {
            if (obj.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
            else if (obj.Function == EngineKeyFunctions.UISelect)
            {
                Finish(new UINone());
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var keyHints = base.MakeKeyHints();

            keyHints.Add(new(UiKeyHints.Close, new[] { EngineKeyFunctions.UISelect, EngineKeyFunctions.UICancel }));

            return keyHints;
        }

        private void GetDescription()
        {
            Window.Title = Loc.GetString("Elona.Inventory.ItemDescriptionLayer.WindowTitle");

            _rawEntries.Clear();
            _itemDescSystem.GetItemDescription(_item, _rawEntries);
        }

        private void WrapDescription(float maxWidth)
        {
            _wrappedEntries.Clear();

            foreach (var entry in _rawEntries)
            {
                if (entry.Type == ItemDescriptionType.Flavor)
                {
                    var (_, wrapped) = UiFonts.ItemDescFlavor.LoveFont.GetWrapS(UIScale, entry.Text, maxWidth);
                    foreach (var text in wrapped)
                    {
                        _wrappedEntries.Add(new ItemDescriptionEntry()
                        {
                            Text = text,
                            TextColor = entry.TextColor,
                            Type = entry.Type,
                        });
                    }
                }
                else
                {
                    _wrappedEntries.Add(entry);
                }
            }
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);

            Window.SetSize(Width, Height);
            TextTopicItemName.SetPreferredSize();

            var maxWidth = Width - (68 * 2) - UiFonts.ItemDescNormal.LoveFont.GetWidthV(UIScale, " ");
            WrapDescription(maxWidth);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);

            Window.SetPosition(X, Y);
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
            TextTopicItemName.Update(dt);
        }

        public override void Draw()
        {
            Window.Draw();

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
                    AssetEnchantmentIcons.DrawRegionS(UIScale, ((int)icon.Value).ToString(), x - 28, y - 7);
                }

                if (entry.IsInheritable)
                {
                    AssetInheritanceIcon.DrawS(UIScale, x - 53, y - 5);
                }
            }
        }
    }
}