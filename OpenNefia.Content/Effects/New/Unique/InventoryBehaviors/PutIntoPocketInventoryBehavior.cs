using OpenNefia.Content.Pickable;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Containers;
using HspIdsInv = OpenNefia.Core.Prototypes.HspIds<OpenNefia.Content.Inventory.InvElonaId>;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Items;
using OpenNefia.Content.Weight;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Effects
{
    public sealed class PutIntoPocketInventoryBehavior : BaseInventoryBehavior
    {
        [Dependency] private readonly IVerbSystem _verbSystem = default!;
        [Dependency] private readonly IPickableSystem _pickable = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IWeightSystem _weights = default!;

        public override HspIdsInv HspIds { get; } = HspIdsInv.From122(new(id: 24, subId: 5));

        public override string WindowTitle => Loc.GetString("Elona.Inventory.Behavior.Put.WindowTitle");
        public override UiElement MakeIcon() => InventoryHelpers.MakeIcon(InventoryIcon.PickUp);
        public override bool ApplyNameModifiers => false;
        public override TurnResult? TurnResultAfterSelectionIfEmpty => TurnResult.Succeeded;
        public override bool QueryAmount => true;

        public IContainer Container { get; }

        public PutIntoPocketInventoryBehavior(IContainer container) : base()
        {
            Container = container;
        }

        public override IEnumerable<IInventorySource> GetSources(InventoryContext context)
        {
            yield return new InventoryInvSource(context.User);
        }

        public override string GetQueryText(InventoryContext context)
        {
            return Loc.GetString("Elona.Inventory.Behavior.Put.QueryText");
        }

        public override int? OnQueryAmount(InventoryContext context, EntityUid item)
        {
            if (!EntityManager.TryGetComponent<StackComponent>(item, out var stack))
                return null;

            var min = 1;
            var max = stack.Count;

            if (EntityManager.TryGetComponent<ItemContainerComponent>(Container.Owner, out var itemContainer)
                && itemContainer.MaxTotalWeight != null)
            {
                var weightLeft = itemContainer.MaxTotalWeight.Value - _weights.GetTotalWeight(Container.Owner, excludeSelf: true, stackCount: 1);
                var itemSingleWeight = _weights.GetTotalWeight(item, stackCount: 1);
                max = (int)double.Floor(weightLeft / itemSingleWeight);

                if (max <= 0)
                {
                    _mes.Display(Loc.GetString("Elona.ItemContainer.Put.Errors.ItemTooHeavy", ("maxWeight", UiUtils.DisplayWeight(itemContainer.MaxTotalWeight.Value))));
                    _audio.Play(Protos.Sound.Fail1);
                    return null;
                }
            }

            return OnQueryAmount(context, item, min, max);
        }

        public override InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount)
        {
            if (!_damages.DoStaminaCheck(context.User, 10))
            {
                _mes.Display(Loc.GetString("Elona.Common.TooExhausted", ("entity", context.User)));
                return new InventoryResult.Finished(TurnResult.Succeeded);
            }

            if (!_pickable.CheckPickableOwnStateAndMessage(item))
                return new InventoryResult.Finished(TurnResult.Succeeded);

            if (Container.Insert(item))
            {
                _audio.Play(Protos.Sound.Drop1);
                _mes.Display(Loc.GetString("Elona.ItemContainer.Put.Succeed", ("entity", context.User), ("target", item)));
            }
            else
            {
                // XXX: error message is displayed inside ItemContainerSystem.
                _audio.Play(Protos.Sound.Fail1);
            }

            return new InventoryResult.Continuing();
        }
    }
}
