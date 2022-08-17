using OpenNefia.Content.Cargo;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Currency;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Items;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Weight;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Fame;

namespace OpenNefia.Content.Loot
{
    public interface ILootSystem : IEntitySystem
    {
        IList<DroppedItem> CalcDroppedItemsOnDeath(EntityUid uid);
        void DropItemsOnDeath(EntityUid victim, IEnumerable<DroppedItem> droppedItems);

        ItemFilter MakeDefaultLootItemFilter(EntityUid chara, PrototypeId<TagPrototype>[] filterSet);
        void ModifyItemForCorpse(EntityUid item, EntityUid victim, EntityUid? attacker = null);
        void ModifyItemForRemains(EntityUid item, EntityUid chara, EntityUid? attacker = null);

        IList<LootDrop> CalcLootDrops(EntityUid victim, EntityUid? attacker = null);
        void SpawnLootItems(EntityUid victim, IEnumerable<LootDrop> lootDrops, EntityUid? attacker = null);

        void DropLoot(EntityUid victim, EntityUid? attacker = null);

        void AddLootToResultList(IList<LootDrop> lootDrops, EntityUid victim, IReadOnlyCollection<PrototypeId<TagPrototype>> categoryChoices, OnGenerateLootItemDelegate? onGenerateLoot = null);
        void AddLootToResultList(IList<LootDrop> lootDrops, EntityUid victim, PrototypeId<TagPrototype> category, OnGenerateLootItemDelegate? onGenerateLoot = null);
    }

    public sealed class LootSystem : EntitySystem, ILootSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly ICargoSystem _cargo = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IFameSystem _fame = default!;

        public override void Initialize()
        {
            SubscribeComponent<CharaComponent, EntityKilledEvent>(HandleKilled);
            SubscribeComponent<LootTypeComponent, OnGenerateLootDropsEvent>(AddLootDropsFromComponent, priority: EventPriorities.Highest);
            SubscribeComponent<GoldLootComponent, OnGenerateLootDropsEvent>(AddGoldLootDrops);
        }

        private void HandleKilled(EntityUid uid, CharaComponent component, ref EntityKilledEvent args)
        {
            DropLoot(uid, args.Attacker);
        }

        private void AddLootDropsFromComponent(EntityUid uid, LootTypeComponent component, OnGenerateLootDropsEvent args)
        {
            foreach (var entry in component.LootDrops)
            {
                if (entry.OneIn != null && !_rand.OneIn(entry.OneIn.Value))
                    continue;
                else if (entry.Prob != null && !_rand.Prob(entry.Prob.Value))
                    continue;

                args.OutLootDrops.Add(entry);
            }
        }

        private void AddGoldLootDrops(EntityUid uid, GoldLootComponent component, OnGenerateLootDropsEvent args)
        {
            var fame = CompOrNull<FameComponent>(args.Attacker ?? _gameSession.Player)?.Fame.Buffed ?? 0;
            args.OutLootDrops.Add(new LootDrop(new ItemFilter()
            {
                Id = Protos.Item.GoldPiece,
                Amount = 2500 + _rand.Next(fame + 1000)
            }));
        }

        private bool ShouldDropCardOrFigurine(EntityUid uid)
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
        private bool ShouldDropNPCItem(EntityUid chara, EntityUid item)
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

        private IList<DroppedItem> CalcDroppedPlayerItems(EntityUid player)
        {
            if (!TryMap(player, out var map))
                return new List<DroppedItem>();

            var result = new List<DroppedItem>();

            foreach (var item in _inv.EnumerateItems(player).Where(i => ShouldDropPlayerItem(player, i)))
            {
                var action = DropAction.PlaceInMap;
                var isPrecious = CompOrNull<ItemComponent>(item)?.IsPrecious ?? false;
                var curseState = CompOrNull<CurseStateComponent>(item)?.CurseState ?? CurseState.Normal;

                if (!isPrecious)
                {
                    if (_rand.OneIn(4))
                    {
                        action = DropAction.Delete;
                    }
                    if (curseState == CurseState.Blessed)
                    {
                        if (_rand.OneIn(3))
                        {
                            action = DropAction.PlaceInMap;
                        }
                    }
                    if (curseState <= CurseState.Cursed)
                    {
                        if (_rand.OneIn(3))
                        {
                            action = DropAction.Delete;
                        }
                    }
                    if (curseState <= CurseState.Doomed)
                    {
                        if (_rand.OneIn(3))
                        {
                            action = DropAction.Delete;
                        }
                    }
                }

                result.Add(new DroppedItem(item, action));
            }

            return result;
        }

