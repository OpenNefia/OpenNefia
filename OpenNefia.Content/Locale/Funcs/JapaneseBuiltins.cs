using OpenNefia.Content.DisplayName;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.World;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Locale.Funcs
{
    [RegisterLocaleFunctions("ja_JP")]
    public static class JapaneseBuiltins
    {        
        [LocaleFunction("sore_wa")]
        public static string BuiltIn_sore_wa(object? obj, bool ignoreSight = false)
        {
            if (obj is string s)
                return s;

            if (obj is not EntityUid entity)
                return "それは";

            var gameSession = IoCManager.Resolve<IGameSessionManager>();

            if (gameSession.IsPlayer(entity))
                return "";

            var visibilitySys = EntitySystem.Get<VisibilitySystem>();

            if (!visibilitySys.CanSeeEntity(GameSession.Player, entity) && !ignoreSight)
            {
                return "それは";
            }

            return EntitySystem.Get<IDisplayNameSystem>().GetDisplayName(entity) + "は";
        }

        /// <hsp>#defcfunc cnvDate int d,int mode</hsp>
        [LocaleFunction("format_date")]
        public static string BuiltIn_format_date(object? obj, string? mode = null)
        {
            if (obj is not GameDateTime dateTime)
                return "";

            var s = $"﻿{dateTime.Year}﻿年{dateTime.Month}﻿月{dateTime.Day}日";

            if (mode == "hour")
                s += $"﻿{dateTime.Hour}時";

            return s;
        }
    }
}
