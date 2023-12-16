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
    /// - Effect area components should raise <see cref="EffectHitEvent"/> which applies
    ///   the effect to the target. In turn "effect damage" components will listen for 
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
        TurnResult Apply(EntityUid source, EntityUid target, PrototypeId<EntityPrototype> effectID, EffectArgSet args);
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

        public TurnResult Apply(EntityUid source, EntityUid target, PrototypeId<EntityPrototype> effectID, EffectArgSet args)
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
    /// <see cref="ApplyEffectEvent"/> internally. 
    /// Examples are the magic and action entity systems.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class CastEffectEvent : TurnResultEntityEventArgs
    {
        public EntityUid Source { get; }
        public EntityUid Target { get; }
        public EffectArgSet Args { get; }

        public CastEffectEvent(EntityUid source, EntityUid target, EffectArgSet args)
        {
            Source = source;
            Target = target;
            Args = args;
        }
    }

    /// <summary>
    /// Runs the actual logic of the event after checks like MP/stamina/targeting
    /// have been completed.
    /// </summary>
    [EventUsage(EventTarget.Effect)]
    public sealed class ApplyEffectEvent : TurnResultEntityEventArgs
    {
        public EntityUid Source { get; }
        public EntityUid Target { get; }
        public EntityCoordinates Coords { get; }
        public EffectArgSet Args { get; }

        public ApplyEffectEvent(EntityUid source, EntityUid target, EntityCoordinates coords, EffectArgSet args)
        {
            Source = source;
            Target = target;
            Coords = coords;
            Args = args;
        }
    }
}