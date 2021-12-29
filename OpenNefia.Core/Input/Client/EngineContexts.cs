﻿namespace OpenNefia.Core.Input
{
    /// <summary>
    ///     Contains a helper function for setting up all default engine contexts.
    /// </summary>
    internal static class EngineContexts
    {
        /// <summary>
        ///     Adds the default set of engine contexts to a context container.
        /// </summary>
        /// <param name="contexts">Default contexts will be set up inside this container.</param>
        public static void SetupContexts(IInputContextContainer contexts)
        {
            var common = contexts.GetContext(InputContextContainer.DefaultContextName);
            common.AddFunction(EngineKeyFunctions.UIClick);
            common.AddFunction(EngineKeyFunctions.UIRightClick);

            common.AddFunction(EngineKeyFunctions.ShowDebugConsole);
            common.AddFunction(EngineKeyFunctions.ShowDebugMonitors);

            common.AddFunction(EngineKeyFunctions.UISelect);
            common.AddFunction(EngineKeyFunctions.UICancel);

            common.AddFunction(EngineKeyFunctions.UIUp);
            common.AddFunction(EngineKeyFunctions.UIDown);
            common.AddFunction(EngineKeyFunctions.UILeft);
            common.AddFunction(EngineKeyFunctions.UIRight);

            common.AddFunction(EngineKeyFunctions.TextCursorLeft);
            common.AddFunction(EngineKeyFunctions.TextCursorRight);
            common.AddFunction(EngineKeyFunctions.TextCursorWordLeft);
            common.AddFunction(EngineKeyFunctions.TextCursorWordRight);
            common.AddFunction(EngineKeyFunctions.TextCursorBegin);
            common.AddFunction(EngineKeyFunctions.TextCursorEnd);

            common.AddFunction(EngineKeyFunctions.TextCursorSelect);
            common.AddFunction(EngineKeyFunctions.TextCursorSelectLeft);
            common.AddFunction(EngineKeyFunctions.TextCursorSelectRight);
            common.AddFunction(EngineKeyFunctions.TextCursorSelectWordLeft);
            common.AddFunction(EngineKeyFunctions.TextCursorSelectWordRight);
            common.AddFunction(EngineKeyFunctions.TextCursorSelectBegin);
            common.AddFunction(EngineKeyFunctions.TextCursorSelectEnd);

            common.AddFunction(EngineKeyFunctions.TextBackspace);
            common.AddFunction(EngineKeyFunctions.TextSubmit);
            common.AddFunction(EngineKeyFunctions.TextCopy);
            common.AddFunction(EngineKeyFunctions.TextCut);
            common.AddFunction(EngineKeyFunctions.TextPaste);
            common.AddFunction(EngineKeyFunctions.TextSelectAll);
            common.AddFunction(EngineKeyFunctions.TextHistoryPrev);
            common.AddFunction(EngineKeyFunctions.TextHistoryNext);
            common.AddFunction(EngineKeyFunctions.TextReleaseFocus);
            common.AddFunction(EngineKeyFunctions.TextScrollToBottom);
            common.AddFunction(EngineKeyFunctions.TextDelete);
            common.AddFunction(EngineKeyFunctions.TextTabComplete);

            var field = contexts.New("field", common);
            field.AddFunction(EngineKeyFunctions.North);
            field.AddFunction(EngineKeyFunctions.South);
            field.AddFunction(EngineKeyFunctions.West);
            field.AddFunction(EngineKeyFunctions.East);
            field.AddFunction(EngineKeyFunctions.Southeast);
            field.AddFunction(EngineKeyFunctions.Northeast);
            field.AddFunction(EngineKeyFunctions.Northwest);
            field.AddFunction(EngineKeyFunctions.Southwest);

            field.AddFunction(EngineKeyFunctions.ShowEscapeMenu);
        }
    }
}