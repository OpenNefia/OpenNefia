using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Home;
using OpenNefia.Core.Locale;
using OpenNefia.Content.UI;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Stayers;

namespace OpenNefia.Content.Home
{
    public sealed partial class HouseBoardSystem
    {
        [Dependency] private readonly IHomeSystem _homes = default!;
        [Dependency] private readonly IServantSystem _servants = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IStayersSystem _stayers = default!;

        private void HouseBoard_Design(EntityUid user)
        {
            _mes.Newline();
            _mes.Display(Loc.GetString("Elona.Home.Design.Help"));
            var args = new MapDesignerLayer.Args(user);
            _uiManager.Query<MapDesignerLayer, MapDesignerLayer.Args>(args);
        }

        private void HouseBoard_ViewHomeRank(EntityUid user)
        {
            var map = GetMap(user);
            var mostValuable = _homes.CalcMostValuableItems(map);
            var homeRank = _homes.UpdateRank(map);
            var args = new HomeRankLayer.Args(mostValuable, homeRank);
            _uiManager.Query<HomeRankLayer, HomeRankLayer.Args>(args);
        }

        private void HouseBoard_AlliesInYourHome(EntityUid user)
        {
            var candidates = _parties.EnumerateUnderlings(user).Concat(_stayers.EnumerateStayers(StayingTags.Ally).Select(s => s.Owner));
            
            //var args = new ChooseAllyMenu.Args(candidates)
            //{
            //    WindowTitle = Loc.GetString("Elona.Item.HouseBoard.Stayers.Window.Title"),
            //    Prompt = Loc.GetString("Elona.Item.HouseBoard.Stayers.Prompt"),
            //    XOffset = 20
            //};
        }

        private void HouseBoard_RecruitServant(EntityUid user)
        {
            _servants.QueryHire();
        }

        private void HouseBoard_MoveAStayer(EntityUid user)
        {
            _mes.Display("TODO", UiColors.MesYellow);
        }

        private void GetDefaultHouseBoardActions(EntityUid houseBoard, HouseBoardComponent component, HouseBoardGetActionsEvent args)
        {
            // >>>>>>>> shade2/map_user.hsp:224 	if areaId(gArea)=areaShop{ ..
            // TODO zones
            // TODO shops
            // TODO ranches
            var map = GetMap(houseBoard);

            args.OutActions.Add(new(Loc.GetString("Elona.Item.HouseBoard.Actions.Design"), HouseBoard_Design));

            if (_homes.ActiveHomeIDs.Contains(map.Id))
            {
                args.OutActions.Add(new(Loc.GetString("Elona.Item.HouseBoard.Actions.HomeRank"), HouseBoard_ViewHomeRank));
                args.OutActions.Add(new(Loc.GetString("Elona.Item.HouseBoard.Actions.AlliesInYourHome"), HouseBoard_AlliesInYourHome));
                if (_areaManager.TryGetFloorOfMap(map.Id, out var floor) && floor.Value.FloorNumber == 1)
                {
                    args.OutActions.Add(new(Loc.GetString("Elona.Item.HouseBoard.Actions.RecruitAServant"), HouseBoard_RecruitServant));
                }
                args.OutActions.Add(new(Loc.GetString("Elona.Item.HouseBoard.Actions.MoveAStayer"), HouseBoard_MoveAStayer));
            }
            // <<<<<<<< shade2 / map_user.hsp:240      } ..
        }
    }
}
