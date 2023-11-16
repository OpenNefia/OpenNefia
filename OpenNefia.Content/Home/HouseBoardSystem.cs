using OpenNefia.Content.Damage;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Items;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Home
{
    public interface IHouseBoardSystem : IEntitySystem
    {
        TurnResult UseHouseBoard(EntityUid reader, EntityUid ancientBook, HouseBoardComponent? houseBoardComp = null);
    }

    public sealed partial class HouseBoardSystem : EntitySystem, IHouseBoardSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        public override void Initialize()
        {
            SubscribeComponent<HouseBoardComponent, GetVerbsEventArgs>(GetVerbs_HouseBoard);
            SubscribeComponent<HouseBoardComponent, HouseBoardGetActionsEvent>(GetDefaultHouseBoardActions);
        }

        private void GetVerbs_HouseBoard(EntityUid uid, HouseBoardComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Use House Board", () => UseHouseBoard(args.Source, args.Target)));
        }

        public TurnResult UseHouseBoard(EntityUid user, EntityUid houseBoard, HouseBoardComponent? houseBoardComp = null)
        {
            if (!Resolve(houseBoard, ref houseBoardComp))
                return TurnResult.Aborted;

            // >>>>>>>> shade2 / action.hsp:1808   case effShop...
            if (!TryMap(houseBoard, out var map) || !HasComp<MapTypePlayerOwnedComponent>(map.MapEntityUid))
            {
                _mes.Display(Loc.GetString("Elona.Item.HouseBoard.CannotUseItHere"));
                return TurnResult.Aborted;
            }

            var itemCount = _lookup.EntityQueryInMap<ItemComponent>(map).Count();
            var furnitureCount = _lookup.EntityQueryInMap<FurnitureComponent>(map).Count();
            var maxItemCount = "(none)";
            if (TryComp<MapCommonComponent>(map.MapEntityUid, out var mapCommon))
            {
                var maxItems = mapCommon.MaxItemsOnGround;
                maxItemCount = maxItems != null ? maxItems.Value.ToString() : Loc.GetString("Elona.Item.HouseBoard.Unlimited");
            }

            // <<<<<<<< shade2 / action.hsp:1811   swbreak..

            _mes.Display(Loc.GetString("Elona.Item.HouseBoard.ItemCount", 
                ("mapEntity", map.MapEntityUid),
                ("itemCount", itemCount),
                ("furnitureCount", furnitureCount),
                ("maxItems", maxItemCount)));

            var ev = new HouseBoardQueriedEvent(user);
            RaiseEvent(houseBoard, ev);

            while (true)
            {
                var ev2 = new HouseBoardGetActionsEvent(user);
                RaiseEvent(houseBoard, ev2);

                if (ev2.OutActions.Count == 0)
                {
                    Logger.Warning("No house board interact options found!");
                    _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                    return TurnResult.Aborted;
                }

                var choices = ev2.OutActions.Select(act => new PromptChoice<HouseBoardAction>(act, act.Text));
                var args = new Prompt<HouseBoardAction>.Args(choices)
                {
                    IsCancellable = true,
                    QueryText = Loc.GetString("Elona.Item.HouseBoard.WhatDo")
                };

                var result = _uiManager.Query<Prompt<HouseBoardAction>,
                    Prompt<HouseBoardAction>.Args,
                    PromptChoice<HouseBoardAction>>(args);

                if (!result.HasValue)
                    break;

                result.Value.ChoiceData.Action(user);
            }

            return TurnResult.Aborted;
        }
    }

    public delegate void HouseBoardActionDelegate(EntityUid user);

    public sealed class HouseBoardAction
    {
        public HouseBoardAction(string text, HouseBoardActionDelegate action)
        {
            Text = text;
            Action = action;
        }

        public HouseBoardActionDelegate Action { get; }
        public string Text { get; }
    }

    public sealed class HouseBoardQueriedEvent
    {
        public HouseBoardQueriedEvent(EntityUid user)
        {
            User = user;
        }

        public EntityUid User { get; }
    }

    public sealed class HouseBoardGetActionsEvent
    {
        public HouseBoardGetActionsEvent(EntityUid user)
        {
            User = user;
        }

        public IList<HouseBoardAction> OutActions { get; } = new List<HouseBoardAction>();
        public EntityUid User { get; }
    }
}