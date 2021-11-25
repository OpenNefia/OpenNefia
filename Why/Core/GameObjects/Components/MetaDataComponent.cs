using System;
using Why.Core.Prototypes;
using Why.Core.Serialization.Manager.Attributes;

namespace Why.Core.GameObjects
{
    /// <summary>
    ///     Contains meta data about this entity that isn't component specific.
    /// </summary>
    public class MetaDataComponent : Component
    {
        [DataField("name")]
        private string? _entityName;
        [DataField("desc")]
        private string? _entityDescription;
        private EntityPrototype? _entityPrototype;
        private bool _entityPaused;

        /// <summary>
        ///     The in-game name of this entity.
        /// </summary>
        public string EntityName
        {
            get
            {
                if (_entityName == null)
                    return _entityPrototype != null ? _entityPrototype.Name : string.Empty;
                return _entityName;
            }
            set
            {
                string? newValue = value;
                if (_entityPrototype != null && _entityPrototype.Name == newValue)
                    newValue = null;

                if (_entityName == newValue)
                    return;

                _entityName = newValue;
            }
        }

        /// <summary>
        ///     The in-game description of this entity.
        /// </summary>
        public string EntityDescription
        {
            get
            {
                if (_entityDescription == null)
                    return _entityPrototype != null ? _entityPrototype.Description : string.Empty;
                return _entityDescription;
            }
            set
            {
                string? newValue = value;
                if (_entityPrototype != null && _entityPrototype.Description == newValue)
                    newValue = null;

                if(_entityDescription == newValue)
                    return;

                _entityDescription = newValue;
            }
        }

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
