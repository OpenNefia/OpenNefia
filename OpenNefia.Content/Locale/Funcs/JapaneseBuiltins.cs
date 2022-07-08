using OpenNefia.Content.DisplayName;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Visibility;
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
    }
}
