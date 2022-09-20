using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Factions
{
    [RegisterComponent]
    public class FactionComponent : Component
    {
        /// <summary>
        /// The hostility of this entity from the standpoint of the player.
        /// </summary>
        [DataField("relation")]
        public Relation RelationToPlayer { get; set; } = Relation.Neutral;

        [DataField]
        public Dictionary<EntityUid, Relation> PersonalRelations { get; set; } = new();
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
