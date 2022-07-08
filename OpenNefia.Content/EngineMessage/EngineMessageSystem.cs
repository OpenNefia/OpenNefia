using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Content.UI;
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

namespace OpenNefia.Content.EngineMessage
{
    public sealed class EngineMessageSystem : EntitySystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;

        private DateTime _playTimeWarnTimestamp = DateTime.Now;
        private int _hoursPlayed = 0;

        public override void Initialize()
        {
            SubscribeBroadcast<GameInitiallyLoadedEventArgs>(ShowStartupMessage);
            SubscribeBroadcast<PlayerFrameUpdateEventArgs>(CheckTimeWarnMessage);
        }

        private void ShowStartupMessage(GameInitiallyLoadedEventArgs ev)
        {
            var version = "1.22"; // TODO
            _mes.Display($"  Lafrontier presents Elona ver {version}. Welcome traveler! ");
        }

        private void CheckTimeWarnMessage(PlayerFrameUpdateEventArgs ev)
        {
            // >>>>>>>> shade2/main.hsp:1020 	if (gmsec()-time_warn)>3600{ ...
            var time = DateTime.Now;
            var hoursDiff = (time - _playTimeWarnTimestamp).Hours;
            if (hoursDiff >= 1)
            {
                _playTimeWarnTimestamp = time;
                _hoursPlayed += hoursDiff;
                _mes.Display(Loc.GetString("Elona.EngineMessage.PlayTime.Report", ("hoursPlayed", _hoursPlayed)), UiColors.MesYellow);

                // >>>>>>>> shade2/etc.hsp:402 *time_warn_talk ...
                if (Loc.TryGetString($"Elona.EngineMessage.PlayTime.WarnMessage.{_hoursPlayed}", out var timeWarnMes))
                    _mes.Display(timeWarnMes, UiColors.MesYellow);
                // <<<<<<<< shade2/etc.hsp:413  ..
            }
            // <<<<<<<< shade2/main.hsp:1024 		} ..
        }
    }
}