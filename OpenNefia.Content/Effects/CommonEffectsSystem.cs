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
using OpenNefia.Content.Charas;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.UI;
using OpenNefia.Content.Factions;
using OpenNefia.Content.VanillaAI;

namespace OpenNefia.Content.Effects
{
    public sealed class CommonEffectsSystem : EntitySystem
    {
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;

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

        public void MakeSound(EntityUid origin, MapCoordinates coords, int tileRadius, float wakeChance, bool isWhistle = false)
        {
            if (!TryMap(coords, out var map))
                return;

            foreach (var (chara, spatial) in _lookup.EntityQueryInMap<CharaComponent, SpatialComponent>(map.Id))
            {
                var entity = chara.Owner;

                if (spatial.MapPosition.TryDistanceTiled(coords, out var dist) && dist < tileRadius)
                {
                    if (_rand.Prob(wakeChance))
                    {
                        if (_effects.HasEffect(entity, Protos.StatusEffect.Sleep))
                        {
                            _effects.Remove(entity, Protos.StatusEffect.Sleep);
                            _mes.Display(Loc.GetString("Elona.CommonEffects.Sound.Waken", ("entity", entity)));
                        }

                        _emoIcons.SetEmotionIcon(entity, EmotionIcons.Question, 2);

                        if (isWhistle && _rand.OneIn(500))
                        {
                            _mes.Display(Loc.GetString("Elona.CommonEffects.Sound.GetsAngry", ("entity", entity)), UiColors.MesSkyBlue);
                            _mes.Display(Loc.GetString("Elona.CommonEffects.Sound.CanNoLongerStand", ("entity", entity)));

                            if (_parties.IsInPlayerParty(entity))
                            {
                                _factions.SetPersonalRelationTowards(entity, origin, Relation.Enemy);
                            }
                            _vanillaAI.SetTarget(entity, origin, 80);
                            _emoIcons.SetEmotionIcon(entity, EmotionIcons.Angry, 2);
                        }
                    }
                }
            }
        }
    }
}
