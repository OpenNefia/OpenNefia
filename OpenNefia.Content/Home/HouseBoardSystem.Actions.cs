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
using OpenNefia.Core.Audio;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.ChooseNPC;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Log;
using Love;
using OpenNefia.Content.Currency;
using OpenNefia.Core.Game;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.Factions;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Activity;

namespace OpenNefia.Content.Home
{
    public sealed partial class HouseBoardSystem
    {
        [Dependency] private readonly IHomeSystem _homes = default!;
        [Dependency] private readonly IServantSystem _servants = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IStayersSystem _stayers = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

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
            var map = GetMap(user);
            var candidates = _parties.EnumerateUnderlings(user).Where(e => Spatial(e).MapID == map.Id);

            var args = new ChooseAllyMenu.Args(candidates)
            {
                WindowTitle = Loc.GetString("Elona.Stayers.Manage.Window.Title"),
                Prompt = Loc.GetString("Elona.Stayers.Manage.Prompt"),
                XOffset = 20
            };

            var cancelled = false;

            while (!cancelled)
            {
                var result = _uiManager.Query<ChooseAllyMenu, ChooseAllyMenu.Args, ChooseAllyMenu.Result>(args);
                if (result.HasValue)
                {
                    _audio.Play(Protos.Sound.Ok1);

                    _mes.Newline();
                    var ally = result.Value.Selected;

                    if (_stayers.IsStaying(ally))
                    {
                        _stayers.UnregisterStayer(ally);
                        _mes.Display(Loc.GetString("Elona.Stayers.Manage.Remove.Ally", ("entity", ally)));
                    }
                    else
                    {
                        _stayers.RegisterStayer(ally, map, StayingTags.Ally, Spatial(ally).LocalPosition);
                        _mes.Display(Loc.GetString("Elona.Stayers.Manage.Add.Ally", ("entity", ally)));
                    }
                }
                else
                {
                    cancelled = true;
                }
            }
        }

        private void HouseBoard_RecruitServant(EntityUid user)
        {
            _servants.QueryHire();
        }

        private void HouseBoard_MoveServant(EntityUid user)
        {
            var map = GetMap(user);
            var candidates = _lookup.EntityQueryInMap<ServantComponent>(map).Select(s => s.Owner);
            var args = new ChooseNPCMenu.Args(candidates)
            {
                Behavior = new ServantInfoBehavior(_servants),
                Prompt = Loc.GetString("Elona.Servant.Move.Prompt.Who")
            };
            var result = _uiManager.Query<ChooseNPCMenu, ChooseNPCMenu.Args, ChooseNPCMenu.Result>(args);

            if (result.HasValue)
            {
                var chara = result.Value.Selected;
                if (_factions.GetRelationToPlayer(chara) <= Relation.Enemy)
                {
                    _mes.Display(Loc.GetString("Elona.Servant.Move.DontTouchMe", ("entity", chara)));
                }
                else
                {
                    _audio.Play(Protos.Sound.Ok1);
                    var spatial = Spatial(chara);
                    var origPos = spatial.MapPosition;
                    while (true)
                    {
                        _mes.Newline();
                        _mes.Display(Loc.GetString("Elona.Servant.Move.Prompt.Where", ("entity", chara)));
                        var posArgs = new PositionPrompt.Args(origPos);
                        var newPos = _uiManager.Query<PositionPrompt, PositionPrompt.Args, PositionPrompt.Result>(posArgs);
                        if (!newPos.HasValue)
                            break;
                        if (!map.CanAccess(newPos.Value.Coords) || !newPos.Value.Coords.TryToEntity(_mapManager, out var entityCoords))
                        {
                            _mes.Display(Loc.GetString("Elona.Servant.Move.Invalid"));
                        }
                        else
                        {
                            spatial.Coordinates = entityCoords;
                            var aiAnchor = EnsureComp<AIAnchorComponent>(chara);
                            aiAnchor.InitialPosition = entityCoords.Position;
                            aiAnchor.Anchor = aiAnchor.InitialPosition;
                            _activities.RemoveActivity(chara);
                            _mes.Newline();
                            _mes.Display(Loc.GetString("Elona.Servant.Move.IsMoved", ("entity", chara)));
                            _audio.Play(Protos.Sound.Foot, chara);
                            break;
                        }
                    }
                }
            }

            _field.RefreshScreen();
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
                args.OutActions.Add(new(Loc.GetString("Elona.Item.HouseBoard.Actions.MoveAStayer"), HouseBoard_MoveServant));
            }
            // <<<<<<<< shade2 / map_user.hsp:240      } ..
        }
    }
}
