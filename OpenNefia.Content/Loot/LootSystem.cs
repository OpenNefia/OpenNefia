using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Configuration;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Items;
using OpenNefia.Content.Cargo;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Roles;

namespace OpenNefia.Content.Loot
{
    public interface ILootSystem : IEntitySystem
    {
    }

    public sealed class LootSystem : EntitySystem, ILootSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly ICargoSystem _cargo = default!;

        public override void Initialize()
        {
        }

        private bool ShouldDropCardOrFigure(EntityUid uid)
        {
            var quality = CompOrNull<QualityComponent>(uid)?.Quality.Base ?? Quality.Bad;
            return _rand.OneIn(175)
                || quality == Quality.Unique
                || _config.GetCVar(CCVars.DebugAlwaysDropFigureAndCard)
                || (quality == Quality.Great && _rand.OneIn(2))
                || (quality == Quality.Good && _rand.OneIn(3));
        }

        private bool ShouldDropPlayerItem(EntityUid player, EntityUid item)
        {
            // >>>>>>>> shade2/item.hsp:99 		if iNum(cnt)=0:continue ...
            if (!IsAlive(item))
                return false;

            if (TryMap(player, out var map) && CompOrNull<MapCommonComponent>(map.MapEntityUid)?.IsTemporary == true)
            {
                if (_equipSlots.IsEquippedOnAnySlot(item) || CompOrNull<ItemComponent>(item)?.IsPrecious == true || _rand.OneIn(3))
                {
                    return false;
                }
            }
            else if (_rand.OneIn(3))
            {
                return false;
            }

            if (HasComp<CargoComponent>(item))
            {
                if (TryMap(player, out map) && !_cargo.CanUseCargoItemsIn(map))
                {
                    return false;
                }
                else if (_rand.OneIn(2))
                {
                    return false;
                }
            }

            var identify = CompOrNull<IdentifyComponent>(item)?.IdentifyState ?? IdentifyState.None;
            var shouldDrop = true;

            if (_equipSlots.IsEquippedOnAnySlot(item))
            {
                if (_rand.OneIn(10))
                {
                    shouldDrop = false;
                }

                var curse = CompOrNull<CurseStateComponent>(item)?.CurseState ?? CurseState.Normal;
                if (curse >= CurseState.Blessed)
                {
                    if (_rand.OneIn(2))
                    {
                        shouldDrop = false;
                    }
                }

                if (curse <= CurseState.Cursed)
                {
                    if (_rand.OneIn(2))
                    {
                        shouldDrop = false;
                    }
                }

                if (curse <= CurseState.Doomed)
                {
                    if (_rand.OneIn(2))
                    {
                        shouldDrop = false;
                    }
                }
            }
            else if (identify >= IdentifyState.Full)
            {
                if (_rand.OneIn(4))
                {
                    shouldDrop = false;
                }
            }

            return shouldDrop;
            // <<<<<<<< shade2/item.hsp:135 		if f:iNum(ci)=0:continue ..
        }

        /// <summary>
        /// Whether or not to drop an item in a character's inventory when they die.
        /// </summary>
        private bool ShouldDropItem(EntityUid chara, EntityUid item)
        {
            if (!IsAlive(item))
                return false;

            if (HasComp<RoleCustomCharaComponent>(chara))
                return false;

            var shouldDrop = false;
            var itemQuality = CompOrNull<QualityComponent>(item)?.Quality.Base ?? Quality.Bad;
            
            if (itemQuality >= Quality.God)
                shouldDrop = true;

            if (_rand.OneIn(30))
                shouldDrop = true;

            if (itemQuality >= Quality.Great)
            {
                if (_rand.OneIn(2))
                    shouldDrop = true;
            }

            if (HasComp<RoleAdventurerComponent>(chara))
            {
                if (!_rand.OneIn(5))
                    shouldDrop = false;
            }

            // TODO arena

            if (itemQuality == Quality.Unique)
                shouldDrop = true;

            if (HasComp<AlwaysDropOnDeathComponent>(item))
                shouldDrop = true;

            return shouldDrop;
        }
    }
}