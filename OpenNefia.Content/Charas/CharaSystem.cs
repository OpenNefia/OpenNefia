using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Skills;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects;
using System;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Log;
using OpenNefia.Content.Sanity;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.World;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Buffs;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Weight;
using OpenNefia.Content.CharaMake;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Parties;

namespace OpenNefia.Content.Charas
{
    public interface ICharaSystem : IEntitySystem
    {
        PrototypeId<ChipPrototype> GetDefaultCharaChip(EntityUid uid, CharaComponent? chara = null);
        PrototypeId<ChipPrototype> GetDefaultCharaChip(PrototypeId<RacePrototype> raceID, Gender gender);
        PrototypeId<ChipPrototype> GetDefaultCharaChip(RacePrototype race, Gender gender);
        bool RenewStatus(EntityUid entity, CharaComponent? chara);
        bool Revive(EntityUid uid, bool force = false, CharaComponent? chara = null);
        IEnumerable<CharaComponent> EnumerateNonAllies(IMap map);
    }

    public sealed partial class CharaSystem : EntitySystem, ICharaSystem
    {
        [Dependency] private readonly IMapPlacement _mapPlacement = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IPartySystem _parties = default!;

        public bool Revive(EntityUid entity, bool force = false, CharaComponent? chara = null)
        {
            if (!Resolve(entity, ref chara) || EntityManager.IsAlive(entity))
                return false;

            var spatial = Spatial(entity);
            var placeType = CharaPlaceType.Npc;
            if (force)
                placeType = CharaPlaceType.Ally;
            var pos = _mapPlacement.FindFreePositionForChara(spatial.MapPosition, placeType);
            if (pos == null || !pos.Value.TryToEntity(_mapManager, out var entPos))
                return false;

            spatial.Coordinates = entPos;
            chara.Liveness = CharaLivenessState.Alive;

            if (EntityManager.TryGetComponent<SkillsComponent>(entity, out var skills))
            {
                skills.HP = skills.MaxHP / 3;
                skills.MP = skills.MaxMP / 3;
                skills.Stamina = skills.MaxStamina / 3;
            }

            if (EntityManager.TryGetComponent<SanityComponent>(entity, out var sanity))
            {
                sanity.Insanity = 0;
            }

            if (EntityManager.TryGetComponent<HungerComponent>(entity, out var hunger))
            {
                hunger.Nutrition = 8000;
            }

            _vanillaAI.SetTarget(entity, null, 0);

            return RenewStatus(entity, chara);
        }

        public bool RenewStatus(EntityUid entity, CharaComponent? chara = null)
        {
            if (!Resolve(entity, ref chara))
                return false;

            _activities.RemoveActivity(entity);
            _effects.RemoveAll(entity);
            _buffs.RemoveAllBuffs(entity);
            _emoicons.SetEmotionIcon(entity, null);
            _skillAdjusts.RemoveAllSkillAdjusts(entity);

            if (EntityManager.TryGetComponent<VanillaAIComponent>(entity, out var vai))
            {
                vai.Aggro = 0;
            }
            
            _refresh.Refresh(entity);

            return true;
        }

        public IEnumerable<CharaComponent> EnumerateNonAllies(IMap map)
        {
            return _lookup.EntityQueryInMap<CharaComponent>(map.Id)
                .Where(c => !_parties.IsInPlayerParty(c.Owner));
        }
    }
}
