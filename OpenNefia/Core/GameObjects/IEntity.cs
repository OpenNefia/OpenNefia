using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.GameObjects
{
    public interface IEntity
    {
        /// <summary>
        /// The Entity Manager that controls this entity.
        /// </summary>
        IEntityManager EntityManager { get; }

        /// <summary>
        /// The unique ID of this entity.
        /// </summary>
        EntityUid Uid { get; }

        /// <summary>
        /// Position of this entity on the map.
        /// </summary>
        public Vector2i Pos { get; set; }

        /// <summary>
        /// Map this entity is in.
        /// </summary>
        public IMap? Map { get; }

        /// <summary>
        ///     The current lifetime stage of this entity. You can use this to check
        ///     if the entity is initialized or being deleted.
        /// </summary>
        EntityLifeStage LifeStage { get; internal set; }

        /// <summary>
        ///     The prototype that was used to create this entity.
        /// </summary>
        EntityPrototype? Prototype { get; }

        /// <summary>
        /// Position of this entity on the map.
        /// </summary>
        MapCoordinates Coords { get; }

        /// <summary>
        ///     The Transform Component of this entity.
        /// </summary>
        SpatialComponent Spatial { get; }

        /// <summary>
        ///     The MetaData Component of this entity.
        /// </summary>
        MetaDataComponent MetaData { get; }

        /// <summary>
        ///     Whether this entity has fully initialized.
        /// </summary>
        bool Initialized { get; }

        bool Initializing { get; }

        /// <summary>
        ///     True if the entity has been deleted.
        /// </summary>
        bool Deleted { get; }

        /// <summary>
        ///     Determines if this entity is still valid.
        /// </summary>
        /// <returns>True if this entity is still valid.</returns>
        bool IsValid();
    }
}
