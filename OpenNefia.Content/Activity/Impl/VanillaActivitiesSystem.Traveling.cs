using OpenNefia.Content.Maps;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.World;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem
    {
        [Dependency] private readonly IMoveableSystem _moveable = default!;

        private const int TravelDistancePerTile = 4;

        private void Initialize_Traveling()
        {
            SubscribeEntity<BeforeMoveEventArgs>(ProcWorldMapTravel);

            SubscribeComponent<ActivityTravelingComponent, OnActivityPassTurnEvent>(Traveling_OnPassTurn);
            SubscribeComponent<ActivityTravelingComponent, OnActivityFinishEvent>(Traveling_OnFinish);
        }

        private bool _travelFinished = false;

        private void ProcWorldMapTravel(EntityUid uid, BeforeMoveEventArgs args)
        {
            if (args.Handled)
                return;

            // Traveling logic for the HSP version:
            //
            // When the player is in the world map and tries to move onto another tile, the game
            // ends their turn early and gives them the "traveling" activity. That gets finished
            // instantly as it has no animation delay. Each turn the activity runs it does the
            // traveling logic and *also* calls the "move to this square" routine (*act_move) again,
            // over and over. When the activity finishes it sets the `travelDone` boolean, and then
            // when *act_move gets called immediately after it will finally update the player's
            // position, doing all the normal movement things.

            if (!_gameSession.IsPlayer(uid) || !TryMap(uid, out var map) || !HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
                return;

            if (_travelFinished)
            {
                _travelFinished = false;
                // Proceed with the regular movement routine.
            }
            else
            {
                if (!_activities.HasActivity(uid, Protos.Activity.Traveling))
                {
                    if (_activities.StartActivity(uid, Protos.Activity.Traveling, out var activityComp))
                    {
                        // Prevent the player from moving immediately and defer to when the
                        // traveling activity finishes.
                        var traveling = EnsureComp<ActivityTravelingComponent>(activityComp.Owner);
                        traveling.Destination = args.OutNewPosition;

                        args.Handle(TurnResult.Succeeded);
                    }
                }
            }
        }

        private void Traveling_OnPassTurn(EntityUid uid, ActivityTravelingComponent component, OnActivityPassTurnEvent args)
        {
            var ev = new OnTravelInWorldMapEvent(args.Activity);
            RaiseEvent(args.Activity.Actor, ref ev);

            _world.PassTime(GameTimeSpan.FromMinutes(1));
        }

        private void Traveling_OnFinish(EntityUid uid, ActivityTravelingComponent component, OnActivityFinishEvent args)
        {
            _world.State.TravelDistance += TravelDistancePerTile;
            _travelFinished = true;

            // This will trigger BeforeMoveEventArgs again and run the above event handler.
            _moveable.MoveEntity(args.Activity.Actor, component.Destination);
        }
    }

    [ByRefEvent]
    public struct OnTravelInWorldMapEvent
    {
        public ActivityComponent Activity { get; }

        public OnTravelInWorldMapEvent(ActivityComponent activity)
        {
            Activity = activity;
        }
    }
}