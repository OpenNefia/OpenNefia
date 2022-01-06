using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    internal class EntityManagerInternal : EntityManager, IEntityManagerInternal
    {
        EntityUid IEntityManagerInternal.AllocEntity(PrototypeId<EntityPrototype>? prototypeName, EntityUid? uid)
        {
            return AllocEntity(prototypeName, uid);
        }

        void IEntityManagerInternal.FinishEntityLoad(EntityUid entity, IEntityLoadContext? context)
        {
            LoadEntity(entity, context);
        }

        void IEntityManagerInternal.FinishEntityInitialization(EntityUid entity)
        {
            InitializeEntity(entity);
        }

        void IEntityManagerInternal.FinishEntityStartup(EntityUid entity)
        {
            StartEntity(entity);
        }
    }
}
