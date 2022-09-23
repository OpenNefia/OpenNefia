using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI;
using OpenNefia.Core.Serialization.Manager.Attributes;
using NuGet.Common;
using OpenNefia.Core;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Enchantments;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Directions;
using OpenNefia.Content.UI.Layer;

namespace OpenNefia.Content.Spells
{
    public enum TeleportEntity
    {
        EffectSource,
        EffectTarget
    }

    [ImplicitDataDefinitionForInheritors]
    public interface ITeleportCoordinates
    {
        EntityCoordinates GetCoordinates(IMap map, EntityCoordinates coords, int attempt);
    }

    public sealed class NullTeleportPosition : ITeleportCoordinates
    {
        public EntityCoordinates GetCoordinates(IMap map, EntityCoordinates coords, int attempt)
        {
            return coords;
        }
    }

    public class BaseEffectTeleport : Effect
    {
        [Dependency] protected readonly IRandom _rand = default!;
        [Dependency] protected readonly IMapManager _mapManager = default!;
        [Dependency] protected readonly IEntityLookup _entityLookup = default!;
        [Dependency] protected readonly IEntityGen _entityGen = default!;
        [Dependency] protected readonly IRandomGenSystem _randomGen = default!;
        [Dependency] protected readonly ICharaGen _charaGen = default!;
        [Dependency] protected readonly IItemGen _itemGen = default!;
        [Dependency] protected readonly IGameSessionManager _gameSession = default!;
        [Dependency] protected readonly IMessagesManager _mes = default!;
        [Dependency] protected readonly IAudioManager _audio = default!;
        [Dependency] protected readonly IEffectSystem _effects = default!;
        [Dependency] protected readonly IVisibilitySystem _vis = default!;
        [Dependency] protected readonly IActivitySystem _activities = default!;
        [Dependency] protected readonly IEnchantmentSystem _enchantments = default!;
        [Dependency] protected readonly IFieldLayer _field = default!;

        [DataField]
        public bool IgnoresPreventTeleport { get; set; }

        [DataField]
        public TeleportEntity SourceEntity { get; set; } = TeleportEntity.EffectSource;

        [DataField]
        public ITeleportCoordinates Coordinates { get; set; } = new NullTeleportPosition();

        [DataField]
        public LocaleKey MessageKey { get; set; } = "Elona.Effect.Teleport.Execute";

        [DataField]
        public int MaxAttempts { get; set; } = 200;

        public override TurnResult Apply(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
        {
            var ent = SourceEntity == TeleportEntity.EffectSource ? source : target;

            if (!_mapManager.TryGetMapOfEntity(ent, out var map))
                return TurnResult.Failed;

            if (!IgnoresPreventTeleport)
            {
                if (_enchantments.HasEnchantmentEquipped<EncPreventTeleportComponent>(ent))
                {
                    _mes.Display(Loc.GetString("Elona.Effect.Teleport.Prevented"));
                    return TurnResult.Failed;
                }
            }

            if (EntityManager.TryGetComponent<MapCommonComponent>(map.MapEntityUid, out var mapCommon)
                && mapCommon.PreventsTeleport)
            {
                _mes.Display(Loc.GetString("Elona.Effect.Teleport.Prevented"));
                return TurnResult.Failed;
            }

            var spatial = EntityManager.GetComponent<SpatialComponent>(ent);

            EntitySystem.InjectDependencies(Coordinates);

            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var newCoords = Coordinates.GetCoordinates(map, coords, attempt);

                if (map.CanAccess(newCoords))
                {
                    _activities.RemoveActivity(ent);
                    spatial.Coordinates = newCoords;

                    if (_gameSession.IsPlayer(ent))
                    {
                        _field.RefreshScreen();
                        _audio.Play(Protos.Sound.Teleport1, newCoords);
                        _mes.Display(Loc.GetString(MessageKey, ("chara", ent), ("source", source), ("target", target)));
                    }
                    else if (_vis.IsInWindowFov(coords))
                    {
                        _audio.Play(Protos.Sound.Teleport1, coords);
                        _mes.Display(Loc.GetString(MessageKey, ("chara", ent), ("source", source), ("target", target)));
                    }

                    return TurnResult.Succeeded;
                }
            }

            return TurnResult.Failed;
        }
    }

    public sealed class WholeMapTeleportCoordinates : ITeleportCoordinates
    {
        [Dependency] private readonly IRandom _rand = default!;

        public EntityCoordinates GetCoordinates(IMap map, EntityCoordinates coords, int attempt)
        {
            var pos = (_rand.Next(map.Width - 2) + 1, _rand.Next(map.Height - 2) + 1);
            return new EntityCoordinates(coords.EntityId, pos);
        }
    }

    public sealed class EffectTeleport : BaseEffectTeleport
    {
        public EffectTeleport()
        {
            Coordinates = new WholeMapTeleportCoordinates();
        }
    }

    public sealed class EffectTeleportOther : BaseEffectTeleport
    {
        public EffectTeleportOther()
        {
            Coordinates = new WholeMapTeleportCoordinates();
            SourceEntity = TeleportEntity.EffectTarget;
        }
    }

    public sealed class EffectShortTeleport : BaseEffectTeleport
    {
        public sealed class TeleportCoordinates : ITeleportCoordinates
        {
            [Dependency] private readonly IRandom _rand = default!;

            public EntityCoordinates GetCoordinates(IMap map, EntityCoordinates coords, int attempt)
            {
                var offset = ((3 - attempt / 70 + _rand.Next(5)) * (_rand.OneIn(2) ? -1 : 1),
                              (3 - attempt / 70 + _rand.Next(5)) * (_rand.OneIn(2) ? -1 : 1));
                return coords.Offset(offset);
            }
        }

        public EffectShortTeleport()
        {
            Coordinates = new TeleportCoordinates();
        }
    }

    public sealed class RadiusTeleportCoordinates : ITeleportCoordinates
    {
        [Dependency] private readonly IRandom _rand = default!;

        public EntityCoordinates GetCoordinates(IMap map, EntityCoordinates coords, int attempt)
        {
            var offset = _rand.NextVec2iInRadius(attempt / 8 + 2);
            return coords.Offset(offset);
        }
    }

    public sealed class EffectShadowStep : BaseEffectTeleport
    {
        public EffectShadowStep()
        {
            Coordinates = new RadiusTeleportCoordinates();
            MessageKey = "Elona.Effect.Teleport.ShadowStep";
            SourceEntity = TeleportEntity.EffectSource;
        }

        public override TurnResult Apply(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
        {
            var targetSpatial = EntityManager.GetComponent<SpatialComponent>(target);
            return base.Apply(source, target, targetSpatial.Coordinates, verb, args);
        }
    }

    public sealed class EffectDrawShadow : BaseEffectTeleport
    {
        public EffectDrawShadow()
        {
            Coordinates = new RadiusTeleportCoordinates();
            MessageKey = "Elona.Effect.Teleport.DrawShadow";
            SourceEntity = TeleportEntity.EffectTarget;
        }

        public override TurnResult Apply(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args)
        {
            var sourceSpatial = EntityManager.GetComponent<SpatialComponent>(source);
            return base.Apply(source, target, sourceSpatial.Coordinates, verb, args);
        }
    }
}