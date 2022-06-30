using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.EmotionIcon
{
    [RegisterComponent]
    public sealed class EmotionIconComponent : Component
    {
        public override string Name => "EmotionIcon";

        [DataField]
        public string? EmotionIconId { get; set; }

        [DataField]
        public int TurnsRemaining { get; set; }
    }
}
