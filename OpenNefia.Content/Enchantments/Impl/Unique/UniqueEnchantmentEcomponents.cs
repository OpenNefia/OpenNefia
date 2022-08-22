using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Enchantments
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncRandomTeleportComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncRandomTeleport";

        public string? Description => Loc.GetString("Elona.Enchantment.Item.RandomTeleport");
    }
}