        private IList<DroppedItem> CalcDroppedNPCItems(EntityUid npc)
        {
            if (_factions.GetRelationToPlayer(npc) >= Relation.Ally)
                return new List<DroppedItem>();

            return _inv.EnumerateItems(npc)
                .Where(i => ShouldDropNPCItem(npc, i))
                .Select(i => new DroppedItem(i, DropAction.PlaceInMap))
                .ToList();
        }

        public IList<DroppedItem> CalcDroppedItemsOnDeath(EntityUid uid)
        {
            var ev = new BeforeDropItemsOnDeathEvent();
            if (Raise(uid, ev))
                return ev.OutDroppedItems;

            if (_gameSession.IsPlayer(uid))
            {
                return CalcDroppedPlayerItems(uid);
            }
            else
            {
                return CalcDroppedNPCItems(uid);
            }
        }

        public void DropItemsOnDeath(EntityUid victim, IEnumerable<DroppedItem> droppedItems)
        {
            if (!TryMap(victim, out var map))
                return;

            var charaSpatial = Spatial(victim);
            var inv = CompOrNull<InventoryComponent>(victim);

            foreach (var dropped in droppedItems)
            {
                if (_equipSlots.TryGetSlotEquippedOn(dropped.Item, out var owner, out var slot))
                {
                    if (!_equipSlots.TryUnequip(owner.Value, slot, inv?.Container, silent: true, force: true))
                    {
                        Logger.ErrorS("loot", $"Failed to unequip {dropped.Item} on entity {owner.Value}");
                        continue;
                    }
                }

                switch (dropped.Action)
                {
                    case DropAction.Delete:
                        EntityManager.DeleteEntity(dropped.Item);
                        break;
                    case DropAction.PlaceInMap:
                    default:
                        Spatial(dropped.Item).Coordinates = charaSpatial.Coordinates;
                        _stacks.TryStackAtSamePos(dropped.Item);
                        if (TryComp<PickableComponent>(dropped.Item, out var pickable))
                            pickable.OwnState = OwnState.None;
                        break;
                }
            }
        }

        public ItemFilter MakeDefaultLootItemFilter(EntityUid chara, PrototypeId<TagPrototype>[] filterSet)
        {
            return new ItemFilter()
            {
                MinLevel = _randomGen.CalcObjectLevel(chara),
                Quality = _randomGen.CalcObjectQuality(Quality.Normal),
                Tags = new[] { _rand.Pick(filterSet) },
            };
        }

        public void ModifyItemForRemains(EntityUid item, EntityUid chara, EntityUid? attacker = null)
        {
            if (TryProtoID(chara, out var id))
                EnsureComp<EntityProtoSourceComponent>(item).EntityID = id.Value;

            if (TryComp<ChipComponent>(item, out var itemChip) && TryComp<ChipComponent>(chara, out var charaChip))
            {
                itemChip.Color = charaChip.Color;
            }

            if (TryComp<WeightComponent>(item, out var itemWeight) && TryComp<ValueComponent>(item, out var itemValue))
            {
                if (TryComp<WeightComponent>(chara, out var charaWeight))
                {
                    itemWeight.Weight = charaWeight.Weight;
                }

                if (HasComp<CorpseComponent>(item))
                {
                    itemWeight.Weight = 250 * (itemWeight.Weight + 100) / 100 + 500;
                    itemValue.Value = itemWeight.Weight / 5;
                }
                else
                {
                    itemWeight.Weight = 20 * (itemWeight.Weight + 500) / 500;
                    itemValue.Value = _levels.GetLevel(chara) * 40 + 600;
                    var rarity = _randomGen.GetRarity(chara, RandomGenTables.Chara) / 1000;
                    if (rarity < 20 && _factions.GetRelationToPlayer(chara) < Relation.Dislike)
                    {
                        itemValue.Value *= Math.Clamp(4 - rarity / 5, 1, 5);
                    }
                }
            }
        }

