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
    public interface ILovePotionSpikedSystem : IEntitySystem
    {
    }

    public sealed class LovePotionSpikedSystem : EntitySystem, ILovePotionSpikedSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<LovePotionSpikedComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_LovePotionSpiked);
        }

        private void LocalizeExtra_LovePotionSpiked(EntityUid uid, LovePotionSpikedComponent component, ref LocalizeItemNameExtraEvent args)
        {
            args.OutFullName.Append(Loc.GetString("Elona.Potion.ItemName.Aphrodisiac"));
        }
    }
}