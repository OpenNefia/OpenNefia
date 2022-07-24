﻿using OpenNefia.Content.Maps;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.Sleep;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
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
        private void Initialize_PreparingToSleep()
        {
            SubscribeComponent<ActivityPreparingToSleepComponent, OnActivityStartEvent>(PreparingToSleep_OnStart);
            SubscribeComponent<ActivityPreparingToSleepComponent, OnActivityPassTurnEvent>(PreparingToSleep_OnPassTurn);
            SubscribeComponent<ActivityPreparingToSleepComponent, OnActivityFinishEvent>(PreparingToSleep_OnFinish);
        }

        private void PreparingToSleep_OnStart(EntityUid uid, ActivityPreparingToSleepComponent component, OnActivityStartEvent args)
        {
            var actor = args.Activity.Actor;

            if (!TryMap(actor, out var map))
            {
                _activities.RemoveActivity(actor);
                return;
            }

            if (HasComp<MapTypePlayerOwnedComponent>(map.MapEntityUid)
                || HasComp<MapTypeTownComponent>(map.MapEntityUid)
                || HasComp<MapTypeGuildComponent>(map.MapEntityUid))
            {
                _mes.Display(Loc.GetString("Elona.Sleep.Activity.Start.LieDown"));
            }
            else
            {
                _mes.Display(Loc.GetString("Elona.Sleep.Activity.Start.StartToCamp"));
            }

            if (IsAlive(component.Bed))
                _inUse.SetItemInUse(actor, component.Bed.Value);
        }

        private void PreparingToSleep_OnPassTurn(EntityUid uid, ActivityPreparingToSleepComponent component, OnActivityPassTurnEvent args)
        {
            if (IsAlive(component.Bed))
                _inUse.SetItemInUse(args.Activity.Actor, component.Bed.Value);
        }

        private void PreparingToSleep_OnFinish(EntityUid uid, ActivityPreparingToSleepComponent component, OnActivityFinishEvent args)
        {
            _mes.Display(Loc.GetString("Elona.Sleep.Activity.Finish"));
            _sleep.Sleep(args.Activity.Actor, component.Bed);
            if (IsAlive(component.Bed))
                _inUse.RemoveItemInUse(args.Activity.Actor, component.Bed.Value);
        }
    }
}
