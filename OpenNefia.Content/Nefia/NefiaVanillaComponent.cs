using OpenNefia.Content.Nefia.Layout;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// Nefia generation algorithm based on vanilla's. First picks the dungeon
    /// layout randomly using a template, then tries up to 2000 times to generate 
    /// a dungeon with that layout.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public class NefiaVanillaComponent : Component
    {
        public override string Name => "NefiaVanilla";

        [DataField]
        public IVanillaNefiaTemplate Template { get; set; } = new NefiaTemplateDungeon();
    }
}
