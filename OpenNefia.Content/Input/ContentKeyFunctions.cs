using OpenNefia.Core.Input;

namespace OpenNefia.Content.Input
{
    /// <summary>
    /// NOTE: When adding a new key function here, be sure to register it in <see
    /// cref="ContentContexts"/> and keybinds.yml also.
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
        public static readonly BoundKeyFunction Dig = "Elona.Dig";
        public static readonly BoundKeyFunction Drop = "Elona.Drop";
        public static readonly BoundKeyFunction Drink = "Elona.Drink";
        public static readonly BoundKeyFunction Eat = "Elona.Eat";
        public static readonly BoundKeyFunction Throw = "Elona.Throw";
        public static readonly BoundKeyFunction Open = "Elona.Open";
        public static readonly BoundKeyFunction Fire = "Elona.Fire";
        public static readonly BoundKeyFunction Ammo = "Elona.Ammo";
        public static readonly BoundKeyFunction Bash = "Elona.Bash";
        public static readonly BoundKeyFunction Rest = "Elona.Rest";
        public static readonly BoundKeyFunction Interact = "Elona.Interact";
        public static readonly BoundKeyFunction Use = "Elona.Use";
        public static readonly BoundKeyFunction Read = "Elona.Read";
        public static readonly BoundKeyFunction Examine = "Elona.Examine";
        public static readonly BoundKeyFunction CharaInfo = "Elona.CharaInfo";
        public static readonly BoundKeyFunction Equipment = "Elona.Equipment";
        public static readonly BoundKeyFunction FeatInfo = "Elona.FeatInfo";
        public static readonly BoundKeyFunction Backlog = "Elona.Backlog";
        public static readonly BoundKeyFunction Journal = "Elona.Journal";
        public static readonly BoundKeyFunction ChatLog = "Elona.ChatLog";

        public static readonly BoundKeyFunction ReplFullscreen = "Elona.ReplFullscreen";
        public static readonly BoundKeyFunction ReplPrevCompletion = "Elona.ReplPrevCompletion";
        public static readonly BoundKeyFunction ReplNextCompletion = "Elona.ReplNextCompletion";
        public static readonly BoundKeyFunction ReplComplete = "Elona.ReplComplete";
        public static readonly BoundKeyFunction ReplClear = "Elona.ReplClear";

        public static readonly BoundKeyFunction QuickStart = "Elona.QuickStart";
    }
}
