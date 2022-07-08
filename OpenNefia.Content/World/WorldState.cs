using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.World
{
    [DataDefinition]
    public class WorldState
    {
        /// <summary>
        /// The date the game starts on when creating a new character.
        /// </summary>
        public static readonly GameDateTime DefaultDate = new(517, 8, 12, 1, 10, 0);

        /// <summary>
        /// Current in-game date.
        /// </summary>
        [DataField]
        public GameDateTime GameDate { get; set; } = DefaultDate;

        /// <summary>
        /// The date the game was initially started. Used for things like calculating
        /// time passed.
        /// </summary>
        [DataField]
        public GameDateTime InitialDate { get; set; } = DefaultDate;

        /// <summary>
        /// Number of in-game minutes that have passed across all maps so far.
        /// </summary>
        [DataField]
        public int PlayTurns { get; set; }

        /// <summary>
        /// Total number of creatures the player has killed.
        /// </summary>
        [DataField]
        public int TotalKills { get; set; }

        /// <summary>
        /// Total number of times the player has died.
        /// </summary>
        [DataField]
        public int TotalDeaths { get; set; }

        /// <summary>
        /// Random seed this save was initialized with.
        /// </summary>
        [DataField]
        public int RandomSeed { get; set; }

        /// <summary>
        /// The deepest dungeon level the player has traversed to.
        /// </summary>
        [DataField]
        public int DeepestLevel { get; set; }

        /// <summary>
        /// Date the player last entered the world map.
        /// </summary>
        [DataField]
        public GameDateTime TravelStartDate { get; set; } = GameDateTime.Zero;

        /// <summary>
        /// Total distance traveled on the world map so far. Reset when entering
        /// a new travel destination map (town/guild).
        /// </summary>
        [DataField]
        public int TravelDistance { get; set; }

        [DataField]
        public string? LastDepartedMapName { get; set; }

        [DataField]
        public bool IsFireGiantReleased { get; set; }
    }
}
