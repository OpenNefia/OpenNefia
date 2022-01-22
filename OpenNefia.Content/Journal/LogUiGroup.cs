using OpenNefia.Content.Inventory;
using OpenNefia.Content.UI;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.Journal
{
    public class LogGroupSublayerArgs
    {
        public enum LogTab
        {
            Backlog,
            Journal,
            ChatLog
        }

        public LogTab Type { get; }

        public LogGroupSublayerArgs(LogTab type)
        {
            Type = type;
        }
    }

    public class LogGroupUiLayer : GroupableUiLayer<LogGroupSublayerArgs, UINone>
    {
        public LogGroupUiLayer()
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

    public class LogUiGroupArgs : UiGroupArgs<LogGroupUiLayer, LogGroupSublayerArgs>
    {
        public LogUiGroupArgs(LogGroupSublayerArgs.LogTab selectedTab)
        {
            foreach (LogGroupSublayerArgs.LogTab logType in Enum.GetValues(typeof(LogGroupSublayerArgs.LogTab)))
            {
                var args = new LogGroupSublayerArgs(logType);
                if (logType == selectedTab)
                    SelectedArgs = args;

                Layers[args] = logType switch
                {
                    LogGroupSublayerArgs.LogTab.Backlog => new BacklogUiLayer(),
                    // TODO: add other group layers
                    _ => new LogGroupUiLayer()
                };
            }
        }
    }

    public class LogUiGroup : UiGroup<LogGroupUiLayer, LogUiGroupArgs, LogGroupSublayerArgs, UINone>
    {
        protected override AssetDrawable? GetIcon(LogGroupSublayerArgs args)
        {
            var iconType = args.Type switch
            {
                LogGroupSublayerArgs.LogTab.Backlog => InventoryIcon.Log,
                LogGroupSublayerArgs.LogTab.Journal => InventoryIcon.Read,
                LogGroupSublayerArgs.LogTab.ChatLog => InventoryIcon.Chat,
                _ => InventoryIcon.Drink
            };

            var icon = InventoryHelpers.MakeIcon(iconType);
            if (icon is not AssetDrawable iconAsset)
                return null;

            return iconAsset;
        }

        protected override string GetTabName(LogGroupSublayerArgs args)
        {
            return Loc.GetString($"Elona.UI.MenuGroup.Log.{args.Type}");
        }
    }
}
