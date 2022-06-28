﻿using OpenNefia.Core.GameObjects;
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
            SubscribeComponent<MetaDataComponent, EntityMapInitEvent>(OnMapInit, priority: EventPriorities.VeryLow);
        }

        public void Refresh(EntityUid entity)
        {
            var ev = new EntityRefreshEvent();
            RaiseEvent(entity, ref ev);
        }

        private void OnMapInit(EntityUid uid, MetaDataComponent component, ref EntityMapInitEvent args)
        {
            Refresh(uid);
        }
    }

    [ByRefEvent]
    public struct EntityRefreshEvent
    {
    }
}
