using OpenNefia.Content.Inventory;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Spells
{
    public class SpellGroupSublayerArgs
    {
        public enum SpellTab
        {
            Spell,
            Skill
        }

        public SpellTab Type { get; }

        public SpellGroupSublayerArgs(SpellTab type)
        {
            Type = type;
        }
    }

    public record SpellGroupSublayerResult
    {
        public sealed record CastSpell(SpellDefinition Spell) : SpellGroupSublayerResult
        {
            public override string ToString() => $"{nameof(CastSpell)}({Spell.Effect.ID}, {Spell.AssociatedSkill.ID})";
        }
        //public sealed record UseSkill(EntityUid? SelectedItem = null) : InventoryResult
        //{
        //    public override string ToString() => $"{nameof(Continuing)}()";
        //}
    }

    public class SpellGroupUiLayer : GroupableUiLayer<SpellGroupSublayerArgs, SpellGroupSublayerResult>
    {
        public SpellGroupUiLayer()
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

    public class SpellUiGroupArgs : UiGroupArgs<SpellGroupUiLayer, SpellGroupSublayerArgs>
    {
        public SpellUiGroupArgs(SpellGroupSublayerArgs.SpellTab selectedTab)
        {
            foreach (SpellGroupSublayerArgs.SpellTab SpellTabType in Enum.GetValues(typeof(SpellGroupSublayerArgs.SpellTab)))
            {
                var args = new SpellGroupSublayerArgs(SpellTabType);
                if (SpellTabType == selectedTab)
                    SelectedArgs = args;

                Layers[args] = SpellTabType switch
                {
                    SpellGroupSublayerArgs.SpellTab.Spell => new SpellsUiLayer(),
                    // SpellsGroupSublayerArgs.SpellsTab.Actions => new JournalUiLayer(),
                    // TODO: add other group layers
                    _ => new SpellGroupUiLayer()
                };
            }
        }
    }

    public class SpellUiGroup : UiGroup<SpellGroupUiLayer, SpellUiGroupArgs, SpellGroupSublayerArgs, SpellGroupSublayerResult>
    {
        protected override AssetDrawable? GetIcon(SpellGroupSublayerArgs args)
        {
            var iconType = args.Type switch
            {
                SpellGroupSublayerArgs.SpellTab.Spell => InventoryIcon.Spell,
                SpellGroupSublayerArgs.SpellTab.Skill => InventoryIcon.Skill,
                _ => InventoryIcon.Spell
            };

            var icon = InventoryHelpers.MakeIcon(iconType);
            if (icon is not AssetDrawable iconAsset)
                return null;

            return iconAsset;
        }

        protected override string GetTabName(SpellGroupSublayerArgs args)
        {
            return Loc.GetString($"Elona.UI.MenuGroup.Spell.{args.Type}");
        }
    }
}
