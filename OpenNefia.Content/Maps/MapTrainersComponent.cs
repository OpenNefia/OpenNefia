using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    public class MapTrainersComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTrainers";

        [DataField]
        public HashSet<PrototypeId<SkillPrototype>> TrainedSkills { get; } = new();
    }
}
