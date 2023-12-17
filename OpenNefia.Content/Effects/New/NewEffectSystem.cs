﻿using OpenNefia.Content.Logic;
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
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Skills;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Effects.New
{
    /// <summary>
    /// New implementation of the effect system. This is an ECS-based version designed
    /// around combining components:
    /// - All effects are entities with an <see cref="EffectComponent"/>.
    /// - Effects must have an "effect type" such as <see cref="EffectTypeSpellComponent"/>
    ///   or <see cref="EffectTypeActionComponent"/>. These determine the resource costs 
    ///   of the effect.
    /// - Effects should subscribe to <see cref="GetEffectPlayerTargetEvent"/> so they can
    ///   provide a default target if none is provided.
    ///   The <see cref="EffectTargetOtherComponent"/> targets an entity or the ground,
    ///   and <see cref="EffectTargetDirectionComponent"/> targets a cardinal direction.
    /// - You can customize how the effect is applied and its range with an "effect area"
    ///   component like <see cref="EffectAreaBallComponent"/> for AoE or just 
    ///   omit all area components to skip area targeting.
    /// - Effect area components should raise <see cref="ApplyEffectDamageEvent"/>, which applies
    ///   the effect to the target. In turn, "effect damage" components will listen for 
    ///   this event to apply the actual effect. These include
    ///   <see cref="EffectDamageElementalComponent"/> for elemental damage, among others.
    /// </summary>
    /// <remarks>
    /// To summarize:
    /// - <see cref="INewEffectSystem"/> raises <see cref="CastEffectEvent"/>.
    /// - Event handlers of <see cref="CastEffectEvent"/> should raise <see cref="ApplyEffectAreaEvent"/>.
    /// - Event handlers of <see cref="ApplyEffectAreaEvent"/> should raise <see cref="ApplyEffectDamageEvent"/>
    ///   and possibly <see cref="ApplyEffectTileDamageEvent"/>.
    /// - Event handlers of <see cref="ApplyEffectDamageEvent"/> could call 
    /// <see cref="INewEffectSystem.Apply"/> recursively for even more complex effects.
    /// Following this convention allows you to mix and match different effect behaviors
    /// to easily create new effects from the existing parts.
    /// </remarks>
    public interface INewEffectSystem : IEntitySystem
    {
        /// <summary>
        /// Applies an effect.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="effectID"></param>
        /// <param name="args"></param>
        /// <param name="retainEffectEntity">If false, the effect entity will be deleted after the effect is applied.</param>
        /// <returns>Turn result from the effect.</returns>
        TurnResult Apply(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, PrototypeId<EntityPrototype> effectID, EffectArgSet args, bool retainEffectEntity = false);

        TurnResult Apply(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, EntityUid effect, EffectArgSet args);

        bool TryPromptEffectTarget(EntityUid source, EntityUid value, EffectArgSet args, [NotNullWhen(true)] out EffectTarget? target);
    }

    public sealed record class EffectTarget(EntityUid? Target, EntityCoordinates? Coords);

    public sealed class NewEffectSystem : EntitySystem, INewEffectSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;

        public TurnResult Apply(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, PrototypeId<EntityPrototype> effectID, EffectArgSet args, bool retainEffectEntity = false)
        {
            var effect = _entityGen.SpawnEntity(effectID, MapCoordinates.Global);
            if (!IsAlive(effect) || !HasComp<EffectComponent>(effect.Value))
            {
                Logger.ErrorS("effect", $"Failed to cast event {effectID}, entity could not be spawned or has no {nameof(EffectComponent)}");
                if (effect != null && !retainEffectEntity)
                    EntityManager.DeleteEntity(effect.Value);
                return TurnResult.Aborted;
            }

            var result = Apply(source, target, targetCoords, effect.Value, args);

            if (IsAlive(effect) && !retainEffectEntity)
                EntityManager.DeleteEntity(effect.Value);

            return result;
        }

        public TurnResult Apply(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, EntityUid effect, EffectArgSet args)
        {
            var ev = new CastEffectEvent(source, target, targetCoords, args);
            RaiseEvent(effect, ev);
            return ev.TurnResult;
        }

        public bool TryPromptEffectTarget(EntityUid source, EntityUid effect, EffectArgSet args, [NotNullWhen(true)] out EffectTarget? target)
        {
            if (!IsAlive(effect))
            {
                target = null;
                return false;
            }

            var ev = new GetEffectPlayerTargetEvent(source, args);
            RaiseEvent(effect, ev);
            if (!ev.Handled || (ev.OutTarget == null && ev.OutCoords == null))
            {
                target = null;
                return false;
            }

            target = new(ev.OutTarget, ev.OutCoords);
            return true;
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
        public EntityCoordinates? TargetCoords { get; }
        public EffectArgSet Args { get; }

        public CastEffectEvent(EntityUid source, EntityUid? target, EntityCoordinates? targetCoords, EffectArgSet args)
        {
            Source = source;
            Target = target;
            TargetCoords = targetCoords;
            Args = args;
        }
    }

    /// <summary>
    /// Raised to retrieve the primary target or location of this effect,
    /// if it was <c>null</c>.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class GetEffectPlayerTargetEvent : HandledEntityEventArgs
    {
        public EntityUid Source { get; }
        public EffectArgSet Args { get; }

        public EntityUid? OutTarget { get; set; } = null;
        public EntityCoordinates? OutCoords { get; set; } = null;

        public GetEffectPlayerTargetEvent(EntityUid source, EffectArgSet args)
        {
            Source = source;
            Args = args;
        }

        public void Handle(EntityUid? target, EntityCoordinates? coords = null)
        {
            OutTarget = target;
            OutCoords = coords;
            Handled = true;
        }
    }

    /// <summary>
    /// Raised to retrieve the primary target if the AI is casting this effect.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class GetEffectAITargetEvent : CancellableEntityEventArgs
    {
        public EntityUid Source { get; }
        public EffectArgSet Args { get; }

        public EntityUid? OutTarget { get; set; } = null;
        public EntityCoordinates? OutCoords { get; set; } = null;

        public GetEffectAITargetEvent(EntityUid source, EffectArgSet args)
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
        public EffectCommonArgs CommonArgs => Args.Ensure<EffectCommonArgs>();

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
    public sealed class ApplyEffectDamageEvent : TurnResultEntityEventArgs, IApplyEffectEvent
    {
        /// <summary>
        /// Person who casted the event.
        /// </summary>
        public EntityUid Source { get; }

        /// <summary>
        /// Target of the effect. May be different from the original target
        /// in the case of AoE. May be <c>null</c> if the effect targets the ground.
        /// </summary>
        public EntityUid? InnerTarget { get; }

        public EntityCoordinates SourceCoords { get; }

        /// <summary>
        /// Coordinates of the target entity or targeted position, 
        /// These are guaranteed to be available even if there is no <see cref="InnerTarget"/>.
        /// </summary>
        public EntityCoordinates TargetCoords { get; }

        public MapCoordinates SourceCoordsMap => SourceCoords.ToMap(IoCManager.Resolve<IEntityManager>());
        public MapCoordinates TargetCoordsMap => TargetCoords.ToMap(IoCManager.Resolve<IEntityManager>());

        public IMap Map => IoCManager.Resolve<IMapManager>().GetMap(SourceCoordsMap.MapId);
        public IMap TargetMap => IoCManager.Resolve<IMapManager>().GetMap(TargetCoordsMap.MapId);

        public EffectArgSet Args { get; }

        /// <summary>
        /// Number of affected tiles if this event was invoked with an AoE.
        /// </summary>
        public int AffectedTiles { get; }

        /// <summary>
        /// Index of the tile being affected, starting from 0.
        /// </summary>
        public int AffectedTileIndex { get; } = 0;

        /// <summary>
        /// A damage property for calculating damage somewhere in the effect chain.
        /// </summary>
        public int OutDamage { get; set; } = 0;

        /// <summary>
        /// A damage property for calculating elemental power somewhere in the effect chain.
        /// </summary>
        public int OutElementalPower { get; set; } = 0;

        /// <summary>
        /// Set to false if the effect failed so that the associated item is not identified.
        /// </summary>
        public bool OutEffectWasObvious { get; set; } = true;

        public ApplyEffectDamageEvent(EntityUid source, EntityUid? target, EntityCoordinates sourceCoords, EntityCoordinates targetCoords, EffectArgSet args, int affectedTiles, int affectedTileIndex)
        {
            Source = source;
            InnerTarget = target;
            SourceCoords = sourceCoords;
            TargetCoords = targetCoords;
            Args = args;
            AffectedTiles = affectedTiles;
            AffectedTileIndex = affectedTileIndex;
        }
    }
}