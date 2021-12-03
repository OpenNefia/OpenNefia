using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     Contains information about the entity's position in a map.
    /// </summary>
    public class SpatialComponent : Component
    {
        public override string Name => "Spatial";

        /// <summary>
        /// Position of this entity on the map.
        /// </summary>
        private Vector2i _pos;
        public Vector2i Pos { 
            get => _pos; 
            set
            {
                var oldPos = _pos;

                _pos = value;

                if (oldPos != _pos)
                {
                    var ev = new PositionChangedEvent(new MapCoordinates(Map, oldPos), new MapCoordinates(Map, _pos));
                    Owner.EntityManager.EventBus.RaiseLocalEvent(OwnerUid, ref ev);
                }
            }
        }

        /// <summary>
        /// Map this entity is in.
        /// </summary>
        public IMap? Map { get; private set; }

        public MapCoordinates Coords => new MapCoordinates(Map, Pos);

        /// <summary>
        /// If true, this entity cannot be moved over. This also causes
        /// collision events to be fired when the entity is moved into.
        /// </summary>
        [DataField]
        public bool IsSolid { get; set; }

        /// <summary>
        /// If true, this entity blocks field of view.
        /// </summary>
        [DataField]
        public bool IsOpaque { get; set; }

        [DataField]
        public Direction Direction { get; set; } = Direction.North;

        internal void ChangeMap(IMap? newMap)
        {
            if (newMap == Map)
                return;

            var oldMap = Map;

            Map = newMap;
        }
    }
}
