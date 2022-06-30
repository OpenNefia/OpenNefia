using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Activity
{
    public interface IActivitySystem : IEntitySystem
    {
        void InterruptUsing(EntityUid offeringItem);
        void RemoveActivity(EntityUid entity);
        bool TryGetActivity(EntityUid target, [NotNullWhen(true)] out ActivityComponent? activityComp);
        bool HasActivity(EntityUid entity);
    }

    public sealed class ActivitySystem : EntitySystem, IActivitySystem
    {
        [Dependency] protected readonly ISlotSystem _slots = default!;

        public void InterruptUsing(EntityUid offeringItem)
        {
        }

        public void RemoveActivity(EntityUid entity)
        {
            if (!TryComp<ActivityComponent>(entity, out var activityComp))
                return;

            if (activityComp.SlotID == null)
                return;
            
            if (_slots.HasSlot(entity, activityComp.SlotID.Value))
            {
                _slots.RemoveSlot(entity, activityComp.SlotID.Value);
            }
            else
            {
                Logger.WarningS("activity", $"Missing slot {activityComp.SlotID} on entity {entity}'s activity!");
            }

            activityComp.SlotID = null;
        }

        public bool TryGetActivity(EntityUid target, [NotNullWhen(true)] out ActivityComponent? activityComp)
        {
            if (!TryComp<ActivityComponent>(target, out activityComp))
                return false;

            if (activityComp.SlotID == null || !_slots.HasSlot(target, activityComp.SlotID.Value))
            {
                Logger.WarningS("activity", $"Pruning dead activity with slot {activityComp.SlotID} on entity {target}");
                RemoveActivity(target);
                activityComp = null;
                return false;
            }

            return true;
        }

        public bool HasActivity(EntityUid entity)
        {
            return TryGetActivity(entity, out _);
        }
    }
}
