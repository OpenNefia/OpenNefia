using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.PCCs
{
    /// <summary>
    /// Holds PCC data that is not tied to renderer state.
    /// </summary>
    [RegisterComponent]
    public sealed class PCCComponent : Component
    {
        public override string Name => "PCC";

        [DataField("usePCC")]
        public bool UsePCC { get; set; } = false;

        /// <summary>
        /// List of PCC parts. Be sure to call <see cref="IPCCSystem.RebakePCCImage(EntityUid, PCCComponent?)"/>
        /// if you modify this.
        /// </summary>
        [DataField("pccParts")]
        public Dictionary<string, PCCPart> PCCParts { get; } = new();

        /// <summary>
        /// Direction the PCC is facing.
        /// </summary>
        [DataField("pccDirection")]
        public PCCDirection PCCDirection { get; set; }
    }

    public enum PCCDirection
    {
        South = 0,
        West = 1,
        North = 2,
        East = 3
    }
}