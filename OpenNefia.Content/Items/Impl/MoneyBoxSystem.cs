using OpenNefia.Content.Currency;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Inventory;
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
using OpenNefia.Core.Audio;
using OpenNefia.Content.Weight;
using OpenNefia.Content.RandomGen;

namespace OpenNefia.Content.Items.Impl
{
    public interface IMoneyBoxSystem : IEntitySystem
    {
        TurnResult UseMoneyBox(EntityUid source, EntityUid target, MoneyBoxComponent? moneyBox = null);
    }

    public sealed class MoneyBoxSystem : EntitySystem, IMoneyBoxSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;

        public override void Initialize()
        {
            SubscribeComponent<MoneyBoxComponent, EntityBeingGeneratedEvent>(BeingGenerated_MoneyBox);
            SubscribeComponent<MoneyBoxComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_MoneyBox);
            SubscribeComponent<MoneyBoxComponent, GetVerbsEventArgs>(GetVerbs_MoneyBox);
            SubscribeComponent<MoneyBoxComponent, ThrownEntityImpactedGroundEvent>(OnThrown_MoneyBox);
        }

        private static readonly int[] MoneyBoxIncrements =
        {
            500,
            2000,
            10000,
            50000,
            500000,
            5000000,
            100000000
        };

        private void BeingGenerated_MoneyBox(EntityUid uid, MoneyBoxComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (component.GoldIncrement <= 0)
            {
                var count = MoneyBoxIncrements.Length;
                component.GoldIncrement = MoneyBoxIncrements[_rand.Next(_rand.Next(count) + 1) + 1];
            }

            int index = MoneyBoxIncrements.Length - 1;
            for (int i = 0; i < MoneyBoxIncrements.Length; i++)
            {
                var inc = MoneyBoxIncrements[i];
                if (component.GoldIncrement <= inc)
                {
                    index = i;
                    break;
                }
            }

            if (TryComp<ValueComponent>(uid, out var value))
                value.Value = 200 + index * index + index * 100;
        }

        private void LocalizeExtra_MoneyBox(EntityUid uid, MoneyBoxComponent component, ref LocalizeItemNameExtraEvent args)
        {
            var increment = Loc.GetString($"Elona.MoneyBox.ItemName.Increments.{component.GoldIncrement}");
            var s = Loc.GetString("Elona.MoneyBox.ItemName.Amount", ("name", args.OutFullName.ToString()), ("increment", increment));
            args.OutFullName.Clear().Append(s);
        }

        private void GetVerbs_MoneyBox(EntityUid uid, MoneyBoxComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Deposit Gold", () => UseMoneyBox(args.Source, args.Target)));
        }

        private void OnThrown_MoneyBox(EntityUid uid, MoneyBoxComponent component, ThrownEntityImpactedGroundEvent args)
        {
            if (args.Handled)
                return;

            _mes.Display(Loc.GetString("Elona.Drinkable.Thrown.Shatters"));
            _audio.Play(Protos.Sound.Crush2, uid);

            if (component.GoldDeposited > 0)
            {
                _itemGen.GenerateItem(uid, Protos.Item.GoldPiece, amount: component.GoldDeposited);
            }

            EntityManager.DeleteEntity(uid);
            args.Handled = true;
        }

        private void DepositGold(WalletComponent source, MoneyBoxComponent moneyBox, int amount)
        {
            amount = Math.Min(amount, source.Gold);
            source.Gold -= amount;
            moneyBox.GoldDeposited += amount;
            if (amount > 0 && TryComp<WeightComponent>(moneyBox.Owner, out var weight))
            {
                weight.Weight += 100;
            }
        }

        public TurnResult UseMoneyBox(EntityUid source, EntityUid target, MoneyBoxComponent? moneyBox = null)
        {
            if (!Resolve(target, ref moneyBox))
                return TurnResult.Aborted;

            if (!TryComp<WalletComponent>(source, out var wallet) || moneyBox.GoldIncrement > wallet.Gold)
            {
                _mes.Display(Loc.GetString("Elona.MoneyBox.NotEnoughGold"));
                return TurnResult.Aborted;
            }

            if (moneyBox.GoldDeposited >= moneyBox.GoldLimit)
            {
                _mes.Display(Loc.GetString("Elona.MoneyBox.Full"));
                return TurnResult.Aborted;
            }

            if (!_stacks.TrySplit(target, 1, out var split))
                return TurnResult.Aborted;

            _audio.Play(Protos.Sound.Paygold1, split);
            DepositGold(wallet, Comp<MoneyBoxComponent>(split), moneyBox.GoldIncrement);

            return TurnResult.Aborted;
        }
    }
}