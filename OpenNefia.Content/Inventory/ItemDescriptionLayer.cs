using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Inventory
{
    public class ItemDescriptionLayer : BaseUiLayer<UiNoResult>
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ItemDescriptionSystem _itemDescSystem = default!;

        private EntityUid _item;

        private IAssetInstance AssetEnchantmentIcons;
        private IAssetInstance AssetInheritanceIcon;

        private IUiText TextTopicItemName = new UiTextTopic();

        private UiWindow Window = new();

        private readonly List<ItemDescriptionEntry> _entries = new();

        public ItemDescriptionLayer(EntityUid item)
        {
            EntitySystem.InjectDependencies(this);

            AssetEnchantmentIcons = Assets.Get(AssetPrototypeOf.EnchantmentIcons);
            AssetInheritanceIcon = Assets.Get(AssetPrototypeOf.InheritanceIcon);

            _item = item;

            TextTopicItemName.Text = DisplayNameSystem.GetDisplayName(_item);

            BindKeys();

            BuildDescription();
        }

        private void BindKeys()
        {
            Keybinds[CoreKeybinds.Cancel] += (_) => Cancel();
            Keybinds[CoreKeybinds.Escape] += (_) => Cancel();
            Keybinds[CoreKeybinds.Enter] += (_) => Finish(new UiNoResult());
        }

        private void BuildDescription()
        {
            Window.Title = "Known Information";

            _itemDescSystem.GetItemDescription(_item, _entries);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);

            Window.SetSize(Width, Height);
            TextTopicItemName.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            Window.SetPosition(X, Y);
            TextTopicItemName.SetPosition(X + 28, Y + 34);
        }

        public override void OnQuery()
        {
            Sounds.Play(Protos.Sound.Pop2);
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
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

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                var x = X + 68;
                var y = Y + 68 + i * 18;
                ItemDescriptionIcon? icon = null;

                // TODO better text handling
                FontSpec font;
                switch (entry.Type)
                {
                    case ItemDescriptionType.Flavor:
                        font = new FontSpec(13, 11, color: entry.TextColor, style: FontStyle.Italic);
                        break;
                    case ItemDescriptionType.FlavorItalic:
                        font = new FontSpec(13, 11, color: entry.TextColor, style: FontStyle.Italic);
                        x = Bounds.Right - font.LoveFont.GetWidth(entry.Text) - 80;
                        break;
                    case ItemDescriptionType.Normal:
                    default:
                        font = new FontSpec(14, 12, color: entry.TextColor);
                        icon = entry.Icon;
                        break;
                }

                GraphicsEx.SetFont(font);
                Love.Graphics.Print(entry.Text, x, y);

                Love.Graphics.SetColor(Color.White);
            
                if (icon.HasValue)
                {
                    AssetEnchantmentIcons.DrawRegion(((int)icon.Value).ToString(), x - 28, y - 7);
                }

                if (entry.IsInheritable)
                {
                    AssetInheritanceIcon.Draw(x - 53, y - 5);
                }
            }
        }
    }

    [DataDefinition]
    public class ItemDescriptionEntry
    {
        [DataField]
        public string Text { get; set; } = string.Empty;
        
        [DataField]
        public Color TextColor { get; set; } = Color.Black;
        
        [DataField]
        public ItemDescriptionType Type { get; set; }
        
        [DataField]
        public ItemDescriptionIcon? Icon { get; set; }
        
        [DataField]
        public bool IsInheritable { get; set; }
    }

    public enum ItemDescriptionType
    {
        Normal,
        Flavor,
        FlavorItalic
    }

    public enum ItemDescriptionIcon : int
    {
        Icon0 = 0,
        Icon1 = 1,
        Icon2 = 2,
        Icon3 = 3,
        Icon4 = 4,
        Icon5 = 5,
        Icon6 = 6,
        Icon7 = 7,
        Icon8 = 8,
        Icon9 = 9,
    }
}