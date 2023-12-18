using OpenNefia.Core.Input;

namespace OpenNefia.Content.Input
{
    /// <summary>
    ///     Contains a helper function for setting up all content
    ///     contexts, and modifying existing engine ones.
    /// </summary>
    public static class ContentContexts
    {
        public static void SetupContexts(IInputContextContainer contexts)
        {
            var common = contexts.GetContext("common");
            common.AddFunction(ContentKeyFunctions.UIIdentify);
            common.AddFunction(ContentKeyFunctions.UIMode);
            common.AddFunction(ContentKeyFunctions.UIMode2);
            common.AddFunction(ContentKeyFunctions.UIPortrait);

            common.AddFunction(ContentKeyFunctions.ReplFullscreen);
            common.AddFunction(ContentKeyFunctions.ReplPrevCompletion);
            common.AddFunction(ContentKeyFunctions.ReplNextCompletion);
            common.AddFunction(ContentKeyFunctions.ReplComplete);
            common.AddFunction(ContentKeyFunctions.ReplClear);

            common.AddFunction(ContentKeyFunctions.DiagonalOnly);

            // Used in both the field and in the backlog layer itself.
            common.AddFunction(ContentKeyFunctions.Backlog);

            common.AddFunction(ContentKeyFunctions.QuickStart);

            var field = contexts.GetContext("field");
            field.AddFunction(ContentKeyFunctions.Ascend);
            field.AddFunction(ContentKeyFunctions.Descend);
            field.AddFunction(ContentKeyFunctions.Activate);
            field.AddFunction(ContentKeyFunctions.Close);

            field.AddFunction(ContentKeyFunctions.PickUp);
            field.AddFunction(ContentKeyFunctions.Drop);
            field.AddFunction(ContentKeyFunctions.Drink);
            field.AddFunction(ContentKeyFunctions.Eat);
            field.AddFunction(ContentKeyFunctions.Throw);
            field.AddFunction(ContentKeyFunctions.Open);
            field.AddFunction(ContentKeyFunctions.Fire);
            field.AddFunction(ContentKeyFunctions.Ammo);
            field.AddFunction(ContentKeyFunctions.Bash);
            field.AddFunction(ContentKeyFunctions.Rest);
            field.AddFunction(ContentKeyFunctions.Interact);
            field.AddFunction(ContentKeyFunctions.Use);
            field.AddFunction(ContentKeyFunctions.Read);
            field.AddFunction(ContentKeyFunctions.Target);
            field.AddFunction(ContentKeyFunctions.Look);
            field.AddFunction(ContentKeyFunctions.Examine);
            field.AddFunction(ContentKeyFunctions.Dig);
            field.AddFunction(ContentKeyFunctions.Cast);
            field.AddFunction(ContentKeyFunctions.Skill);

            field.AddFunction(ContentKeyFunctions.CharaInfo);
            field.AddFunction(ContentKeyFunctions.Equipment);
            field.AddFunction(ContentKeyFunctions.FeatInfo);
            field.AddFunction(ContentKeyFunctions.Journal);
            field.AddFunction(ContentKeyFunctions.ChatLog);
        }
    }
}
