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
    public interface IPoisonSpikedSystem : IEntitySystem
    {
    }

    public sealed class PoisonSpikedSystem : EntitySystem, IPoisonSpikedSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<PoisonSpikedComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_PoisonSpiked);
        }

        private void LocalizeExtra_PoisonSpiked(EntityUid uid, PoisonSpikedComponent component, ref LocalizeItemNameExtraEvent args)
        {
            args.OutFullName.Append(Loc.GetString("Elona.Potion.ItemName.Poisoned"));
        }
    }
}