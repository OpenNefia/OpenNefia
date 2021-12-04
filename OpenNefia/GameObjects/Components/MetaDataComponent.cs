using System;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     Contains meta data about this entity that isn't component specific.
    /// </summary>
    public class MetaDataComponent : Component
    {
        public override string Name => "MetaData";

        /// <summary>
        ///     The in-game name of this entity.
        /// </summary>
        [DataField]
        [Localize]
        public string? DisplayName;

        private EntityPrototype? _entityPrototype;

        /// <summary>
        ///     The prototype this entity was created from, if any.
        /// </summary>
        public EntityPrototype? EntityPrototype
        {
            get => _entityPrototype;
            set
            {
                _entityPrototype = value;
            }
        }

        /// <summary>
        ///     The current lifetime stage of this entity. You can use this to check
        ///     if the entity is initialized or being deleted.
        /// </summary>
        public EntityLifeStage EntityLifeStage { get; internal set; }

        public bool EntityInitialized => EntityLifeStage >= EntityLifeStage.Initialized;
        public bool EntityInitializing => EntityLifeStage == EntityLifeStage.Initializing;
        public bool EntityDeleted => EntityLifeStage >= EntityLifeStage.Deleted;

        /// <summary>
        /// Liveness of the entity for validity purposes.
        /// 
        /// As an example, if a character is killed it will still remain in
        /// the map as a DeadAndBuried entity, for example if something wants to
        /// cast Resurrect on it. If the character is an ally, it will become
        /// Hidden when killed instead, so it doesn't get permanently removed
        /// when the map is changed.
        /// 
        /// A stackable entity with amount 0 counts as DeadAndBuried.
        /// </summary>
        public EntityGameLiveness Liveness = EntityGameLiveness.Alive;

        public bool IsAliveAndPrimary => Liveness == EntityGameLiveness.Alive && !EntityDeleted;
        public bool IsAlive => (Liveness == EntityGameLiveness.Alive || Liveness == EntityGameLiveness.AliveSecondary) && !EntityDeleted;
        public bool IsDeadAndBuried => Liveness == EntityGameLiveness.DeadAndBuried || EntityDeleted;
    }

    public enum EntityGameLiveness
    {
        /// <summary>
        /// This entity is active in the map.
        /// </summary>
        Alive,

        /// <summary>
        /// This entity is active, but should not be considered the "primary" entity
        /// on the tile (riding/tag team partners).
        /// </summary>
        AliveSecondary,

        /// <summary>
        /// This entity is not active, but should not be removed from the map.
        /// </summary>
        Hidden,

        /// <summary>
        /// This entity should be removed from the map when certain actions are performed
        /// (enumerating a container, changing maps).
        /// </summary>
        DeadAndBuried
    }
}
