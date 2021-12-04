using OpenNefia.Core.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class PotionPuddleComponent : Component
    {
        public override string Name => "PotionPuddle";

        [DataField]
        public EffectArgs Args = new();

        [DataField]
        public IEffect? Effect;
    }
}
