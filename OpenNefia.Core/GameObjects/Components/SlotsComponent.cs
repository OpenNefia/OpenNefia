using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    [RegisterComponent]
    public class SlotsComponent : Component
    {
        public override string Name => "Slots";

        /// <summary>
        /// Maximum free slot ID for adding new slots.
        /// </summary>
        [DataField]
        public SlotId MaxSlotId { get; internal set; } = new SlotId(1);

        [DataField("registrations")]
        internal readonly Dictionary<SlotId, SlotRegistration> _registrations = new();

        /// <summary>
        /// Slots registered on this entity.
        /// </summary>
        public IReadOnlyDictionary<SlotId, SlotRegistration> Registrations => _registrations;

        public bool HasAnySlotsWithComp(Type compType)
        {
            return _registrations.Values.Any(reg => reg.CompTypes.Contains(compType));
        }
    }

    [DataDefinition]
    public class SlotRegistration
    {
        /// <summary>
        /// ID of this registration.
        /// </summary>
        [DataField]
        public SlotId Id { get; }

        [DataField("compTypes")]
        internal readonly HashSet<Type> _compTypes;

        /// <summary>
        /// The types of components this registration initially added.
        /// </summary>
        /// <remarks>
        /// These must implement <see cref="IComponent"/>.
        /// </remarks>
        public IReadOnlySet<Type> CompTypes => _compTypes;

        public SlotRegistration(SlotId id, HashSet<Type> compTypes)
        {
            Id = id;
            _compTypes = compTypes;
        }
    }
}
