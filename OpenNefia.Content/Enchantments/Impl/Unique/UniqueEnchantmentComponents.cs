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
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncSuckBloodComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncSuckBlood";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncSuckExperienceComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncSuckExperience";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncSummonCreatureComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncSummonCreature";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncPreventTeleportComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncPreventTeleport";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncResistBlindness : Component, IEnchantmentComponent
    {
        public override string Name => "EncResistBlindness";
    }
}
