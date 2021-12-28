﻿using OpenNefia.Content.GameObjects;
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
    [RegisterLocaleFunctions]
    public static class SharedBuiltins
    {
        /// <summary>
        /// Function: name(entity, ignoreSight)
        /// </summary>
        /// <hsp>#defcfunc name int tc</hsp>
        [LocaleFunction("name")]
        public static string BuiltIn_name(object? obj, bool ignoreSight = false)
        {
            if (obj is not EntityUid entity)
            {
                return Loc.Get("Elona.GameObjects.Chara.Something");
            }

            var gameSession = IoCManager.Resolve<IGameSessionManager>();

            if (gameSession.IsPlayer(entity))
                return Loc.Get("Elona.GameObjects.Chara.You");

            var visibilitySys = EntitySystem.Get<VisibilitySystem>();

            if (!visibilitySys.CanSeeEntity(entity, GameSession.Player.Uid))
            {
                return Loc.Get("Elona.GameObjects.Chara.Something");
            }

            return DisplayNameSystem.GetDisplayName(entity);
        }
    }
}
