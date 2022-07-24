using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.DeferredEvents
{
    public interface IDeferredEventsSystem : IEntitySystem
    {
        void Add(Action fn);
        bool IsEventQueued();
    }

    public sealed class DeferredEventsSystem : EntitySystem, IDeferredEventsSystem
    {
        public void Add(Action fn)
        {
            // TODO
        }

        public bool IsEventQueued()
        {
            // TODO
            return false;
        }
    }
}
