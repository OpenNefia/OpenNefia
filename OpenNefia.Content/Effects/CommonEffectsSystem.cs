using OpenNefia.Content.Parties;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Random;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Visibility;

namespace OpenNefia.Content.Effects
{
    public sealed class CommonEffectsSystem : EntitySystem
    {
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public void WakeUpEveryone(IMap map)
        {
            var hour = _world.State.GameDate.Hour;
            if (hour >= 7 || hour <= 22)
            {
                foreach (var effects in _lookup.EntityQueryInMap<StatusEffectsComponent>(map.Id))
                {
                    if (_parties.IsUnderlingOfPlayer(effects.Owner) && _effects.HasEffect(effects.Owner, Protos.StatusEffect.Sleep))
                    {
                        if (_rand.OneIn(10))
                        {
                            _effects.Remove(effects.Owner, Protos.StatusEffect.Sleep);
                        }
                    }
                }
            }
        }

        // TODO move into effect apply callbacks
        public void GetWet(EntityUid ent, int amount, StatusEffectsComponent? statusEffects = null)
        {
            if (!Resolve(ent, ref statusEffects))
                return;

            _effects.Apply(ent, Protos.StatusEffect.Wet, amount, statusEffects: statusEffects);
            _mes.Display(Loc.GetString("Elona.CommonEffects.Wet.GetsWet", ("entity", ent)));

            if (TryComp<VisibilityComponent>(ent, out var vis) && vis.IsInvisible.Buffed)
            {
                _mes.Display(Loc.GetString("Elona.CommonEffects.Wet.IsRevealed", ("entity", ent)));
            }
        }

        public void DamageTileFire(MapCoordinates coords, EntityUid? source)
        {
            throw new NotImplementedException();
        }

        public void DamageItemsFire(EntityUid target)
        {
            throw new NotImplementedException();
        }
    }
}
