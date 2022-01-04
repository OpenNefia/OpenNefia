using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.GameObjects
{
    internal interface IEntityManagerInternal : IEntityManager
    {
        // These methods are used by the map loader to do multi-stage entity construction during map load.
        // I would recommend you refer to the MapLoader for usage.

        Entity AllocEntity(PrototypeId<EntityPrototype>? prototypeName, EntityUid? uid = null);

        void FinishEntityLoad(Entity entity, IEntityLoadContext? context = null);

        void FinishEntityInitialization(Entity entity);

        void FinishEntityStartup(Entity entity);

        void FlushEntities();

        /// <summary>
        /// The next valid entity UID to generate.
        /// </summary>
        /// <remarks>
        /// This should **only** be set when handling game saving/loading.
        /// </remarks>
        public int NextEntityUid { get; set; }
    }
}
