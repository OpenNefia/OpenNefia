using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Buffs
{
    public interface IBuffsSystem : IEntitySystem
    {
        void RemoveAllBuffs(EntityUid entity, BuffsComponent? buffs = null);
    }

    public sealed class BuffsSystem : EntitySystem, IBuffsSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        
        public override void Initialize()
        {
            SubscribeComponent<BuffsComponent, EntityRefreshEvent>(ApplyBuffs, priority: EventPriorities.High);
        }

        private void ApplyBuffs(EntityUid uid, BuffsComponent component, ref EntityRefreshEvent args)
        {
            foreach (var buff in component.Buffs)
            {
                if (buff.TurnsRemaining > 0)
                {
                    var ev = new P_BuffOnEntityRefreshEvent(uid, buff);
                    _protos.EventBus.RaiseEvent(buff.BuffID, ev);
                }
            }
        }

        public void RemoveAllBuffs(EntityUid entity, BuffsComponent? buffs = null)
        {
            if (!Resolve(entity, ref buffs))
                return;

            // TODO
        }
    }

    [PrototypeEvent(typeof(BuffPrototype))]
    public sealed class P_BuffOnEntityRefreshEvent
    {
        public EntityUid Entity { get; set; }
        public BuffInstance Buff { get; set; }

        public P_BuffOnEntityRefreshEvent(EntityUid entity, BuffInstance buff)
        {
            Entity = entity;
            Buff = buff;
        }
    }
}
