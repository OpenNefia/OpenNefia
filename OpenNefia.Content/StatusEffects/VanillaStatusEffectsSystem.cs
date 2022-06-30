using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.TurnOrder;

namespace OpenNefia.Content.StatusEffects
{
    public sealed class VanillaStatusEffectsSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeComponent<StatusDrunkComponent, EntityPassTurnEventArgs>(PickDrunkardFight);
        }

        private void PickDrunkardFight(EntityUid uid, StatusDrunkComponent component, EntityPassTurnEventArgs args)
        {
        }
    }
}
