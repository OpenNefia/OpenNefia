using OpenNefia.Content.EtherDisease;
using OpenNefia.Content.Items;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Materials
{
    public sealed class VanillaMaterialsSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public void IncreaseEtherDiseaseSpeed(MaterialPrototype proto, ref P_MaterialApplyToEquipperEvent args)
        {
            if (TryComp<EtherDiseaseComponent>(args.Equipper, out var ether))
            {
                ether.EtherDiseaseExtraSpeed.Buffed += 5;
            }
        }
    }

    [PrototypeEvent(typeof(MaterialPrototype))]
    [ByRefEvent]
    public struct P_MaterialApplyToItemEvent
    {
        public EntityUid Item { get; }

        public P_MaterialApplyToItemEvent(EntityUid item)
        {
            Item = item;
        }
    }

    [PrototypeEvent(typeof(MaterialPrototype))]
    [ByRefEvent]
    public struct P_MaterialApplyToEquipperEvent
    {
        public EntityUid Equipper { get; }
        public EntityUid Item { get; }

        public P_MaterialApplyToEquipperEvent(EntityUid equipper, EntityUid item)
        {
            Equipper = equipper;
            Item = item;
        }
    }
}