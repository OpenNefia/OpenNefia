using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.LecchoTorte.QuickStart
{
    [DataDefinition]
    public sealed class QuickstartItem
    {
        [DataField(required: true)]
        public PrototypeId<EntityPrototype> ID { get; }

        [DataField]
        public int Amount { get; } = 1;

        [DataField]
        public QuickstartEntityLocation Location { get; } = QuickstartEntityLocation.Inventory;

        [DataField]
        public ComponentRegistry Components { get; } = new();
    }

    public enum QuickstartEntityLocation 
    {
        Inventory,
        Equipment,
        Ground,
    }
}
