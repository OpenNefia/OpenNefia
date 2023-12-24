using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Utility;
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
        public static string BuiltIn_name(object? obj, bool? ignoreSight = null, int? amount = null)
        {
            if (obj is string s)
                return s;

            EntityUid entity;
            if (obj is IMap map)
            {
                entity = map.MapEntityUid;
            }
            else if (obj is IArea area)
            {
                entity = area.AreaEntityUid;
            }
            else if (obj is EntityUid ent)
            {
                entity = ent;
            }
            else
            {
                return Loc.GetString("Elona.GameObjects.Common.Something");
            }

            var gameSession = IoCManager.Resolve<IGameSessionManager>();

            if (gameSession.IsPlayer(entity))
                return Loc.GetString("Elona.GameObjects.Common.You");

            var visibilitySys = EntitySystem.Get<VisibilitySystem>();

            if (!visibilitySys.CanSeeEntity(GameSession.Player, entity) && !(ignoreSight ?? false))
            {
                return Loc.GetString("Elona.GameObjects.Common.Something");
            }

            return EntitySystem.Get<IDisplayNameSystem>().GetDisplayName(entity, noArticle: false, amount: amount);
        }

        [LocaleFunction("basename")]
        public static string BuiltIn_basename(object? obj)
        {
            if (obj is string s)
                return s;

            if (obj is not EntityUid entity)
                return Loc.GetString("Elona.GameObjects.Common.Something");
            
            return EntitySystem.Get<IDisplayNameSystem>().GetBaseName(entity);
        }

        /// <summary>
        /// returns the ordnial short form for a number; 1 = st, 2 = nd, 3 = rd, ect.
        /// </summary>
        [LocaleFunction("ordinal")]
        public static string BuiltIn_ordinal(object? obj)
        {
            if (obj is not long longNum)
                return string.Empty;

            var number = Convert.ToInt32(longNum);
            var last = (number % 10);
            return GetOrdinal(number) ?? GetOrdinal(last) ?? "th";
        }

        private static string? GetOrdinal(int number)
        {
            return number switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                var num when num >= 11 && num <= 13 => "th",
                _ => null
            };
        }

        [LocaleFunction("quote")]
        public static string BuiltIn_quote(object? obj)
        {
            // TODO probably needs to be useable before all locale files have been fully loaded
            if (Loc.Language == LanguagePrototypeOf.Japanese)
                return $"﻿「{obj}」";
            else
                return $"\"{obj}\"";
        }

        [LocaleFunction("capitalize")]
        public static string BuiltIn_Capitalize(object? obj)
        {
            return $"{obj}".FirstCharToUpper();
        }

        [LocaleFunction("loc")]
        public static string BuiltIn_Loc(object? obj, params object[] rest)
        {
            LocaleArg[] args = new LocaleArg[rest.Length];
            for (var i = 0; i < rest.Length; i++)
            {
                args[i] = ($"arg{i}", rest[i]);
            }
            return Loc.GetString(obj?.ToString() ?? "", args);
        }
            
        [LocaleFunction("gender")]
        public static string BuiltIn_Gender(object? obj)
        {
            var gender = Gender.Male;

            if (obj is EntityUid entity)
                gender = IoCManager.Resolve<IEntityManager>().GetComponentOrNull<CharaComponent>(entity)?.Gender ?? gender;

            return gender.ToString();
        }

        [LocaleFunction("player")]
        public static EntityUid BuiltIn_Player()
        {
            return IoCManager.Resolve<IGameSessionManager>().Player;
        }

        [LocaleFunction("alias")]
        public static string BuiltIn_Alias(object? obj)
        {
            if (obj is not EntityUid entity)
                return "";

            return IoCManager.Resolve<IEntityManager>()
                .GetComponentOrNull<AliasComponent>(entity)?.Alias ?? "";
        }
    }
}