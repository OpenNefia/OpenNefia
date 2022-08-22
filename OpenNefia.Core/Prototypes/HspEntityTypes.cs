namespace OpenNefia.Core.Prototypes
{
    /// <summary>
    /// Type identifiers for entity prototypes in HSP variants of Elona.
    /// 
    /// Since all types of entities (character, item, feat, mef) are consolidated
    /// under <see cref="EntityPrototype"/>, it becomes necessary to state both what
    /// the "logical" type ("chara", "item") and the numeric ID are.
    /// </summary>
    public static class HspEntityTypes
    {
        public const string Chara = "chara";
        public const string Item = "item";
        public const string Feat = "feat";
        public const string Mef = "mef";
        public const string Enchantment = "enchantment";
    }
}
