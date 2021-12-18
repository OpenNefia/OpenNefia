using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class FactionComponent : Component
    {
        public override string Name => "Faction";

        [DataField]
        public Relation Relation { get; set; } = Relation.Neutral;
    }

    public enum Relation : int
    {
        Enemy = -3,
        Hate = -2,
        Dislike = -1,
        Neutral = 0,
        Ally = 10,
    }
}
