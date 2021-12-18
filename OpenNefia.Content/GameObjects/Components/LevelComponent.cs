using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class LevelComponent : Component
    {
        public override string Name => "Level";

        [DataField(required: true)]
        public int Level { get; set; }

        [DataField]
        public int Experience { get; set; }

        [DataField]
        public int ExperienceToNext { get; set; }
    }
}
