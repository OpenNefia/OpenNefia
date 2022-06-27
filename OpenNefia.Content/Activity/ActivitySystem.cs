using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Activity
{
    public interface IActivitySystem : IEntitySystem
    {
        void InterruptUsing(EntityUid offeringItem);
        void RemoveActivity(EntityUid entity);
    }

    public sealed class ActivitySystem : EntitySystem, IActivitySystem
    {
        [Dependency] protected readonly ISlotSystem _slots = default!;

        public void InterruptUsing(EntityUid offeringItem)
        {
        }

        public void RemoveActivity(EntityUid entity)
        {
        }
    }
}
