using OpenNefia.Content.EntityGen;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public interface IRefreshSystem : IEntitySystem
    {
        void Refresh(EntityUid entity);
    }

    public class RefreshSystem : EntitySystem, IRefreshSystem
    {
        public override void Initialize()
        {
            SubscribeEntity<EntityMapInitEvent>(OnMapInit, priority: EventPriorities.VeryLow);
            SubscribeEntity<EntityGeneratedEvent>(OnGenerated, priority: EventPriorities.VeryLow);
        }

        public void Refresh(EntityUid entity)
        {
            var ev = new EntityRefreshEvent();
            RaiseEvent(entity, ref ev);
        }

        private void OnMapInit(EntityUid uid, ref EntityMapInitEvent args)
        {
            Refresh(uid);
        }

        private void OnGenerated(EntityUid uid, ref EntityGeneratedEvent args)
        {
            Refresh(uid);
        }
    }

    [ByRefEvent]
    public struct EntityRefreshEvent
    {
    }
}
