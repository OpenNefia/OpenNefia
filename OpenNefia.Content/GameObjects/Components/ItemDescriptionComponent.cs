using OpenNefia.Content.Inventory;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    [RegisterComponent]
    public class ItemDescriptionComponent : Component, IComponentLocalizable
    {
        public override string Name => "ItemDescription";

        [Localize]
        public ItemDescriptionEntry? Primary { get; private set; }

        private List<ItemDescriptionEntry> _extra = new();

        public IReadOnlyList<ItemDescriptionEntry> Extra => _extra;

        void IComponentLocalizable.LocalizeFromLua(NLua.LuaTable table)
        {
            if (table.TryGetTable(nameof(Primary), out var primaryTbl))
            {
                Primary = ItemDescriptionEntry.FromLua(primaryTbl).First();
            }
            else
            {
                Primary = null;
            }

            _extra.Clear();

            for (int i = 0; i < 5; i++)
            {
                if (table.TryGetValue(i, out var obj))
                {
                    if (obj is NLua.LuaTable extraDescTbl)
                    {
                        foreach (var desc in ItemDescriptionEntry.FromLua(extraDescTbl))
                        {
                            _extra.Add(desc);
                        }
                    }
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

        public static IEnumerable<ItemDescriptionEntry> FromLua(NLua.LuaTable table)
        {
            yield return new ItemDescriptionEntry()
            {
                Text = table.GetStringOrEmpty(nameof(Text)),
            };

            if (table.TryGetString("Footnote", out var footnote))
            {
                yield return new ItemDescriptionEntry()
                {
                    Text = footnote,
                    Type = ItemDescriptionType.FlavorItalic
                };
            }
        }
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