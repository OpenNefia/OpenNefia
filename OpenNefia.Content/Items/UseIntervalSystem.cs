using Love;
using OpenNefia.Content.DisplayName;
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
using OpenNefia.Content.World;

namespace OpenNefia.Content.Items
{
    public interface IUseIntervalSystem : IEntitySystem
    {
    }

    public sealed class UseIntervalSystem : EntitySystem, IUseIntervalSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        
        public override void Initialize()
        {
            SubscribeComponent<UseIntervalComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_UseInterval);
        }

        private void LocalizeExtra_UseInterval(EntityUid uid, UseIntervalComponent useInterval, ref LocalizeItemNameExtraEvent args)
{
            if (_world.State.GameDate < useInterval.DateNextUseableOn)
{
                args.OutFullName.Append(Loc.GetString("Elona.Item.ItemName.UseInterval", ("hours", _world.State.GameDate.TotalHours - useInterval.DateNextUseableOn.TotalHours)));
            }
        }
    }
}