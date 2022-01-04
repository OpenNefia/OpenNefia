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
            common.AddFunction(ContentKeyFunctions.DiagonalOnly);

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
            field.AddFunction(ContentKeyFunctions.Examine);
        }
    }
}
