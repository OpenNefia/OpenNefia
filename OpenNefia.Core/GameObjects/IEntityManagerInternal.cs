using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.GameObjects
{
    internal interface IEntityManagerInternal : IEntityManager
    {
        // These methods are used by the map loader to do multi-stage entity construction during map load.
        // I would recommend you refer to the MapLoader for usage.

        EntityUid AllocEntity(PrototypeId<EntityPrototype>? prototypeName, EntityUid? uid = null);

        void FinishEntityLoad(EntityUid entity, IEntityLoadContext? context = null);

        void FinishEntityInitialization(EntityUid entity);

        void FinishEntityStartup(EntityUid entity);

        void FlushEntities(EntityDeleteType deleteType = EntityDeleteType.Delete);

        /// <summary>
        /// The next valid entity UID to generate.
        /// </summary>
        /// <remarks>
        /// This should **only** be set when handling game saving/loading.
        /// </remarks>
        public int NextEntityUid { get; set; }
    }
}
