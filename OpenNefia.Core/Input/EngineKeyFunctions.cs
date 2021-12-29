namespace OpenNefia.Core.Input
{
    /// <summary>
    /// NOTE: When adding a new key function here, be sure to register it in <see cref="EngineContexts"/> also.
    /// </summary>
    [KeyFunctions]
    public static class EngineKeyFunctions
    {
        public static readonly BoundKeyFunction North = "North";
        public static readonly BoundKeyFunction South = "South";
        public static readonly BoundKeyFunction West = "West";
        public static readonly BoundKeyFunction East = "East";
        public static readonly BoundKeyFunction Southeast = "Southeast";
        public static readonly BoundKeyFunction Northeast = "Northeast";
        public static readonly BoundKeyFunction Northwest = "Northwest";
        public static readonly BoundKeyFunction Southwest = "Southwest";

        public static readonly BoundKeyFunction UIClick = "UIClick";
        public static readonly BoundKeyFunction UIRightClick = "UIRightClick";

        public static readonly BoundKeyFunction UIUp = "UIUp";
        public static readonly BoundKeyFunction UIDown = "UIDown";
        public static readonly BoundKeyFunction UILeft = "UILeft";
        public static readonly BoundKeyFunction UIRight = "UIRight";
        public static readonly BoundKeyFunction UISelect = "UISelect";
        public static readonly BoundKeyFunction UICancel = "UICancel";

        public static readonly BoundKeyFunction ShowDebugConsole = "ShowDebugConsole";
        public static readonly BoundKeyFunction ShowDebugMonitors = "ShowDebugMonitors";
        public static readonly BoundKeyFunction ShowEscapeMenu = "ShowEscapeMenu";

        // Cursor keys in LineEdit and such.
        public static readonly BoundKeyFunction TextCursorLeft = "TextCursorLeft";
        public static readonly BoundKeyFunction TextCursorRight = "TextCursorRight";
        public static readonly BoundKeyFunction TextCursorWordLeft = "TextCursorWordLeft";
        public static readonly BoundKeyFunction TextCursorWordRight = "TextCursorWordRight";
        public static readonly BoundKeyFunction TextCursorBegin = "TextCursorBegin";
        public static readonly BoundKeyFunction TextCursorEnd = "TextCursorEnd";

        // Cursor keys for also selecting text.
        public static readonly BoundKeyFunction TextCursorSelect = "TextCursorSelect";
        public static readonly BoundKeyFunction TextCursorSelectLeft = "TextCursorSelectLeft";
        public static readonly BoundKeyFunction TextCursorSelectRight = "TextCursorSelectRight";
        public static readonly BoundKeyFunction TextCursorSelectWordLeft = "TextCursorSelectWordLeft";
        public static readonly BoundKeyFunction TextCursorSelectWordRight = "TextCursorSelectWordRight";
        public static readonly BoundKeyFunction TextCursorSelectBegin = "TextCursorSelectBegin";
        public static readonly BoundKeyFunction TextCursorSelectEnd = "TextCursorSelectEnd";

        public static readonly BoundKeyFunction TextBackspace = "TextBackspace";
        public static readonly BoundKeyFunction TextSubmit = "TextSubmit";
        public static readonly BoundKeyFunction TextSelectAll = "TextSelectAll";
        public static readonly BoundKeyFunction TextCopy = "TextCopy";
        public static readonly BoundKeyFunction TextCut = "TextCut";
        public static readonly BoundKeyFunction TextPaste = "TextPaste";
        public static readonly BoundKeyFunction TextHistoryPrev = "TextHistoryPrev";
        public static readonly BoundKeyFunction TextHistoryNext = "TextHistoryNext";
        public static readonly BoundKeyFunction TextReleaseFocus = "TextReleaseFocus";
        public static readonly BoundKeyFunction TextScrollToBottom = "TextScrollToBottom";
        public static readonly BoundKeyFunction TextDelete = "TextDelete";
        public static readonly BoundKeyFunction TextTabComplete = "TextTabComplete";
    }
}