        public void ModifyItemForCorpse(EntityUid item, EntityUid victim, EntityUid? attacker = null)
        {
            ModifyItemForRemains(item, victim);

            if (HasComp<LivestockComponent>(victim) && IsAlive(attacker) && _skills.HasSkill(attacker.Value, Protos.Skill.Anatomy))
            {
                var extraAmount = 1;
                if (_skills.Level(attacker.Value, Protos.Skill.Anatomy) > _levels.GetLevel(victim))
                    extraAmount++;

                _stacks.SetCount(item, _stacks.GetCount(item) + _rand.Next(extraAmount));
            }
        }

        public void AddLootToResultList(IList<LootDrop> lootDrops, EntityUid victim, PrototypeId<TagPrototype> category, OnGenerateLootItemDelegate? onGenerateLoot = null)
            => AddLootToResultList(lootDrops, victim, new[] { category }, onGenerateLoot);

        public void AddLootToResultList(IList<LootDrop> lootDrops, EntityUid victim, IReadOnlyCollection<PrototypeId<TagPrototype>> categoryChoices, OnGenerateLootItemDelegate? onGenerateLoot = null)
        {
            var itemFilter = MakeDefaultLootItemFilter(victim, categoryChoices.ToArray());
            lootDrops!.Add(new LootDrop(itemFilter, onGenerateLoot));
        }

        public IList<LootDrop> CalcLootDrops(EntityUid victim, EntityUid? attacker = null)
        {
            if (_gameSession.IsPlayer(victim))
                return new List<LootDrop>();

            IList<LootDrop> result = new List<LootDrop>();

            var victimQuality = CompOrNull<QualityComponent>(victim)?.Quality.Buffed ?? Quality.Normal;

            if (TryComp<MoneyComponent>(victim, out var victimMoney))
            {
                if (victimQuality > Quality.Great || _rand.OneIn(20) || victimMoney.AlwaysDropsGoldOnDeath)
                {
                    var goldDropped = victimMoney.Gold / (1 + (victimMoney.AlwaysDropsGoldOnDeath ? 3 : 0));
                    result.Add(new LootDrop(new ItemFilter() { Id = Protos.Item.GoldPiece, Amount = goldDropped }));
                    victimMoney.Gold -= goldDropped;
                }

                if (victimMoney.Platinum > 0)
                {
                    result.Add(new LootDrop(new ItemFilter() { Id = Protos.Item.PlatinumCoin, Amount = victimMoney.Platinum }));
                }
            }

            if (TryComp<EquipmentGenComponent>(victim, out var eqType) && eqType.EquipmentType != null)
            {
                var pev = new P_EquipmentTypeOnGenerateLootEvent(victim, result);
                _protos.EventBus.RaiseEvent(eqType.EquipmentType.Value, pev);
            }

            if (TryComp<LootTypeComponent>(victim, out var lootType) && lootType.LootType != null)
            {
                var pev = new P_LootTypeOnGenerateLootEvent(victim, result);
                _protos.EventBus.RaiseEvent(lootType.LootType.Value, pev);
            }

            var remainsChance = _config.GetCVar(CCVars.DebugAlwaysDropRemains) ? 1 : 40;

            if (_rand.OneIn(remainsChance))
                AddLootToResultList(result, victim, new[] { Protos.Tag.ItemCatRemains }, ModifyItemForRemains);

            // TODO show house
            // TODO arena
            var isArena = false;

            if (!isArena && HasComp<RoleCustomCharaComponent>(victim))
            {
                var color = CompOrNull<ChipComponent>(victim)?.Color ?? Color.White;

                // TODO figure/card

                if (ShouldDropCardOrFigurine(victim))
                {
                    result.Add(new LootDrop(new ItemFilter() { Id = Protos.Item.Card }));
                }
                if (ShouldDropCardOrFigurine(victim))
                {
                    result.Add(new LootDrop(new ItemFilter() { Id = Protos.Item.Figurine }));
                }
            }

            var ev = new OnGenerateLootDropsEvent(attacker, result);
            RaiseEvent(victim, ev);
            result = ev.OutLootDrops;

            var anatomySuccess = _rand.OneIn(60);
            if (IsAlive(attacker))
            {
                // TODO god cat blessing
                var power = Math.Sqrt(_skills.Level(attacker.Value, Protos.Skill.Anatomy));
                if (power > _rand.Next(150))
                    anatomySuccess = true;
                _skills.GainSkillExp(attacker.Value, Protos.Skill.Anatomy, 10 + (anatomySuccess ? 4 : 0));
            }

            if (anatomySuccess || victimQuality >= Quality.Great
                               || _config.GetCVar(CCVars.DebugAlwaysDropRemains)
                               || HasComp<LivestockComponent>(victim))
            {
                result.Add(new LootDrop(new ItemFilter() { Id = Protos.Item.Corpse }, ModifyItemForCorpse));
            }

            if (TryComp<RichLootComponent>(victim, out var richLoot))
            {
                for (var i = 0; i < richLoot.RichLootItemCount; i++)
                {
                    result.Add(new LootDrop(new ItemFilter()
                    {
                        MinLevel = _randomGen.CalcObjectLevel(victim),
                        Tags = new[] { Protos.Tag.ItemCatOreValuable }
                    }));
                }
                if (_rand.OneIn(3))
                {
                    result.Add(new LootDrop(new ItemFilter() { Id = Protos.Item.Wallet }));
                }
            }

            return result;
        }

