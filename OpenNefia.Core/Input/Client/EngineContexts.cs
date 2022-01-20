namespace OpenNefia.Core.Input
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

            common.AddFunction(EngineKeyFunctions.North);
            common.AddFunction(EngineKeyFunctions.South);
            common.AddFunction(EngineKeyFunctions.West);
            common.AddFunction(EngineKeyFunctions.East);
            common.AddFunction(EngineKeyFunctions.Southeast);
            common.AddFunction(EngineKeyFunctions.Northeast);
            common.AddFunction(EngineKeyFunctions.Northwest);
            common.AddFunction(EngineKeyFunctions.Southwest);
            common.AddFunction(EngineKeyFunctions.Wait);

            common.AddFunction(EngineKeyFunctions.ShowDebugConsole);
            common.AddFunction(EngineKeyFunctions.ShowDebugMonitors);

            common.AddFunction(EngineKeyFunctions.UIUp);
            common.AddFunction(EngineKeyFunctions.UIDown);
            common.AddFunction(EngineKeyFunctions.UILeft);
            common.AddFunction(EngineKeyFunctions.UIRight);
            common.AddFunction(EngineKeyFunctions.UISelect);
            common.AddFunction(EngineKeyFunctions.UICancel);

            common.AddFunction(EngineKeyFunctions.UINextPage);
            common.AddFunction(EngineKeyFunctions.UIPreviousPage);
            common.AddFunction(EngineKeyFunctions.UINextTab);
            common.AddFunction(EngineKeyFunctions.UIPreviousTab);
            common.AddFunction(EngineKeyFunctions.Backlog);

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
            common.AddFunction(EngineKeyFunctions.TextPageUp);
            common.AddFunction(EngineKeyFunctions.TextPageDown);
            common.AddFunction(EngineKeyFunctions.TextDelete);
            common.AddFunction(EngineKeyFunctions.TextTabComplete);

            var field = contexts.New("field", common);
            field.AddFunction(EngineKeyFunctions.ShowEscapeMenu);
            field.AddFunction(EngineKeyFunctions.QuickSaveGame);
            field.AddFunction(EngineKeyFunctions.QuickLoadGame);
        }
    }
}
