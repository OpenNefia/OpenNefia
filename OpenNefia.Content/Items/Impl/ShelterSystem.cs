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

namespace OpenNefia.Content.Items
{
    public interface IShelterSystem : IEntitySystem
    {
    }

    public sealed class ShelterSystem : EntitySystem, IShelterSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<ShelterComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_Shelter);
        }

        private void LocalizeExtra_Shelter(EntityUid uid, ShelterComponent shelter, ref LocalizeItemNameExtraEvent args)
        {
            args.OutFullName.Append(" " + Loc.GetString("Elona.Shelter.ItemName.SerialNumber", ("serialNumber", shelter.SerialNumber)));
        }
    }
}