using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.Prototypes;
using OpenNefia.VisualAI.Engine;
using OpenNefia.Core.Utility;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.ConfigMenu.UICell;
using System.Collections.Generic;

namespace OpenNefia.VisualAI.UserInterface
{
    /// <summary>
    /// One entry in a <see cref="UiList{T}"/> of dynamic configurable variables.
    /// </summary>
    public interface IDynamicVariableItem
    {
        public string Name { get; }
        public IDynamicVariable Variable { get; }
    }

    public sealed class VisualAIConfigureBlockList : UiList<IDynamicVariableItem>
    {
        [Dependency] private readonly IAudioManager _audio = default!;

        public delegate void ConfigRefreshedDelegate();
        public event ConfigRefreshedDelegate? OnRefreshConfig;

        public sealed class Item : IDynamicVariableItem
        {
            public Item(string category, string name, VisualAIVariable variable)
            {
                Category = category;
                Name = name;
                Variable = variable;
            }

            public string Category { get; }
            public string Name { get; }
            public VisualAIVariable Variable { get; }
            IDynamicVariable IDynamicVariableItem.Variable => Variable;
        }

        public VisualAIConfigureBlockList()
        {
            // TODO
            EntitySystem.InjectDependencies(this);
        }

        public void SetItems(IEnumerable<Item> items)
        {
            SetCells(items.Select(MakeListCell));
            RefreshConfigValueDisplay();
        }

        private void RefreshConfigValueDisplay()
        {
            // FIXME: #35
            foreach (var cell in DisplayedCells.Cast<BaseDynamicVariableListCell>())
            {
                cell.RefreshVariableValueDisplay();
                cell.ValueTextElem.Text = cell.ValueTextElem.Text.WideSubstring(0, 15);
            }

            OnRefreshConfig?.Invoke();
        }

        private static BaseDynamicVariableListCell MakeListCell(Item item)
        {
            var type = item.Variable.Type;
            var attr = item.Variable.Attribute;

            if (type == typeof(bool))
            {
                return new DynamicVariableBoolCell(item);
            }
            else if (type == typeof(int))
            {
                return new DynamicVariableIntCell(item, (int?)attr.MinValue, (int?)attr.MaxValue, (int)attr.StepAmount);
            }
            else if (type == typeof(float))
            {
                return new DynamicVariableFloatCell(item, attr.MinValue, attr.MaxValue, (int)attr.StepAmount);
            }
            else if (type.IsAssignableTo(typeof(Enum)))
            {
                return new DynamicVariableEnumCell(item);
            }
            else if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(PrototypeId<>))
                {
                    return new DynamicVariablePrototypeIdCell(item);
                }
            }

            throw new InvalidDataException($"Unsupported Visual AI variable type: {type.FullName}");
        }

        protected override void HandleActivate(UiListEventArgs<IDynamicVariableItem> e)
        {
            var cell = (BaseDynamicVariableListCell)e.SelectedCell;

            if (cell.CanActivate())
            {
                _audio.Play(Protos.Sound.Ok1);
                cell.Activate();
                RefreshConfigValueDisplay();
            }
            else
            {
                base.HandleActivate(e);
            }
        }

        protected override void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (SelectedCell == null)
                return;

            var cell = (BaseDynamicVariableListCell)SelectedCell;
            var (canDec, canInc) = cell.CanChange();

            if (args.Function == EngineKeyFunctions.UILeft)
            {
                if (canDec)
                {
                    _audio.Play(Protos.Sound.Cursor1);
                    cell.Change(-1);
                    RefreshConfigValueDisplay();
                }
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UIRight)
            {
                if (canInc)
                {
                    _audio.Play(Protos.Sound.Cursor1);
                    cell.Change(1);
                    RefreshConfigValueDisplay();
                }
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UISelect)
            {
                // Skip so configure menu can be exited out of (?)
                return;
            }
            else
            {
                base.HandleKeyBindDown(args);
            }
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var hints = base.MakeKeyHints();
            hints.Add(new(UiKeyHints.Page, new[] { EngineKeyFunctions.UIPreviousPage, EngineKeyFunctions.UINextPage }));
            return hints;
        }
    }
}
