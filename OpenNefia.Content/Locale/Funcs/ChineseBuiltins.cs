using OpenNefia.Content.Charas;
using OpenNefia.Content.GameObjects;
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
    [RegisterLocaleFunctions("zh_CN")]
    public static class ChineseBuiltins
    {
        /// <summary>
        /// Function: s(entity, needE)
        /// </summary>
        /// <hsp>#defcfunc _s int tg, int op</hsp>
        [LocaleFunction("s")]
        public static string BuiltIn_s(object? obj, bool needE = false)
        {
            if (needE)
                return "们";
            return "";
        }

        /// <summary>
        /// Function: has(entity, needE)
        /// </summary>
        /// <hsp>#defcfunc have int have_charid</hsp>
        [LocaleFunction("has")]
        public static string BuiltIn_have(object? obj, bool needE = false)
        {
            return "有";
        }

        /// <summary>
        /// Function: is(entity_or_integer)
        /// </summary>
        /// <hsp>#defcfunc is int is_charid</hsp>
        /// <hsp>#defcfunc is2 int is2_arg1</hsp>
        [LocaleFunction("is")]
        public static string BuiltIn_is(object? obj)
        {
            return "是";
        }

        [LocaleFunction("is_not")]
        public static string BuiltIn_is_not(object? obj)
        {
            return "不是";
        }

        /// <summary>
        /// Function: he(entity)
        /// </summary>
        [LocaleFunction("he")]
        public static string BuiltIn_he(object? obj)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt == 1)
                        return "它";
                    else
                        return "它们";

                case long objLong:
                    if (objLong == 1L)
                        return "它";
                    else
                        return "它们";

                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();
                    if (gameSession.IsPlayer(objEntity))
                    {
                        return "你";
                    }

                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out CharaComponent chara))
                    {
                        switch (chara.Gender)
                        {
                            case Gender.Female:
                                return "她";
                            case Gender.Male:
                                return "他";
                            default:
                                return "他们";
                        }
                    }

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count != 1)
                            return "它们";
                    }

                    return "它";

                default:
                    return "它";
            }
        }

        /// <summary>
        /// Function: his(entity)
        /// </summary>
        /// <hsp>#defcfunc his int tg,int mode</hsp>
        [LocaleFunction("his")]
        public static string BuiltIn_his(object? obj)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt == 1)
                        return "它的";
                    else
                        return "他们的";

                case long objLong:
                    if (objLong == 1L)
                        return "它的";
                    else
                        return "他们的";

                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();
                    if (gameSession.IsPlayer(objEntity))
                    {
                        return "你的";
                    }

                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out CharaComponent chara))
                    {
                        switch (chara.Gender)
                        {
                            case Gender.Female:
                                return "他的";
                            case Gender.Male:
                                return "她的";
                            default:
                                return "他们的";
                        }
                    }

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count != 1)
                            return "它们的";
                    }

                    return "它的";

                default:
                    return "它的";
            }
        }

        private static string GetPossessiveSuffix(object? obj)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt == 1)
                        return "'";
                    else
                        return "们";

                case long objLong:
                    if (objLong == 1L)
                        return "'";
                    else
                        return "们";

                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();
                    if (gameSession.IsPlayer(objEntity))
                    {
                        return "r";
                    }

                    return "们";

                default:
                    return "们";
            }
        }

        /// <summary>
        /// Function: possessive(entity)
        /// </summary>
        /// <remarks>
        /// Equivalent to (name(entity) + 's/'/r), which is ("you" + "r") for the player.
        /// </remarks>
        /// <hsp>#defcfunc your int tg,int mode</hsp>
        [LocaleFunction("possessive")]
        public static string BuiltIn_his_named(object? obj)
        {
            return SharedBuiltins.BuiltIn_name(obj) + GetPossessiveSuffix(obj);
        }

        /// <summary>
        /// Function: him(entity)
        /// </summary>
        /// <remarks>
        /// NOTE: This serves a different purpose than him() in the HSP code. It
        /// does not account for targeting, so it will not display "yourself" if the
        /// player is passed as the argument. Use <c>theTarget(source, target)</c> instead
        /// if that's needed.
        /// </remarks>
        [LocaleFunction("him")]
        public static string BuiltIn_him(object? obj)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt == 1)
                        return "它";
                    else
                        return "它们";

                case long objLong:
                    if (objLong == 1L)
                        return "它";
                    else
                        return "它们";

                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();
                    if (gameSession.IsPlayer(objEntity))
                    {
                        return "你";
                    }

                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out CharaComponent chara))
                    {
                        switch (chara.Gender)
                        {
                            case Gender.Female:
                                return "她";
                            case Gender.Male:
                                return "他";
                            default:
                                return "他们";
                        }
                    }

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count != 1)
                            return "它们";
                    }

                    return "它";

                default:
                    return "它";
            }
        }

        /// <summary>
        /// Function: theTarget(sourceEntity, targetEntity)
        /// 
        /// Returns "herself"/"himself" if the target is the same as the source, 
        /// otherwise "her"/"him". This is used for targeted spells, so that they can
        /// show up as "you smack yourself with a putitoro".
        /// </summary>
        /// <hsp>#defcfunc him int him_charid, int him_requiresgender</hsp>
        [LocaleFunction("theTarget")]
        public static string BuiltIn_theTarget(object? source, object? target)
        {
            if (target is not EntityUid targetEnt)
            {
                if (source == target)
                    return "它自己";
                else
                    return "它";
            }

            // Check if the target is different than the source, and return "you"/"her"
            // if so.
            if (source is not EntityUid sourceEnt || sourceEnt != targetEnt)
                return BuiltIn_him(targetEnt);

            // Handle "yourself"/"herself".
            return BuiltIn_himself(targetEnt);
        }

        [LocaleFunction("himself")]
        public static string BuiltIn_himself(object? obj)
        {
            if (obj is not EntityUid objEntity)
            {
                return "他自己";
            }

            // Handle "yourself"/"herself".
            var gameSession = IoCManager.Resolve<IGameSessionManager>();

            if (gameSession.IsPlayer(objEntity))
            {
                return "你自己";
            }

            var entMan = IoCManager.Resolve<IEntityManager>();

            if (entMan.TryGetComponent(objEntity, out CharaComponent chara))
            {
                switch (chara.Gender)
                {
                    case Gender.Female:
                        return "她自己";
                    case Gender.Male:
                        return "他自己";
                    default:
                        return "他们自己";
                }
            }

            if (entMan.TryGetComponent(objEntity, out StackComponent stack))
            {
                if (stack.Count != 1)
                    return "他们自己";
            }

            return "它自己";
        }

        /// <hsp>#defcfunc cnvDate int d,int mode</hsp>
        [LocaleFunction("format_date")]
        public static string BuiltIn_format_date(object? obj, string? mode = null)
        {
            if (obj is not GameDateTime dateTime)
                return "";

            var s = $"﻿{dateTime.Year} {dateTime.Month}﻿/{dateTime.Day}";

            if (mode == "hour")
                s += $"﻿ {dateTime.Hour}h";

            return s;
        }

        [LocaleFunction("plural")]
        public static string BuiltIn_plural(object? obj, bool needE = false)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt == 1)
                        return "";
                    else if (needE)
                        return "们";
                    else
                        return "们";

                case long objLong:
                    if (objLong == 1)
                        return "";
                    else if (needE)
                        return "们";
                    else
                        return "们";

                case EntityUid objEntity:
                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count == 1)
                            return "";
                        else if (needE)
                            return "们";
                        else
                            return "们";
                    }

                    return "";

                default:
                    return "";
            }
        }

        [LocaleFunction("does")]
        public static string BuiltIn_does(object? obj)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt == 1)
                        return "does";
                    else
                        return "do";

                case long objLong:
                    if (objLong == 1)
                        return "does";
                    else
                        return "do";

                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();

                    if (gameSession.IsPlayer(objEntity))
                        return "do";

                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count == 1)
                            return "does";
                        else
                            return "do";
                    }

                    return "does";

                default:
                    return "does";
            }
        }

        [LocaleFunction("does_not")]
        public static string BuiltIn_does_not(object? obj)
        {
            return "没有";
        }
    }
}
