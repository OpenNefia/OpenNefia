using OpenNefia.Content.Dialog;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Levels;
using OpenNefia.Core.Log;
using OpenNefia.Content.Damage;
using OpenNefia.Content.World;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Inventory;
using OpenNefia.Core;
using OpenNefia.Core.UserInterface;
using OpenNefia.Content.Activity;
using Microsoft.FileFormats;
using OpenNefia.Content.DisplayName;

namespace OpenNefia.Content.Quests
{
    public sealed partial class VanillaQuestsSystem
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

        private void HarvestCrop_GetDisplayName(EntityUid uid, HarvestQuestCropComponent component, ref GetDisplayNameEventArgs args)
        {
            // >>>>>>>> elona122/shade2/item_func.hsp:425 		if iProperty(id)=propQuest:s+=lang(""," grown ") ...
            args.OutName = Loc.GetString("Elona.Quest.Types.Harvest.ItemName.Grown", ("baseName", args.OutName), ("weightClass", component.WeightClass));
            // <<<<<<<< elona122/shade2/item_func.hsp:425 		if iProperty(id)=propQuest:s+=lang(""," grown ") ...
        }

        // >>>>>>>> elona122/shade2/command.hsp:3423 		if iProperty(cnt)=propQuest:if (invCtrl!1)&(invC ...
        private HashSet<Type> AllowedCropInventoryContexts = new HashSet<Type>()
        {
            typeof(ExamineInventoryBehavior),
            typeof(DropInventoryBehavior),
            typeof(PickUpInventoryBehavior),
            typeof(EatInventoryBehavior),
            typeof(HarvestQuestDeliverInventoryBehavior),
        };

        private void HarvestCrop_InventoryFilter(EntityUid uid, HarvestQuestCropComponent component, ref InventoryContextFilterEvent args)
        {
            // hackish... but what else can one do?
            if (!AllowedCropInventoryContexts.Contains(args.Behavior.GetType()))
                args.OutAccepted = false;
        }
        // <<<<<<<< elona122/shade2/command.hsp:3423 		if iProperty(cnt)=propQuest:if (invCtrl!1)&(invC ...

        private void HarvestCrop_GetVerbs(EntityUid uid, HarvestQuestCropComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.RemoveWhere(v => v.VerbType == PickableSystem.VerbTypePickUp);
            args.OutVerbs.Add(new Verb(PickableSystem.VerbTypePickUp, "Pick Up Crop", () => HarvestCrop_PickUp(args.Source, args.Target, component)));
        }

        private TurnResult HarvestCrop_PickUp(EntityUid source, EntityUid target, HarvestQuestCropComponent component)
        {
            // >>>>>>>> shade2/action.hsp:151 		if iType(ci)=fltFood:if iProperty(ci)=propQuest: ...
            if (!_activities.HasAnyActivity(source))
            {
                if (_inv.IsInventoryFull(source))
                {
                    _mes.Display(Loc.GetString("Elona.Inventory.Common.InventoryIsFull"));
                    return TurnResult.Aborted;
                }
            }

            var activity = EntityManager.SpawnEntity(Protos.Activity.Harvesting, MapCoordinates.Global);
            Comp<ActivityHarvestingComponent>(activity).Item = target;
            _activities.StartActivity(source, activity);

            // This is so the inventory screen will exit and a turn will pass
            // Normally the pickup behavior would return InventoryResult.Continuing which
            // wouldn't make sense here.
            return TurnResult.Failed; 
            // <<<<<<<< shade2/action.hsp:155 		} ..
        }

        private void HarvestDeliveryChest_GetVerbs(EntityUid uid, HarvestDeliveryChestComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(OpenInventoryBehavior.VerbTypeOpen, "Use Delivery Chest", () => HarvestDeliveryChest_Open(args.Source, args.Target, component)));
        }

        private TurnResult HarvestDeliveryChest_Open(EntityUid source, EntityUid target, HarvestDeliveryChestComponent component)
        {
            if (!TryMap(target, out var map) || !_immediateQuests.TryGetImmediateQuest<QuestTypeHarvestComponent>(map, out var quest, out _, out var questHarvest))
            {
                return TurnResult.Aborted;
            }

            var context = new InventoryContext(source, target, new HarvestQuestDeliverInventoryBehavior(questHarvest));
            var result = _uiManager.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);

            if (result.HasValue && result.Value.Data is InventoryResult.Finished invResult)
            {
                return invResult.TurnResult;
            }

            return TurnResult.Aborted;
        }

        private void RemoveQuestItemsInInventory(EntityUid uid, InventoryComponent component, BeforePartyMemberLeavesMapEventArgs args)
        {
            // TODO recursive delete!
            // >>>>>>>> shade2/quest.hsp:314 	if gQuest=qHarvest{ ..
            foreach (var item in component.Container.ContainedEntities.ToList())
            {
                if (HasComp<HarvestQuestCropComponent>(item))
                    EntityManager.DeleteEntity(item);
            }
            // <<<<<<<< shade2/quest.hsp:320 		} ..
        }
    }
}