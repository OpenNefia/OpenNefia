using System;
using Why.Core.Prototypes;

namespace Why.Core.GameObjects
{
    /// <summary>
    ///     Contains meta data about this entity that isn't component specific.
    /// </summary>
    [RegisterComponent]
    public class MetaDataComponent : Component
    {
        public override string Name => "MetaData";

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
    }
}
