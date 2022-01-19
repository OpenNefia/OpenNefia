using OpenNefia.Content.Inventory;
using OpenNefia.Content.UI;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Journal
{
    public class JournalGroupUiArgs
    {
        public enum LogTab
        {
            Backlog,
            Journal,
            Chat
        }

        public LogTab Type;

        public JournalGroupUiArgs(LogTab type)
        {
            Type = type;
        }
    }

    public class JournalUiLayer : GroupableUiLayer<JournalGroupUiArgs, UINone>
    {
        public JournalUiLayer()
        {
            EventFilter = UIEventFilterMode.Pass;
            CanControlFocus = true;
        }
        protected virtual void OnKeyDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
                Cancel();
        }

        public override void OnQuery()
        {
            base.OnQuery();
            OnKeyBindDown += OnKeyDown;
        }

        public override void OnQueryFinish()
        {
            base.OnQueryFinish();
            OnKeyBindDown -= OnKeyDown;
        }
    }

    public class JournalUiGroupArgs : UiGroupArgs<JournalUiLayer, JournalGroupUiArgs>
    {
        public JournalUiGroupArgs(JournalGroupUiArgs.LogTab type)
        {
            foreach (JournalGroupUiArgs.LogTab logType in Enum.GetValues(typeof(JournalGroupUiArgs.LogTab)))
            {
                var args = new JournalGroupUiArgs(logType);
                if (logType == type)
                    SelectedArgs = args;
                Layers[args] = logType switch
                {
                    JournalGroupUiArgs.LogTab.Backlog => new BacklogUiLayer(),
                    // TODO: add other group layers
                    _ => new JournalUiLayer()
                };
            }
        }
    }

    public class JournalUiGroup : UiGroup<JournalUiLayer, JournalUiGroupArgs, JournalGroupUiArgs, UINone>
    {
        protected override AssetDrawable? GetIcon(JournalGroupUiArgs args)
        {
            var iconType = args.Type switch
            {
                JournalGroupUiArgs.LogTab.Backlog => InventoryIcon.Log,
                JournalGroupUiArgs.LogTab.Journal => InventoryIcon.Read,
                JournalGroupUiArgs.LogTab.Chat => InventoryIcon.Chat,
                _ => InventoryIcon.Drink
            };

            var icon = InventoryHelpers.MakeIcon(iconType);
            if (icon is not AssetDrawable iconAsset)
                return null;

            iconAsset.OriginOffset = (-12, -32);
            return iconAsset;
        }

        protected override string GetText(JournalGroupUiArgs args)
        {
            return Loc.GetString($"Elona.Hud.LogGroup.{args.Type}");
        }
    }
}
