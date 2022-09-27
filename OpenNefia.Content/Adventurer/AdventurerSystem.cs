using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Loot;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Roles;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Adventurer
{
    public interface IAdventurerSystem : IEntitySystem
    {
        bool TryGetArea(EntityUid adventurer, [NotNullWhen(true)] out IArea? area);
    }

    public sealed class AdventurerSystem : EntitySystem, IAdventurerSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IWorldSystem _world = default!;

        public override void Initialize()
        {
            SubscribeEntity<MapOnTimePassedEvent>(ProcUpdateAdventurers);
            SubscribeComponent<HiredAdventurerComponent, BeforeDropItemsOnDeathEvent>(HandleBeforeDropItems);
            SubscribeComponent<RoleAdventurerComponent, AfterDroppedItemsOnDeathEvent>(HandleAfterDroppedLoot);
            SubscribeComponent<RoleAdventurerComponent, CharaPlaceFailureEvent>(Adventurer_CharaPlaceFailure);
        }

        private void HandleBeforeDropItems(EntityUid uid, HiredAdventurerComponent component, BeforeDropItemsOnDeathEvent args)
        {
            if (args.Handled)
                return;

            args.OutDroppedItems.Clear();
            args.Handled = true;
        }

        public bool TryGetArea(EntityUid value, [NotNullWhen(true)] out IArea? area)
        {
            // TODO
            return TryArea(value, out area);
        }

        private void ProcUpdateAdventurers(EntityUid uid, ref MapOnTimePassedEvent args)
        {
            if (args.HoursPassed <= 0)
                return;

            // TODO
        }

        private void HandleAfterDroppedLoot(EntityUid uid, RoleAdventurerComponent component, AfterDroppedItemsOnDeathEvent args)
        {
            // TODO regenerate equipment
        }

        private void Adventurer_CharaPlaceFailure(EntityUid uid, RoleAdventurerComponent component, CharaPlaceFailureEvent args)
        {
            if (!TryComp<CharaComponent>(uid, out var chara))
                return;

            chara.Liveness = CharaLivenessState.AdventurerHospital;
            chara.RevivalDate = _world.State.GameDate + GameTimeSpan.FromHours(24 + _rand.Next(24 / 2));
        }
    }
}