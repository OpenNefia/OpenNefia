using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Quests;
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

namespace OpenNefia.Content.Items.Impl
{
    public sealed class WellSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<WellComponent, InitEntityInDerivedQuestMapEvent>(InitDerivedQuest_Well);
            SubscribeComponent<HolyWellComponent, InitEntityInDerivedQuestMapEvent>(InitDerivedQuest_HolyWell);
        }

        private void InitDerivedQuest_Well(EntityUid uid, WellComponent component, InitEntityInDerivedQuestMapEvent args)
        {
            // >>>>>>>> elona122/shade2/map_rand.hsp:732 	if (iId(cnt)=idWell)or(iId(cnt)=173):iParam1(cnt) ...
            component.WaterAmount -= 10;
            // <<<<<<<< elona122/shade2/map_rand.hsp:732 	if (iId(cnt)=idWell)or(iId(cnt)=173):iParam1(cnt) ...
        }

        private void InitDerivedQuest_HolyWell(EntityUid uid, HolyWellComponent component, InitEntityInDerivedQuestMapEvent args)
        {
            // >>>>>>>> elona122/shade2/map_rand.hsp:732 	if (iId(cnt)=idWell)or(iId(cnt)=173):iParam1(cnt) ...
            component.WaterAmount -= 10;
            // <<<<<<<< elona122/shade2/map_rand.hsp:732 	if (iId(cnt)=idWell)or(iId(cnt)=173):iParam1(cnt) ...
        }
    }
}