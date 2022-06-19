using OpenNefia.Core.GameObjects;
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
    }

    public sealed class ActivitySystem : EntitySystem, IActivitySystem
    {
        public void InterruptUsing(EntityUid offeringItem)
        {
            throw new NotImplementedException();
        }
    }
}
