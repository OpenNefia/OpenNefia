using OpenNefia.Content.Effects;
using OpenNefia.Content.Effects;
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
    public class CurseStateComponent : Component
    {
        public override string Name => "CurseState";

        [DataField]
        public CurseState CurseState { get; set; } = CurseState.Normal;
    }

    public enum CurseState
    {
        Blessed = 1,
        Normal = 0,
        Cursed = -1,
        Doomed = -2
    }
}
