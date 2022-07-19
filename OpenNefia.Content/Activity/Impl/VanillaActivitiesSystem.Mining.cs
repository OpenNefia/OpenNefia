using OpenNefia.Content.Mining;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI;
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

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem
    {
        [Dependency] private readonly IActionDigSystem _actionDig = default!;

        private void Initialize_Mining()
        {
            SubscribeComponent<ActivityMiningComponent, OnActivityStartEvent>(Mining_OnStart);
            SubscribeComponent<ActivityMiningComponent, OnActivityPassTurnEvent>(Mining_OnPassTurn);
            SubscribeComponent<ActivityMiningComponent, OnActivityFinishEvent>(Mining_OnFinish);
        }

        private void Mining_OnStart(EntityUid uid, ActivityMiningComponent component, OnActivityStartEvent args)
        {
            if (!TryMap(component.TargetTile, out var map) || !map.TryGetTilePrototype(component.TargetTile.Position, out var tile))
            {
                _activities.RemoveActivity(uid);
                return;
            }
                
            _mes.Display(Loc.GetString("Elona.Dig.Mining.Start.Wall"));
            if (tile.Kind == TileKind.HardWall)
                _mes.Display(Loc.GetString("Elona.Dig.Mining.Start.HardWall"));

            component.TurnsSpentMining = 0;
        }

        private void Mining_OnPassTurn(EntityUid uid, ActivityMiningComponent component, OnActivityPassTurnEvent args)
        {
            var activity = args.Activity;
            var actor = activity.Actor;

            if (_rand.OneIn(5))
                _damage.DamageStamina(actor, 1);

            component.TurnsSpentMining++;

            var success = _actionDig.CheckMiningSuccess(actor, component.TargetTile, component.TurnsSpentMining);

            if (success)
            {
                _actionDig.FinishMiningWall(actor, component.TargetTile);
                _activities.RemoveActivity(actor);
                return;
            }
            else
            {
                if (CompOrNull<TurnOrderComponent>(actor)?.TotalTurnsTaken % 5 == 0) 
                    _mes.Display(Loc.GetString("Elona.Dig.Sound"), UiColors.MesBlue);
            }
        }

        private void Mining_OnFinish(EntityUid uid, ActivityMiningComponent component, OnActivityFinishEvent args)
        {
            _mes.Display(Loc.GetString("Elona.Dig.Mining.Fail"));
        }
    }
}
