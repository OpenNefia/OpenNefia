using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Food;
using OpenNefia.Core.Game;
using OpenNefia.Content.Resists;
using OpenNefia.Content.UI;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Spells;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Logic;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.Enchantments
{
    public sealed partial class VanillaEnchantmentsSystem
    {
        private void Initialize_Unique()
        {
            SubscribeComponent<EncRandomTeleportComponent, CalcEnchantmentAdjustedPowerEvent>(EncRandomTeleport_CalcAdjustedPower);
            SubscribeComponent<EncRandomTeleportComponent, ApplyEnchantmentOnRefreshEvent>(EncRandomTeleport_ApplyOnRefresh);
            SubscribeComponent<EncRandomTeleportComponent, ApplyEnchantmentAfterPassTurnEvent>(EncRandomTeleport_ApplyAfterPassTurn);
        }
    }
}
