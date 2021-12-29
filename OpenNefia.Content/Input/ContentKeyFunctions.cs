using OpenNefia.Core.Input;

namespace OpenNefia.Content.Input
{
    /// <summary>
    /// NOTE: When adding a new key function here, be sure to register it in <see cref="ContentContexts"/> also.
    /// </summary>
    [KeyFunctions]
    public static class ContentKeyFunctions
    {
        public static readonly BoundKeyFunction Ascend = "Elona.Ascend";
        public static readonly BoundKeyFunction Descend = "Elona.Descend";
        public static readonly BoundKeyFunction Activate = "Elona.Activate";
    }
}
