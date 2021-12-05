using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    internal class EntityManagerInternal : EntityManager, IEntityManagerInternal
    {
        Entity IEntityManagerInternal.AllocEntity(string? prototypeName, EntityUid? uid)
        {
            return AllocEntity(prototypeName, uid);
        }

        void IEntityManagerInternal.FinishEntityLoad(Entity entity, IEntityLoadContext? context)
        {
            LoadEntity(entity, context);
        }

        void IEntityManagerInternal.FinishEntityInitialization(Entity entity)
        {
            InitializeEntity(entity);
        }

        void IEntityManagerInternal.FinishEntityStartup(Entity entity)
        {
            StartEntity(entity);
        }
    }
}
