using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Spells;
using OpenNefia.Core;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Effects
{
    /// <summary>
    /// Teleports either the source or target.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageTeleportComponent : Component
    {
        [DataField]
        public EffectSubject Subject { get; set; } = EffectSubject.Source;

        [DataField]
        public TeleportOrigin Origin { get; set; } = TeleportOrigin.Source;

        [DataField]
        public ITeleportPosition Position { get; set; } = new NullTeleportPosition();

        [DataField]
        public bool IgnoresPreventTeleport { get; set; }

        [DataField]
        public LocaleKey MessageKey { get; set; } = "Elona.Effect.Teleport.General";

        [DataField]
        public int MaxAttempts { get; set; } = 200;
    }

    public enum EffectSubject
    {
        Source,
        Target
    }

    public enum TeleportOrigin
    {
        Source,
        Target,
        TargetCoordinates
    }

    [ImplicitDataDefinitionForInheritors]
    public interface ITeleportPosition
    {
        EntityCoordinates GetCoordinates(EntityUid subject, EntityUid caster, EntityUid? target, IMap map, EntityCoordinates coords, int attempt);
    }

    public sealed class NullTeleportPosition : ITeleportPosition
    {
        public EntityCoordinates GetCoordinates(EntityUid subject, EntityUid caster, EntityUid? target, IMap map, EntityCoordinates coords, int attempt)
        {
            return coords;
        }
    }

    public sealed class WholeMapTeleportPosition : ITeleportPosition
    {
        [Dependency] private readonly IRandom _rand = default!;

        public EntityCoordinates GetCoordinates(EntityUid subject, EntityUid caster, EntityUid? target, IMap map, EntityCoordinates coords, int attempt)
        {
            var pos = (_rand.Next(map.Width - 2) + 1, _rand.Next(map.Height - 2) + 1);
            return new EntityCoordinates(coords.EntityId, pos);
        }
    }

    public sealed class ShortTeleportPosition : ITeleportPosition
    {
        [Dependency] private readonly IRandom _rand = default!;

        public EntityCoordinates GetCoordinates(EntityUid subject, EntityUid caster, EntityUid? target, IMap map, EntityCoordinates coords, int attempt)
        {
            var offset = ((3 - attempt / 70 + _rand.Next(5)) * (_rand.OneIn(2) ? -1 : 1),
                          (3 - attempt / 70 + _rand.Next(5)) * (_rand.OneIn(2) ? -1 : 1));
            return coords.Offset(offset);
        }
    }

    public sealed class RadiusTeleportPosition : ITeleportPosition
    {
        [Dependency] private readonly IRandom _rand = default!;

        public EntityCoordinates GetCoordinates(EntityUid subject, EntityUid caster, EntityUid? target, IMap map, EntityCoordinates coords, int attempt)
        {
            var offset = _rand.NextVec2iInRadius(attempt / 8 + 2);
            return coords.Offset(offset);
        }
    }
}
