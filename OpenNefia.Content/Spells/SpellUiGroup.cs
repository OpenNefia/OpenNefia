using OpenNefia.Content.Actions;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Content.Prototypes.Protos;

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
        public EntityUid Caster { get; }

        public SpellGroupSublayerArgs(SpellTab type, EntityUid caster)
        {
            Type = type;
            Caster = caster;
        }
    }

    public record SpellGroupSublayerResult
    {
        public sealed record CastSpell(SpellPrototype Spell) : SpellGroupSublayerResult
        {
            public override string ToString() => $"{nameof(CastSpell)}({Spell.EffectID}, {Spell.SkillID})";
        }
        public sealed record InvokeAction(ActionPrototype Action) : SpellGroupSublayerResult
        {
            public override string ToString() => $"{nameof(InvokeAction)}({Action.EffectID}, {Action.SkillID})";
        }
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
        public SpellUiGroupArgs(SpellGroupSublayerArgs.SpellTab selectedTab, EntityUid caster)
        {
            foreach (SpellGroupSublayerArgs.SpellTab SpellTabType in Enum.GetValues(typeof(SpellGroupSublayerArgs.SpellTab)))
            {
                var args = new SpellGroupSublayerArgs(SpellTabType, caster);
                if (SpellTabType == selectedTab)
                    SelectedArgs = args;

                Layers[args] = SpellTabType switch
                {
                    SpellGroupSublayerArgs.SpellTab.Spell => new SpellsUiLayer(),
                    SpellGroupSublayerArgs.SpellTab.Skill => new ActionsUiLayer(),
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
