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
        public void Add(Action fn);
    }

    public sealed class DeferredEventsSystem : EntitySystem, IDeferredEventsSystem
    {
        public void Add(Action fn)
        {
            // TODO
        }
    }
}
