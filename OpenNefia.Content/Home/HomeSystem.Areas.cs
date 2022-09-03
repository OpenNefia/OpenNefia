using OpenNefia.Content.Areas;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Locale;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Sidequests;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Roles;
using OpenNefia.Content.Book;

namespace OpenNefia.Content.Home
{
    public sealed partial class HomeSystem
    {
        [Dependency] private readonly ISidequestSystem _sidequests = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;

        private void Initialize_Areas()
        {
            SubscribeEntity<GetAreaEntranceMessageEvent>(GetHomeEntranceMessage);
            SubscribeComponent<MapComponent, GetDisplayNameEventArgs>(AreaHome_GetDisplayName);
            SubscribeComponent<AreaHomeTutorialComponent, AfterAreaFloorGeneratedEvent>(AreaHomeTutorial_FloorGenerate);
        }

        private void AreaHome_GetDisplayName(EntityUid uid, MapComponent component, ref GetDisplayNameEventArgs args)
        {
            if (!TryArea(uid, out var area) || !HasComp<AreaHomeComponent>(area.AreaEntityUid) || !area.TryGetFloorOfContainedMap(component.MapId, out var floorId))
                return;

            if (ActiveHomeID == component.MapId)
                args.OutName = Loc.GetString("Elona.Home.Map.Name");

            var floorNo = floorId.Value.FloorNumber;

            if (floorNo > 0)
                args.OutName += $" B.{floorNo}";
            else if (floorNo < 0)
                args.OutName += $" L.{Math.Abs(floorNo)}";
        }

        private void GetHomeEntranceMessage(EntityUid areaEntity, GetAreaEntranceMessageEvent args)
        {
            if (!TryArea(areaEntity, out var area))
                return;

            foreach (var floor in area.ContainedMaps.Values)
            {
                if (floor.MapId == ActiveHomeID)
                {
                    args.OutMessage = Loc.GetString("Elona.Home.Map.Description");
                    return;
                }
            }
        }

        private void AreaHomeTutorial_FloorGenerate(EntityUid uid, AreaHomeTutorialComponent component, AfterAreaFloorGeneratedEvent args)
        {
            // >>>>>>>> shade2/map.hsp:877 	 		if gHomeLevel=0{ ..
            if (args.FloorId.FloorNumber != 0)
                return;

            if (_sidequests.GetState(Protos.Sidequest.MainQuest) != 0)
                return;

            var ent = _charaGen.GenerateChara(args.Map.AtPos(18, 10), Protos.Chara.Larnneire);
            if (IsAlive(ent))
            {
                EnsureComp<RoleSpecialComponent>(ent.Value);
            }

            ent = _charaGen.GenerateChara(args.Map.AtPos(16, 11), Protos.Chara.Lomias);
            if (IsAlive(ent))
            {
                EnsureComp<RoleSpecialComponent>(ent.Value);
            }

            _itemGen.GenerateItem(args.Map.AtPos(6, 10), Protos.Item.HeirTrunk);
            _itemGen.GenerateItem(args.Map.AtPos(15, 19), Protos.Item.SalaryChest);
            _itemGen.GenerateItem(args.Map.AtPos(9, 8), Protos.Item.Freezer);

            ent = _itemGen.GenerateItem(args.Map.AtPos(18, 19), Protos.Item.Book);
            if (IsAlive(ent) && TryComp<BookComponent>(ent.Value, out var book))
            {
                book.BookID = Protos.Book.BeginnersGuide;
            }
            // <<<<<<<< shade2/map.hsp:884 				flt:item_create -1,idBook,18,19:iBookId(ci)=1 ..
        }
    }
}
