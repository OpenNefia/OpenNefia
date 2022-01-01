using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.EntityGen
{
    /// <summary>
    /// Wraps <see cref="IEntityManager"/>'s spawning functionality with additional event
    /// hooks for initializing entities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is necessary since <see cref="IEntityManager.SpawnEntity"/> is general-purpose
    /// and gets used for instantiating entities loaded from a save game. On the contrary,
    /// some events should only be run the very first time the entity is spawned, and not when
    /// they're loaded from a save.
    /// </para>
    /// </remarks>
    public interface IEntityGen : IEntitySystem
    {
        Entity? SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates);
        Entity? SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates);
    }

    public class EntityGenSystem : EntitySystem, IEntityGen
    {
        [Dependency] private readonly IMapBlueprintLoader _blueprintLoader = default!;

        public override void Initialize()
        {
            _blueprintLoader.OnBlueprintEntityStartup += HandleBlueprintEntityStartup;

            SubscribeLocalEvent<SpatialComponent, EntityCloneFinishedEventArgs>(HandleClone, nameof(HandleClone));
        }

        private void FireGeneratedEvent(EntityUid entity)
        {
            var ev = new EntityGeneratedEvent();
            RaiseLocalEvent(entity, ref ev);
        }

        /// <summary>
        /// Runs entity generation events for entities loaded from blueprints.
        /// </summary>
        private void HandleBlueprintEntityStartup(EntityUid entity)
        {
            FireGeneratedEvent(entity);
        }

        /// <summary>
        /// Runs entity generation events for all cloned entities.
        /// </summary>
        private void HandleClone(EntityUid entity, SpatialComponent component, EntityCloneFinishedEventArgs args)
        {
            FireGeneratedEvent(entity);
        }

        public Entity? SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates)
        {
            if (!coordinates.IsValid(EntityManager))
                return null;

            return SpawnEntity(protoId, coordinates.ToMap(EntityManager));
        }

        public Entity? SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates)
        {
            var ent = EntityManager.SpawnEntity(protoId, coordinates);
            if (ent == null)
            {
                return null;
            }

            FireGeneratedEvent(ent.Uid);

            if (!EntityManager.IsAlive(ent.Uid))
            {
                Logger.WarningS("entity.gen", $"Entity {ent.Uid} became invalid after {nameof(EntityGeneratedEvent)} was fired.");
                return null;
            }

            return ent;
        }
    }

    public struct EntityGeneratedEvent
    {
    }
}
