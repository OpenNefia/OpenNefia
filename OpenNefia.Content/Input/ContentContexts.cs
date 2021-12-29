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
            var field = contexts.GetContext("field");
            field.AddFunction(ContentKeyFunctions.Ascend);
            field.AddFunction(ContentKeyFunctions.Descend);
            field.AddFunction(ContentKeyFunctions.Activate);
        }
    }
}
