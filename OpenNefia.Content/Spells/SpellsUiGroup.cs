using OpenNefia.Content.Inventory;
using OpenNefia.Content.UI;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Spells
{
    public class SpellsGroupSublayerArgs
    {
        public enum SpellsTab
        {
            Spells,
            Actions
        }

        public SpellsTab Type { get; }

        public SpellsGroupSublayerArgs(SpellsTab type)
        {
            Type = type;
        }
    }

    public class SpellsGroupUiLayer : GroupableUiLayer<SpellsGroupSublayerArgs, UINone>
    {
        public SpellsGroupUiLayer()
        {
            EventFilter = UIEventFilterMode.Pass;
            CanControlFocus = true;
            OnKeyBindDown += OnKeyDown;
        }

        protected virtual void OnKeyDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
                Cancel();
        }
    }

    public class SpellsUiGroupArgs : UiGroupArgs<SpellsGroupUiLayer, SpellsGroupSublayerArgs>
    {
        public SpellsUiGroupArgs(SpellsGroupSublayerArgs.SpellsTab selectedTab)
        {
            foreach (SpellsGroupSublayerArgs.SpellsTab SpellsType in Enum.GetValues(typeof(SpellsGroupSublayerArgs.SpellsTab)))
            {
                var args = new SpellsGroupSublayerArgs(SpellsType);
                if (SpellsType == selectedTab)
                    SelectedArgs = args;

                Layers[args] = SpellsType switch
                {
                    SpellsGroupSublayerArgs.SpellsTab.Spells => new SpellsUiLayer(),
                    // SpellsGroupSublayerArgs.SpellsTab.Actions => new JournalUiLayer(),
                    // TODO: add other group layers
                    _ => new SpellsGroupUiLayer()
                };
            }
        }
    }

    public class SpellsUiGroup : UiGroup<SpellsGroupUiLayer, SpellsUiGroupArgs, SpellsGroupSublayerArgs, UINone>
    {
        protected override AssetDrawable? GetIcon(SpellsGroupSublayerArgs args)
        {
            //var iconType = args.Type switch
            //{
            //    SpellsGroupSublayerArgs.SpellsTab.Spells => InventoryIcon.Spells,
            //    SpellsGroupSublayerArgs.SpellsTab.Actions => InventoryIcon.Read,
            //    SpellsGroupSublayerArgs.SpellsTab.ChatSpells => InventoryIcon.Chat,
            //    _ => InventoryIcon.Drink
            //};

            //var icon = InventoryHelpers.MakeIcon(iconType);
            //if (icon is not AssetDrawable iconAsset)
            //    return null;

            //return iconAsset;
            return null;
        }

        protected override string GetTabName(SpellsGroupSublayerArgs args)
        {
            return Loc.GetString($"Elona.UI.MenuGroup.Spells.{args.Type}");
        }
    }
}
