using OpenNefia.Content.DisplayName;
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
using OpenNefia.Content.Items;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Levels;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Audio;
using OpenNefia.Content.SaveLoad;
using OpenNefia.Content.Inventory;
using Microsoft.CodeAnalysis.Operations;
using OpenNefia.Content.Lockpick;

namespace OpenNefia.Content.Chest
{
    public interface IChestSystem : IEntitySystem
    {
        TurnResult OpenChest(EntityUid user, EntityUid chest, bool silent = false, ChestComponent? chestComp = null);
        void DoOpenChestEntity(EntityUid user, EntityUid chest, int? itemCount = null, int? itemLevel = null, int? seed = null, bool silent = false, ChestComponent? chestComp = null);
        void DoOpenChest(EntityUid user, EntityUid chest, int itemCount, int itemLevel, int seed, bool silent = false);
    }

    public sealed partial class ChestSystem : EntitySystem, IChestSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly ISaveLoadSystem _saveLoad = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly ILockpickSystem _lockpicks = default!;

        public override void Initialize()
        {
            SubscribeComponent<ChestComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_Chest);
            SubscribeComponent<ChestComponent, EntityBeingGeneratedEvent>(BeingGenerated_Chest, priority: EventPriorities.High);
            SubscribeComponent<ChestComponent, GetVerbsEventArgs>(GetVerbs_Chest);
            SubscribeComponent<ChestComponent, AfterChestOpenedEvent>(AfterChestOpened_Chest);

            InitializeChestEvents();
        }

        private void LocalizeExtra_Chest(EntityUid uid, ChestComponent chest, ref LocalizeItemNameExtraEvent args)
        {
            if (chest.DisplayLevelInName)
            {
                args.OutFullName.Append(Loc.Space() + Loc.GetString($"Elona.Chest.ItemName.Level", ("level", chest.LockpickDifficulty)));
            }
            if (!chest.HasItems)
            {
                args.OutFullName.Append(Loc.GetString("Elona.Chest.ItemName.Empty"));
            }
        }

        private void BeingGenerated_Chest(EntityUid uid, ChestComponent component, ref EntityBeingGeneratedEvent args)
        {
            var mapLevel = 1;
            var isShelter = false;
            if (TryMap(uid, out var map))
            {
                mapLevel = _levels.GetLevel(map.MapEntityUid);
                isShelter = HasComp<ShelterComponent>(map.MapEntityUid);
            }
            // TODO shelter
            var itemLevel = 5;
            if (!isShelter)
                itemLevel += mapLevel;

            var difficulty = 1;
            if (!isShelter)
                itemLevel += _rand.Next(Math.Abs(mapLevel));

            component.ItemLevel = itemLevel;
            component.LockpickDifficulty = difficulty;
            component.RandomSeed = _rand.Next();
        }

        private void GetVerbs_Chest(EntityUid uid, ChestComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(OpenInventoryBehavior.VerbTypeOpen, "Open Chest", () => OpenChest(args.Source, args.Target)));
        }

        public TurnResult OpenChest(EntityUid user, EntityUid chest, bool silent = false, ChestComponent? chestComp = null)
        {
            if (!Resolve(chest, ref chestComp))
                return TurnResult.Aborted;

            if (!chestComp.HasItems)
            {
                _mes.Display(Loc.GetString("Elona.Chest.Open.Empty"));
                return TurnResult.Aborted;
            }

            if (!_stacks.TrySplit(chest, 1, out var split))
                return TurnResult.Aborted;

            chestComp = Comp<ChestComponent>(split);

            if (chestComp.LockpickDifficulty > 0 && !_lockpicks.TryToLockpick(user, chestComp.LockpickDifficulty))
                return TurnResult.Failed;

            DoOpenChestEntity(user, split, silent: silent, chestComp: chestComp);
            
            chestComp.HasItems = false;
            _stacks.TryStackAtSamePos(chest);

            return TurnResult.Succeeded;
        }

        public void DoOpenChestEntity(EntityUid user, EntityUid chest, int? itemCount = null, int? itemLevel = null, int? seed = null, bool silent = false, ChestComponent? chestComp = null)
        {
            if (!Resolve(chest, ref chestComp))
                return;

            itemCount ??= chestComp.ItemCount ?? 3 + _rand.Next(5);
            itemLevel ??= chestComp.ItemLevel;
            seed ??= chestComp.RandomSeed;

            DoOpenChest(user, chest, itemCount.Value, itemLevel.Value, seed.Value, silent);
        }

        public void DoOpenChest(EntityUid user, EntityUid chest, int itemCount, int itemLevel, int seed, bool silent = false)
        {
            if (!TryMap(chest, out var map))
                return;

            if (!silent)
            {
                _audio.Play(Protos.Sound.Chest1, chest);
                _mes.Display(Loc.GetString("Elona.Chest.Open.YouOpen", ("user", user), ("item", chest)));
                _playerQuery.PromptMore();
            }

            _rand.WithSeed(seed, DoOpen);

            void DoOpen()
            {
                for (var i = 0; i < itemCount; i++)
                {
                    var quality = Quality.Good;
                    if (i == 0)
                        quality = Quality.Great;

                    var itemFilter = new ItemFilter()
                    {
                        MinLevel = _randomGen.CalcObjectLevel(itemLevel),
                        Quality = _randomGen.CalcObjectQuality(quality),
                        Tags = new[] { _rand.Pick(RandomGenConsts.FilterSets.Chest) }
                    };

                    if (i > 0 && !_rand.OneIn(3))
                    {
                        if (!_rand.OneIn(3))
                        {
                            itemFilter.Tags = new[] { Protos.Tag.ItemCatGold };
                        }
                        else
                        {
                            itemFilter.Tags = new[] { Protos.Tag.ItemCatOreValuable };
                        }
                    }

                    var ev = new BeforeGenerateChestItemEvent(user, i, itemFilter);
                    RaiseEvent(chest, ev);
                    itemFilter = ev.OutItemFilter;

                    _itemGen.GenerateItem(chest, itemFilter);
                }
            }

            if (!silent)
            {
                _audio.Play(Protos.Sound.Ding2);
                _mes.Display(Loc.GetString("Elona.Chest.Open.Goods", ("item", chest)));
            }

            var ev = new AfterChestOpenedEvent(user);
            RaiseEvent(chest, ev);

            _saveLoad.QueueAutosave();
        }

        private void AfterChestOpened_Chest(EntityUid uid, ChestComponent component, AfterChestOpenedEvent args)
        {
            if (component.SmallMedalProb != null && _rand.Prob(component.SmallMedalProb.Value))
            {
                _itemGen.GenerateItem(uid, Protos.Item.SmallMedal, amount: 1);
            }
        }
    }

    public sealed class BeforeGenerateChestItemEvent : EntityEventArgs
    {
        public EntityUid ChestOpener { get; }
        public int ItemIndex { get; }

        public ItemFilter OutItemFilter { get; set; }

        public BeforeGenerateChestItemEvent(EntityUid chestOpener, int itemIndex, ItemFilter itemFilter)
        {
            ChestOpener = chestOpener;
            ItemIndex = itemIndex;
            OutItemFilter = itemFilter;
        }
    }

    public sealed class AfterChestOpenedEvent : EntityEventArgs
    {
        public EntityUid ChestOpener { get; }

        public AfterChestOpenedEvent(EntityUid chestOpener)
        {
            ChestOpener = chestOpener;
        }
    }
}