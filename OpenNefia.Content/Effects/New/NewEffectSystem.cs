using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.Effects.New
{
    /// <summary>
    /// New implementation of the effect system. This is an ECS-based version based
    /// around combining components:
    /// 
    /// - All effects are entities with a <see cref="EffectComponent"/>.
    /// - Effects must have an "effect type" such as <see cref="EffectTypeMagicComponent"/>
    ///   or <see cref="EffectTypeActionComponent"/>. These determine the resource costs 
    ///   of the effect.
    /// - You can customize how the effect is applied and its range with an "effect area"
    ///   component like <see cref="EffectAreaBallComponent"/> for AoE or just 
    ///   <see cref="EffectAreaDirectComponent"/> to skip any special targeting logic.
    /// - Effect area components should raise <see cref="ApplyEffectDamageEvent"/>, which applies
    ///   the effect to the target. In turn, "effect damage" components will listen for 
    ///   this event to apply the actual effect. These include
    ///   <see cref="EffectDamageElementalComponent"/> for elemental damage, among others.
    /// </summary>
    public interface INewEffectSystem : IEntitySystem
    {
        /// <summary>
        /// Applies an effect.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="effectID"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        TurnResult Apply(EntityUid source, EntityUid? target, PrototypeId<EntityPrototype> effectID, EffectArgSet args);
    }

    public sealed class NewEffectSystem : EntitySystem, INewEffectSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;

        public override void Initialize()
        {
        }

        public TurnResult Apply(EntityUid source, EntityUid? target, PrototypeId<EntityPrototype> effectID, EffectArgSet args)
        {
            var effectEntity = _entityGen.SpawnEntity(effectID, MapCoordinates.Global);
            if (!IsAlive(effectEntity))
            {
                Logger.ErrorS("effect", $"Failed to cast event {effectID}, entity could not be spawned");
                if (effectEntity != null)
                    EntityManager.DeleteEntity(effectEntity.Value);
                return TurnResult.Aborted;
            }

            var ev = new CastEffectEvent(source, target, args);
            RaiseEvent(effectEntity.Value, ev);

            if (IsAlive(effectEntity))
                EntityManager.DeleteEntity(effectEntity.Value);

            return ev.TurnResult;
        }
    }

    /// <summary>
    /// Called when an effect is cast. This event should delegate to a system that
    /// handles targeting and stat checks (MP/stamina/etc.) before raising 
    /// <see cref="ApplyEffectAreaEvent"/> internally. 
    /// Examples are the magic and action entity systems.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class CastEffectEvent : TurnResultEntityEventArgs
    {
        public EntityUid Source { get; }
        public EntityUid? Target { get; }
        public EffectArgSet Args { get; }

        public CastEffectEvent(EntityUid source, EntityUid? target, EffectArgSet args)
        {
            Source = source;
            Target = target;
            Args = args;
        }
    }

    /// <summary>
    /// Raised to retrieve the primary target or location of this effect,
    /// if it was <c>null</c>.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class GetEffectTargetEvent : CancellableEntityEventArgs
    {
        public EntityUid Source { get; }
        public EffectArgSet Args { get; }

        public EntityUid? OutTarget { get; set; } = null;
        public EntityCoordinates? OutCoords { get; set; } = null;

        public GetEffectTargetEvent(EntityUid source, EffectArgSet args)
        {
            Source = source;
            Args = args;
        }
    }

    public interface IApplyEffectEvent
    {
        public EntityCoordinates SourceCoords { get; }
        public EntityCoordinates TargetCoords { get; }

        public MapCoordinates SourceCoordsMap { get; }
        public MapCoordinates TargetCoordsMap { get; }

        public IMap Map { get; }
        public IMap TargetMap { get; }
    }

    /// <summary>
    /// Runs the actual logic of the event after checks like MP/stamina/targeting
    /// have been completed. Will run area-based logic for calculating effect targets
    /// like ball AoE and bolt piercing.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class ApplyEffectAreaEvent : TurnResultEntityEventArgs, IApplyEffectEvent
    {
        /// <summary>
        /// Person who casted the event.
        /// </summary>
        public EntityUid Source { get; }

        /// <summary>
        /// May be set to <see cref="Source"/> if no target is available.
        /// </summary>
        public EntityUid Target { get; }

        /// <summary>
        /// Automatically set to the location of <see cref="Target"/> if
        /// not set manually.
        /// </summary>
        public EntityCoordinates SourceCoords { get; }
        public EntityCoordinates TargetCoords { get; }

        public MapCoordinates SourceCoordsMap => SourceCoords.ToMap(IoCManager.Resolve<IEntityManager>());
        public MapCoordinates TargetCoordsMap => TargetCoords.ToMap(IoCManager.Resolve<IEntityManager>());

        public IMap Map => IoCManager.Resolve<IMapManager>().GetMap(SourceCoordsMap.MapId);
        public IMap TargetMap => IoCManager.Resolve<IMapManager>().GetMap(TargetCoordsMap.MapId);

        public EffectArgSet Args { get; }

        public ApplyEffectAreaEvent(EntityUid source, EntityUid target, EntityCoordinates sourceCoords, EntityCoordinates targetCoords, EffectArgSet args)
        {
            Source = source;
            Target = target;
            SourceCoords = sourceCoords;
            TargetCoords = targetCoords;
            Args = args;
        }
    }

    /// <summary>
    /// Applies effect tile damage, like fire burning items on the ground.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class ApplyEffectTileDamageEvent : HandledEntityEventArgs
    {
        /// <summary>
        /// Person who casted the event.
        /// </summary>
        public EntityUid Source { get; }

        /// <summary>
        /// Automatically set to the location of <see cref="InnerTarget"/> if
        /// not set manually.
        /// </summary>
        public EntityCoordinates Coords { get; }

        public MapCoordinates CoordsMap => Coords.ToMap(IoCManager.Resolve<IEntityManager>());

        public IMap Map => IoCManager.Resolve<IMapManager>().GetMap(CoordsMap.MapId);

        public EffectArgSet Args { get; }

        public ApplyEffectTileDamageEvent(EntityUid source, EntityCoordinates coords, EffectArgSet args)
        {
            Source = source;
            Coords = coords;
            Args = args;
        }
    }

    /// <summary>
    /// Applies the effect to a single target. 
    /// Raised once for each entity affected by an AoE.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class ApplyEffectDamageEvent : CancellableEntityEventArgs, IApplyEffectEvent
    {
        /// <summary>
        /// Person who casted the event.
        /// </summary>
        public EntityUid Source { get; }

        /// <summary>
        /// Target of the effect. May be different from the original target
        /// in the case of AoE.
        /// </summary>
        public EntityUid InnerTarget { get; }

        /// <summary>
        /// Automatically set to the location of <see cref="InnerTarget"/> if
        /// not set manually.
        /// </summary>
        public EntityCoordinates SourceCoords { get; }
        public EntityCoordinates TargetCoords { get; }

        public MapCoordinates SourceCoordsMap => SourceCoords.ToMap(IoCManager.Resolve<IEntityManager>());
        public MapCoordinates TargetCoordsMap => TargetCoords.ToMap(IoCManager.Resolve<IEntityManager>());

        public IMap Map => IoCManager.Resolve<IMapManager>().GetMap(SourceCoordsMap.MapId);
        public IMap TargetMap => IoCManager.Resolve<IMapManager>().GetMap(TargetCoordsMap.MapId);

        public EffectArgSet Args { get; }

        /// <summary>
        /// A damage property for calculating damage somewhere in the effect chain.
        /// </summary>
        public int OutDamage { get; set; } = 0;

        public ApplyEffectDamageEvent(EntityUid source, EntityUid target, EntityCoordinates sourceCoords, EntityCoordinates targetCoords, EffectArgSet args)
        {
            Source = source;
            InnerTarget = target;
            SourceCoords = sourceCoords;
            TargetCoords = targetCoords;
            Args = args;
        }
    }
}