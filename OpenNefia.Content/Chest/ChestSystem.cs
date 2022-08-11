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

namespace OpenNefia.Content.Chest
{
    public interface IChestSystem : IEntitySystem
    {
    }

    public sealed class ChestSystem : EntitySystem, IChestSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<ChestComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_Chest);
        }

        private void LocalizeExtra_Chest(EntityUid uid, ChestComponent chest, ref LocalizeItemNameExtraEvent args)
        {
            if (chest.DisplayLevelInName)
            {
                args.OutFullName.Append(Loc.Space() + Loc.GetString($"Elona.Chest.ItemName.Level", ("level", chest.LockpickDifficulty)));
            }
            if (!chest.HasItems)
            {
                args.OutFullName.Append(Loc.GetString("Elona.Chest.ItemName.Empty"));
            }
        }
    }
}