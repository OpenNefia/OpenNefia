using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Fame;
using OpenNefia.Core.Game;
using OpenNefia.Content.RandomGen;

namespace OpenNefia.Content.Chests
{
    public sealed partial class ChestSystem
    {
        [Dependency] private readonly IKarmaSystem _karma = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        private void InitializeChestEvents()
        {
            SubscribeComponent<SmallGambleChestComponent, EntityBeingGeneratedEvent>(BeingGenerated_SmallGambleChest);
            SubscribeComponent<SmallGambleChestComponent, BeforeGenerateChestItemEvent>(ChestFilter_SmallGambleChest)
                ;
            SubscribeComponent<BejeweledChestComponent, BeforeGenerateChestItemEvent>(ChestFilter_BejeweledChest);
            
            SubscribeComponent<SafeComponent, BeforeGenerateChestItemEvent>(ChestFilter_Safe);

            SubscribeComponent<WalletComponent, EntityBeingGeneratedEvent>(BeingGenerated_Wallet);
            SubscribeComponent<WalletComponent, BeforeGenerateChestItemEvent>(ChestFilter_Wallet);
            SubscribeComponent<WalletComponent, AfterChestOpenedEvent>(AfterChestOpened_Wallet);

            SubscribeComponent<SuitcaseComponent, EntityBeingGeneratedEvent>(BeingGenerated_Suitcase);
            SubscribeComponent<SuitcaseComponent, AfterChestOpenedEvent>(AfterChestOpened_Suitcase);

            SubscribeComponent<TreasureBallComponent, EntityBeingGeneratedEvent>(BeingGenerated_TreasureBall);
            SubscribeComponent<TreasureBallComponent, BeforeGenerateChestItemEvent>(ChestFilter_TreasureBall);
        }

        private void BeingGenerated_SmallGambleChest(EntityUid uid, SmallGambleChestComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (TryComp<ChestComponent>(uid, out var chest))
            {
                chest.LockpickDifficulty = _rand.Next(_rand.Next(100) + 1) + 1;

                if (TryComp<ValueComponent>(uid, out var value))
                    value.Value.Base = chest.LockpickDifficulty * 25 + 150;
            }

            _stacks.SetCount(uid, _rand.Next(8));
        }

        private void ChestFilter_SmallGambleChest(EntityUid uid, SmallGambleChestComponent component, BeforeGenerateChestItemEvent args)
        {
            var value = CompOrNull<ValueComponent>(uid)?.Value.Buffed ?? 0;
            args.OutItemFilter.Id = Protos.Item.GoldPiece;

            if (_rand.OneIn(75))
            {
                args.OutItemFilter.Amount = 50 * value;
            }
            else
            {
                args.OutItemFilter.Amount = _rand.Next(value / 10 + 1) + 1;
            }
        }

        private void ChestFilter_BejeweledChest(EntityUid uid, BejeweledChestComponent component, BeforeGenerateChestItemEvent args)
        {
            if (args.ItemIndex == 1 && _rand.OneIn(3))
            {
                args.OutItemFilter.Quality = Quality.Great;
            }
            else
            {
                args.OutItemFilter.Quality = Quality.Good;
            }

            if (_rand.OneIn(60))
            {
                args.OutItemFilter.Id = Protos.Item.PotionOfCureCorruption;
            }
        }

        private void ChestFilter_Safe(EntityUid uid, SafeComponent component, BeforeGenerateChestItemEvent args)
        {
            if (!_rand.OneIn(3))
            {
                args.OutItemFilter.Tags = new[] { Protos.Tag.ItemCatGold };
            }
            else
            {
                args.OutItemFilter.Tags = new[] { Protos.Tag.ItemCatOreValuable };
            }
        }

        private void BeingGenerated_Wallet(EntityUid uid, WalletComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (TryComp<ChestComponent>(uid, out var chest))
                chest.LockpickDifficulty = _rand.Next(15);
        }

        private void ChestFilter_Wallet(EntityUid uid, WalletComponent component, BeforeGenerateChestItemEvent args)
        {
            args.OutItemFilter.Id = Protos.Item.GoldPiece;
            var amount = _rand.Next(1000) + 1;
            if (_rand.OneIn(5))
            {
                amount = _rand.Next(9) + 1;
            }
            if (_rand.OneIn(10))
            {
                amount = _rand.Next(5000) + 5000;
            }
            if (_rand.OneIn(20))
            {
                amount = _rand.Next(20000) + 10000;
            }
            args.OutItemFilter.Amount = amount;
        }

        private void AfterChestOpened_Wallet(EntityUid uid, WalletComponent component, AfterChestOpenedEvent args)
        {
            _karma.ModifyKarma(args.ChestOpener, -4);
        }

        private void BeingGenerated_Suitcase(EntityUid uid, SuitcaseComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (TryComp<ChestComponent>(uid, out var chest))
            {
                var playerLevel = _levels.GetLevel(_gameSession.Player);
                chest.ItemLevel = (_rand.Next(10) + 1 * playerLevel / 10 + 1);
                chest.LockpickDifficulty = _rand.Next(15);
            }
        }

        private void AfterChestOpened_Suitcase(EntityUid uid, SuitcaseComponent component, AfterChestOpenedEvent args)
        {
            _karma.ModifyKarma(args.ChestOpener, -8);
        }

        private void BeingGenerated_TreasureBall(EntityUid uid, TreasureBallComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (TryComp<ChestComponent>(uid, out var chest))
                chest.ItemLevel = _levels.GetLevel(_gameSession.Player);
        }

        private void ChestFilter_TreasureBall(EntityUid uid, TreasureBallComponent component, BeforeGenerateChestItemEvent args)
        {
            args.OutItemFilter.Tags = new[] { _randomGen.PickTag(Protos.TagSet.ItemWear) };
            args.OutItemFilter.Quality = component.ItemQuality;
            if (_rand.OneIn(30))
                args.OutItemFilter.Id = Protos.Item.PotionOfCureCorruption;
        }
    }
}