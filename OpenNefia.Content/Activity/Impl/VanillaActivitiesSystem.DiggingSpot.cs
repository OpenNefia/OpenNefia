using OpenNefia.Content.Maps;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem
    {
        [Dependency] private readonly IMapDebrisSystem _mapDebris = default!;

        private void Initialize_DiggingSpot()
        {
            SubscribeComponent<ActivityDiggingSpotComponent, OnActivityStartEvent>(DiggingSpot_OnStart);
            SubscribeComponent<ActivityDiggingSpotComponent, OnActivityPassTurnEvent>(DiggingSpot_OnPassTurn);
            SubscribeComponent<ActivityDiggingSpotComponent, OnActivityFinishEvent>(DiggingSpot_OnFinish);
        }

        private void DiggingSpot_OnStart(EntityUid activity, ActivityDiggingSpotComponent component, OnActivityStartEvent args)
        {
            if (!TryMap(component.TargetTile, out var map))
            {
                args.Cancel();
                return;
            }

            _mes.Display(Loc.GetString("Elona.Dig.Spot.Start"));
        }

        private void DiggingSpot_OnPassTurn(EntityUid activity, ActivityDiggingSpotComponent component, OnActivityPassTurnEvent args)
        {
            if (args.Activity.TurnsRemaining % 5 == 0)
                _mes.Display(Loc.GetString("Elona.Dig.Sound"), UiColors.MesBlue);
        }

        private void DiggingSpot_OnFinish(EntityUid activity, ActivityDiggingSpotComponent component, OnActivityFinishEvent args)
        {
            _mes.Display(Loc.GetString("Elona.Dig.Spot.Finish"));
            var ev = new EntityFinishedDiggingSpotEvent(component.TargetTile);
            RaiseEvent(ev);
            _mapDebris.SpillFragments(component.TargetTile, 1);
        }
    }

    public sealed class EntityFinishedDiggingSpotEvent : EntityEventArgs
    {
        public MapCoordinates TargetTile { get; }

        public EntityFinishedDiggingSpotEvent(MapCoordinates targetTile)
        {
            TargetTile = targetTile;
        }
    }
}
