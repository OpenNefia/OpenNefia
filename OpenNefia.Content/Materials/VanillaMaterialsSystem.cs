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
        public void IncreaseEtherDiseaseSpeed(MaterialPrototype proto, ref P_MaterialApplyToEquipperEvent args)
        {
            if (TryComp<EtherDiseaseComponent>(args.Equipper, out var ether))
            {
                ether.EtherDiseaseExtraSpeed.Buffed += 5;
            }
        }
    }

    /// <summary>
    /// Event for applying buffed material properties to an item, on refresh.
    /// </summary>
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

    /// <summary>
    /// Event for applying buffed material properties to an item's equipper, on refresh.
    /// </summary>
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