using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Levels
{
    [RegisterComponent]
    public class LevelComponent : Component
    {
        [DataField]
        public int Level { get; set; } = 1;

        [DataField]
        public int Experience { get; set; }

        [DataField]
        public int ExperienceToNext { get; set; }

        [DataField]
        public int MaxLevelReached { get; set; } = 1;

        [DataField]
        public bool ShowLevelInName { get; set; } = false;
    }
}
