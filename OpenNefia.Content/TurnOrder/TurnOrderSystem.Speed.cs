using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.TurnOrder
{
    public sealed partial class TurnOrderSystem
    {
        public const int MinSpeed = 10;
        
        /// <inheritdoc/>
        public int CalculateSpeed(EntityUid entity, TurnOrderComponent? turnOrder = null)
        {
            if (!Resolve(entity, ref turnOrder))
                return MinSpeed;

            // TODO calculate speed

            return (int)MathF.Max(turnOrder.CurrentSpeed * turnOrder.CurrentSpeedModifier, MinSpeed);
        }

        public void RefreshSpeed(EntityUid entity, TurnOrderComponent? turnOrder = null)
        {
            if (!Resolve(entity, ref turnOrder))
                return;

            var ev = new EntityRefreshSpeedEvent();
            RaiseEvent(entity, ref ev);

            turnOrder.CurrentSpeed = ev.OutSpeed;
            turnOrder.CurrentSpeedModifier = ev.OutSpeedModifier;
        }
    }

    [ByRefEvent]
    [EventUsage(EventTarget.Normal)]
    public struct EntityRefreshSpeedEvent
    {
        public int OutSpeed { get; set; } = TurnOrderSystem.MinSpeed;
        public float OutSpeedModifier { get; set; } = 1f;

        public EntityRefreshSpeedEvent()
        {
        }
    }
}
