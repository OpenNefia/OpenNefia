using OpenNefia.Core.Input;

namespace OpenNefia.Content.Input
{
    /// <summary>
    /// NOTE: When adding a new key function here, be sure to register it in <see cref="ContentContexts"/> also.
    /// </summary>
    [KeyFunctions]
    public static class ContentKeyFunctions
    {
        public static readonly BoundKeyFunction UIIdentify = "Elona.UIIdentify";
        public static readonly BoundKeyFunction UIMode = "Elona.UIMode";
        public static readonly BoundKeyFunction UIMode2 = "Elona.UIMode2";
        public static readonly BoundKeyFunction UIPortrait = "Elona.UIPortrait";

        public static readonly BoundKeyFunction DiagonalOnly = "Elona.DiagonalOnly";

        public static readonly BoundKeyFunction Ascend = "Elona.Ascend";
        public static readonly BoundKeyFunction Descend = "Elona.Descend";
        public static readonly BoundKeyFunction Activate = "Elona.Activate";
        public static readonly BoundKeyFunction Close = "Elona.Close";

        public static readonly BoundKeyFunction PickUp = "Elona.PickUp";
        public static readonly BoundKeyFunction Drop = "Elona.Drop";
        public static readonly BoundKeyFunction Drink = "Elona.Drink";
        public static readonly BoundKeyFunction Eat = "Elona.Eat";
        public static readonly BoundKeyFunction Throw = "Elona.Throw";
        public static readonly BoundKeyFunction Examine = "Elona.Examine";
        public static readonly BoundKeyFunction Wear = "Elona.Wear";
    }
}
