using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
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
using OpenNefia.Content.Equipment;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Loot;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Roles;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Equipment
{
    public interface IEquipmentGenSystem : IEntitySystem
    {
    }

    public sealed class EquipmentGenSystem : EntitySystem, IEquipmentGenSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEquipmentSystem _equipment = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly ICombatSystem _combat = default!;

        public override void Initialize()
        {
        }

        public void GenerateAndEquipEquipment(EntityUid chara, InventoryComponent? inv = null)
        {
            if (!Resolve(chara, ref inv))
                return;

            var hasWeapon = false;

            for (var i = 0; i < 100; i++)
            {
                var deleteCandidates = _inv.EnumerateLiveItems(chara, inv)
                    .Where(i => !HasComp<AlwaysDropOnDeathComponent>(i))
                    .ToList();

                if (deleteCandidates.Any())
                {
                    var item = _rand.PickAndTake(deleteCandidates);
                    EntityManager.DeleteEntity(item);
                }

                var filter = new ItemFilter()
                {
                    MinLevel = _levels.GetLevel(chara)
                };

                if (HasComp<RoleAdventurerComponent>(chara))
                {
                    filter.Quality = Qualities.Quality.Good;
                }
                else
                {
                    filter.Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Normal);
                }

                PrototypeId<TagPrototype>? category = null;

                foreach (var slot in _equipSlots.GetEquipSlots(chara))
                {
                    if (!_equipSlots.TryGetContainerForEquipSlot(chara, slot, out var container))
                        continue;

                    if (IsAlive(container.ContainedEntity))
                    {
                        if (slot.ID == Protos.EquipSlot.Hand && !hasWeapon && _tags.HasTag(container.ContainedEntity.Value, Protos.Tag.ItemCatEquipMelee))
                        {
                            hasWeapon = true;
                        }
                    }
                    else
                    {
                        if (slot.ID == Protos.EquipSlot.Hand && !hasWeapon)
                        {
                            category = Protos.Tag.ItemCatEquipMelee;
                            break;
                        }
                        else if (slot.ID == Protos.EquipSlot.Head)
                        {
                            category = Protos.Tag.ItemCatEquipHead;
                            break;
                        }
                        else if (slot.ID == Protos.EquipSlot.Body)
                        {
                            category = Protos.Tag.ItemCatEquipBody;
                            break;
                        }
                        else if (slot.ID == Protos.EquipSlot.Ranged)
                        {
                            category = Protos.Tag.ItemCatEquipRanged;
                            break;
                        }
                        else if (slot.ID == Protos.EquipSlot.Ammo)
                        {
                            category = Protos.Tag.ItemCatEquipAmmo;
                            break;
                        }
                    }
                }

                if (category == null)
                    break;

                filter.Tags = new[] { category.Value };
                var item = _itemGen.GenerateItem(inv.Container, filter);
                if (!IsAlive(item))
                    return;

                EnsureComp<IdentifyComponent>(item.Value).IdentifyState = IdentifyState.Full;

                var quality = CompOrNull<QualityComponent>(item.Value)?.Quality.Base ?? Quality.Bad;

                if (quality >= Quality.Great && HasComp<EquipmentComponent>(item.Value) && HasComp<RoleAdventurerComponent>(chara))
                {
                    // TODO news
                }

                _equipment.EquipIfHigherValueInSlotForNPC(chara, item.Value);

                if (!HasComp<RoleAdventurerComponent>(chara) && !_rand.OneIn(3))
                    break;
            }
        }
    }

    public sealed record EquipmentSpecifier(PrototypeId<EquipmentSpecPrototype> ID, ItemFilter ItemFilter);

    public sealed class EquipmentGenTemplate
    {
        public IList<EquipmentSpecifier> Specifiers { get; } = new List<EquipmentSpecifier>();
    }
}