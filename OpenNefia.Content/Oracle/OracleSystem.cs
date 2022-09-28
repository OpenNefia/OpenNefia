using OpenNefia.Content.Adventurer;
using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Items;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Roles;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Utility;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Levels;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.DisplayName;

namespace OpenNefia.Content.Oracle
{
    public interface IOracleSystem : IEntitySystem
    {
        List<string> ArtifactLocations { get; }

        string? GetOracleText(EntityUid item);
        void ConvertArtifact(ref EntityUid item);
    }

    public sealed class OracleSystem : EntitySystem, IOracleSystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IAdventurerSystem _adv = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IQualitySystem _qualities = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IInventorySystem _inventories = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        [RegisterSaveData("Elona.OracleSystem.ArtifactLocations")]
        public List<string> ArtifactLocations { get; } = new();

        public override void Initialize()
        {
            SubscribeComponent<ItemComponent, EntityBeingGeneratedEvent>(AddOracleText);
        }

        private void AddOracleText(EntityUid item, ItemComponent itemComp, ref EntityBeingGeneratedEvent args)
        {
            // >>>>>>>> shade2/item.hsp:628 	itemMemory(1,dbId)++ ...
            var noOracle = false;
            if (args.GenArgs.TryGet<ItemGenArgs>(out var itemGenArgs))
            {
                noOracle = itemGenArgs.NoOracle || itemGenArgs.IsShop;
            }

            if (!noOracle && CompOrNull<QualityComponent>(item)?.Quality.Base == Quality.Unique)
            {
                var text = GetOracleText(item);
                if (text != null)
                    ArtifactLocations.Add(text);
            }
            // <<<<<<<< shade2/item.hsp:636  	} ...
        }

        public string? GetOracleText(EntityUid item)
        {
            if (!TryMap(item, out var map))
                return null;

            var date = _world.State.GameDate;

            if (_lookup.TryGetOwningEntity<CharaComponent>(item, out var owner)
                && HasComp<RoleAdventurerComponent>(owner.Value)
                && _adv.TryGetArea(owner.Value, out var area))
            {
                return Loc.GetString("Elona.Oracle.WasHeldBy", ("item", item), ("owner", owner.Value), ("map", map), ("day", date.Day), ("month", date.Month), ("year", date.Year));
            }

            return Loc.GetString("Elona.Oracle.WasCreatedAt", ("item", item), ("map", map), ("day", date.Day), ("month", date.Month), ("year", date.Year));
        }

        public void ConvertArtifact(ref EntityUid item)
        {
            // >>>>>>>> elona122/shade2/item.hsp:3 #deffunc convertArtifact int ci,int mode ...
            if (!HasComp<EquipmentComponent>(item)
                || _qualities.GetQuality(item) != Quality.Unique
                || _equipSlots.IsEquippedOnAnySlot(item)
                || !TryMap(item, out var map)
                || !TryProtoID(item, out var id)
                || !_inventories.TryGetInventoryContainer(item, out var container))
                return;

            // XXX: I'm not sure what the purpose of this is. It checks if any item in a character's
            // inventory in this map has the same ID as the target item.
            var found = false;
            foreach (var (owningChara, otherItem) in _charas.EnumerateNonAllies(map)
                .SelectMany(c => _inventories.EnumerateInventoryAndEquipment(c.Owner).Select(i => (c, i))))
            {
                if (IsAlive(otherItem) && IsAlive(owningChara.Owner) && !HasComp<RoleAdventurerComponent>(owningChara.Owner) && otherItem != item)
                {
                    if (TryProtoID(otherItem, out var otherID) && id.Value == otherID.Value)
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
                return;

            var itemCategories = GetItemCategories(item);
            if (itemCategories.Length == 0)
                return;

            EntityUid? newItem = null;
            var filter = new ItemFilter()
            {
                MinLevel = _levels.GetLevel(item),
                Quality = Quality.Great,
                Tags = itemCategories
            };
            for (var i = 0; i < 1000; i++)
            {
                newItem = _itemGen.GenerateItem(container, filter);

                if (IsAlive(newItem))
                {
                    if (_qualities.GetQuality(newItem.Value) != Quality.Unique)
                        break;

                    EntityManager.DeleteEntity(newItem.Value);
                }
            }

            if (!IsAlive(newItem))
                return;

            var oldItemName = _displayNames.GetDisplayName(item);
            _mes.Display(Loc.GetString("Elona.Oracle.ConvertArtifact", ("oldItemName", oldItemName), ("item", newItem.Value)));

            EntityManager.DeleteEntity(item);
            item = newItem.Value;
            // <<<<<<<< elona122/shade2/item.hsp:24 	return ci ...
        }

        private PrototypeId<TagPrototype>[] GetItemCategories(EntityUid item)
        {
            return _tags.GetTags(item)
                .Where(tag => _protos.HasExtendedData<TagPrototype, ExtTagItemCategory>(tag))
                .ToArray();
        }
    }
}