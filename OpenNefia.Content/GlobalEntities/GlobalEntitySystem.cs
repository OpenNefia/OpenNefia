using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Stayers;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.GlobalEntities
{
    /// <summary>
    /// A repository of entities that can be assigned a string ID and stored globally.
    /// If you need to create an entity container that must be globally available,
    /// use this system.
    /// </summary>
    public interface IGlobalEntitySystem : IEntitySystem
    {
        EntityUid EnsureGlobalEntity(string id, PrototypeId<EntityPrototype>? protoID);
    }

    [DataDefinition]
    public sealed class GlobalEntitiesState
    {
        [DataField]
        public EntityUid ContainerEntity { get; set; } = EntityUid.Invalid;

        [DataField]
        public Dictionary<string, EntityUid> IdToEntity { get; set; } = new();
    }

    public sealed class GlobalEntitySystem : EntitySystem, IGlobalEntitySystem
    {
        [Dependency] private readonly IEntityGen _entityGen = default!;

        [RegisterSaveData("Elona.GlobalEntitiesSystem.State")]
        private GlobalEntitiesState State { get; set; } = new();

        public override void Initialize()
        {
            SubscribeBroadcast<GameInitiallyLoadedEventArgs>(EnsureContainer);
            SubscribeEntity<BeforeEntityDeletedEvent>(RemoveMappedEntity);
        }

        private void EnsureContainer(GameInitiallyLoadedEventArgs ev)
        {
            EnsureContainer();
        }

        private Container EnsureContainer()
        {
            if (!IsAlive(State.ContainerEntity))
            {
                Logger.WarningS("globalEntities", "Creating global container entity");
                State.ContainerEntity = EntityManager.SpawnEntity(null, MapCoordinates.Global);
                DebugTools.Assert(IsAlive(State.ContainerEntity), "Could not initialize stayers container!");
            }
            return EnsureComp<GlobalEntitiesComponent>(State.ContainerEntity).Container;
        }

        private void RemoveMappedEntity(EntityUid uid, ref BeforeEntityDeletedEvent args)
        {
            if (TryComp<GlobalEntityComponent>(uid, out var globalEntity))
                State.IdToEntity.Remove(globalEntity.ID);
        }

        public EntityUid EnsureGlobalEntity(string id, PrototypeId<EntityPrototype>? protoID)
        {
            var container = EnsureContainer();

            if (State.IdToEntity.TryGetValue(id, out var ent))
            {
                if (!EntityManager.EntityExists(ent))
                {
                    Logger.WarningS("globalEntities", $"Removing dead global entity: {ent}");
                    State.IdToEntity.Remove(id);
                }
                else
                {
                    return ent;
                }
            }

            var result = _entityGen.SpawnEntity(protoID, container);
            DebugTools.Assert(IsAlive(result));

            State.IdToEntity[id] = result!.Value;

            return result.Value;
        }
    }
}