using OpenNefia.Content.Effects;
using OpenNefia.Content.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CurseStates
{
    [RegisterComponent]
    public class CurseStateComponent : Component
    {
        public override string Name => "CurseState";

        [DataField]
        public CurseState CurseState { get; set; } = CurseState.Normal;

        /// <summary>
        /// If true, do not modify <see cref="CurseState"/> when the entity is being generated.
        /// </summary>
        [DataField]
        public bool NoRandomizeCurseState { get; set; } = false;
    }

    public enum CurseState
    {
        Blessed = 1,
        Normal = 0,
        Cursed = -1,
        Doomed = -2
    }
}