        public void SpawnLootItems(EntityUid victim, IEnumerable<LootDrop> lootDrops, EntityUid? attacker = null)
        {
            foreach (var drop in lootDrops)
            {
                var item = _itemGen.GenerateItem(victim, drop.ItemFilter);
                if (IsAlive(item))
                {
                    drop.OnGenerateItem?.Invoke(item.Value, victim, attacker);
                }
            }

            var ev = new AfterGeneratedLootEvent(attacker, lootDrops);
            RaiseEvent(victim, ev);
        }

        public void DropLoot(EntityUid victim, EntityUid? attacker = null)
        {
            // TODO immediate quest

            var items = CalcDroppedItemsOnDeath(victim);
            if (items.Count > 0)
                DropItemsOnDeath(victim, items);

            var lootDrops = CalcLootDrops(victim, attacker);
            if (lootDrops.Count > 0)
                SpawnLootItems(victim, lootDrops, attacker);
        }
    }

    public delegate void OnGenerateLootItemDelegate(EntityUid item, EntityUid victim, EntityUid? attacker = null);

    [DataDefinition]
    public class LootDrop
    {
        public LootDrop() {}

        public LootDrop(ItemFilter itemFilter, OnGenerateLootItemDelegate? onGenerateItem = null)
        {
            ItemFilter = itemFilter;
            OnGenerateItem = onGenerateItem;
        }

        [DataField]
        public ItemFilter ItemFilter { get; } = new();

        [DataField]
        public OnGenerateLootItemDelegate? OnGenerateItem { get; } = null;
    }

    [DataDefinition]
    public sealed class LootDropEntry : LootDrop
    {
        public LootDropEntry() {}

        [DataField]
        public int? OneIn { get; }

        [DataField]
        public float? Prob { get; }
    }

    public enum DropAction
    {
        Delete,
        PlaceInMap
    }

    public sealed record DroppedItem(EntityUid Item, DropAction Action);

    public sealed class BeforeDropItemsOnDeathEvent : HandledEntityEventArgs
    {
        public IList<DroppedItem> OutDroppedItems { get; } = new List<DroppedItem>();
    }

    public sealed class AfterDroppedItemsOnDeathEvent : EntityEventArgs
    {
    }

    public sealed class OnGenerateLootDropsEvent : EntityEventArgs
    {
        public EntityUid? Attacker { get; }

        public IList<LootDrop> OutLootDrops { get; }

        public OnGenerateLootDropsEvent(EntityUid? attacker, IList<LootDrop> lootDrops)
        {
            Attacker = attacker;
            OutLootDrops = lootDrops;
        }
    }

    public sealed class AfterGeneratedLootEvent : EntityEventArgs
    {
        public EntityUid? Attacker { get; }
        public IEnumerable<LootDrop> LootDrops { get; }

        public AfterGeneratedLootEvent(EntityUid? attacker, IEnumerable<LootDrop> lootDrops)
        {
            Attacker = attacker;
            LootDrops = lootDrops;
        }
    }
}