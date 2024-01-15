using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Containers;
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
using OpenNefia.Content.Weight;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.Cargo;
using OpenNefia.Core.UI;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Items.Impl
{
    public interface IItemContainerSystem : IEntitySystem
    {
        bool IsItemContainerFull(EntityUid uid, ItemContainerComponent? containerComp = null);
    }

    public sealed class ItemContainerSystem : EntitySystem, IItemContainerSystem
    {
        [Dependency] private readonly IWeightSystem _weights = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<ItemContainerComponent, ContainerIsInsertingAttemptEvent>(HandleAboutToInsert);
            SubscribeComponent<ItemContainerNoCargoComponent, ContainerIsInsertingAttemptEvent>(HandleAboutToInsert_NoCargo);
        }

        private void HandleAboutToInsert(EntityUid uid, ItemContainerComponent component, ContainerIsInsertingAttemptEvent args)
        {
            // >>>>>>>> elona122/shade2/command.hsp:3685 				if (invCtrl(1)=3)or(invCtrl(1)=5):if inv_sum(- ...
            if (args.Cancelled)
                return;

            if (IsItemContainerFull(uid, component))
            {
                _mes.Display(Loc.GetString("Elona.ItemContainer.Put.Errors.ContainerIsFull"));
                args.Cancel();
                return;
            }

            var itemWeight = _weights.GetTotalWeight(args.EntityUid);
            var itemSingleWeight = _weights.GetTotalWeight(args.EntityUid, stackCount: 1);

            if (component.MaxTotalWeight != null)
            {
                var totalWeight = _weights.GetTotalWeight(uid, excludeSelf: true, stackCount: 1);
                if (totalWeight + itemWeight > component.MaxTotalWeight)
                {
                    _mes.Display(Loc.GetString("Elona.ItemContainer.Put.Errors.TotalWeightTooHeavy", ("maxWeight", UiUtils.DisplayWeight(component.MaxTotalWeight.Value))));
                    args.Cancel();
                    return;
                }
            }

            if (component.MaxItemWeight != null && itemSingleWeight > component.MaxItemWeight)
            {
                _mes.Display(Loc.GetString("Elona.ItemContainer.Put.Errors.ItemTooHeavy", ("maxWeight", UiUtils.DisplayWeight(component.MaxItemWeight.Value))));
                args.Cancel();
                return;
            }
            // <<<<<<<< elona122/shade2/command.hsp:3687 					if iWeight(ci)>=efP*100 :snd seFail1:txt lang ...
        }

        private void HandleAboutToInsert_NoCargo(EntityUid uid, ItemContainerNoCargoComponent component, ContainerIsInsertingAttemptEvent args)
        {
            if (args.Cancelled)
                return;

            if (!component.AllowCargo && HasComp<CargoComponent>(args.EntityUid))
            {
                _mes.Display(Loc.GetString("Elona.ItemContainer.Put.Errors.CannotHoldCargo"));
                args.Cancel();
                return;
            }
        }

        public bool IsItemContainerFull(EntityUid uid, ItemContainerComponent? containerComp = null)
        { 
            // >>>>>>>> elona122/shade2/command.hsp:3688 					if iWeight(ci)<=0	:snd seFail1:txt lang("荷物は入 ...
            if (!Resolve(uid, ref containerComp))
                return true;

            if (containerComp.MaxItemCount != null)
            {
                if (containerComp.Container.ContainedEntities.Count >= containerComp.MaxItemCount)
                {
                    return true;
                }
            }

            return false;
            // <<<<<<<< elona122/shade2/command.hsp:3688 					if iWeight(ci)<=0	:snd seFail1:txt lang("荷物は入 ...
        }
    }
}