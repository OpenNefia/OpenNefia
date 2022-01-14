using OpenNefia.Content.Charas;
using OpenNefia.Content.GameObjects;
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
    [RegisterLocaleFunctions("en_US")]
    public static class EnglishBuiltins
    {
        /// <summary>
        /// Function: s(entity, needE)
        /// </summary>
        /// <hsp>#defcfunc _s int tg, int op</hsp>
        [LocaleFunction("s")]
        public static string BuiltIn_s(object? obj, bool needE = false)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt != 1)
                        return "";
                    else if (needE)
                        return "es";
                    else
                        return "s";

                case long objLong:
                    if (objLong != 1)
                        return "";
                    else if (needE)
                        return "es";
                    else
                        return "s";

                case EntityUid objEntity:
                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count > 1)
                            return "";
                        else if (needE)
                            return "es";
                        else
                            return "s";
                    }

                    var gameSession = IoCManager.Resolve<IGameSessionManager>();

                    if (gameSession.IsPlayer(objEntity))
                        return "";
                    else if (needE)
                        return "es";
                    else
                        return "s";

                default:
                    if (needE)
                        return "es";
                    else
                        return "s";
            }
        }

        /// <summary>
        /// Function: has(entity, needE)
        /// </summary>
        /// <hsp>#defcfunc have int have_charid</hsp>
        [LocaleFunction("has")]
        public static string BuiltIn_have(object? obj, bool needE = false)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt == 1)
                        return "has";
                    else
                        return "have";

                case long objLong:
                    if (objLong == 1)
                        return "has";
                    else
                        return "have";

                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();

                    if (gameSession.IsPlayer(objEntity))
                        return "have";

                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count == 1)
                            return "has";
                        else
                            return "have";
                    }

                    return "has";

                default:
                    return "has";
            }
        }

        /// <summary>
        /// Function: is(entity_or_integer)
        /// </summary>
        /// <hsp>#defcfunc is int is_charid</hsp>
        /// <hsp>#defcfunc is2 int is2_arg1</hsp>
        [LocaleFunction("is")]
        public static string BuiltIn_is(object? obj)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt == 1)
                        return "is";
                    else
                        return "are";

                case long objLong:
                    if (objLong == 1L)
                        return "is";
                    else
                        return "are";

                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();
                    if (gameSession.IsPlayer(objEntity))
                    {
                        return "are";
                    }

                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count != 1)
                            return "are";
                    }

                    return "is";

                default:
                    return "is";
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
                        return "its";
                    else
                        return "their";

                case long objLong:
                    if (objLong == 1L)
                        return "its";
                    else
                        return "their";

                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();
                    if (gameSession.IsPlayer(objEntity))
                    {
                        return "your";
                    }

                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out CharaComponent chara))
                    {
                        switch (chara.Gender)
                        {
                            case Gender.Female:
                                return "her";
                            case Gender.Male:
                                return "his";
                            default:
                                return "their";
                        }
                    }

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count != 1)
                            return "their";
                    }

                    return "its";

                default:
                    return "its";
            }
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
                        return "it";
                    else
                        return "them";

                case long objLong:
                    if (objLong == 1L)
                        return "it";
                    else
                        return "them";

                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();
                    if (gameSession.IsPlayer(objEntity))
                    {
                        return "you";
                    }

                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out CharaComponent chara))
                    {
                        switch (chara.Gender)
                        {
                            case Gender.Female:
                                return "her";
                            case Gender.Male:
                                return "him";
                            default:
                                return "them";
                        }
                    }

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count != 1)
                            return "them";
                    }

                    return "it";

                default:
                    return "it";
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
                    return "itself";
                else
                    return "it";
            }

            // Check if the target is different than the source, and return "you"/"her"
            // if so.
            if (source is not EntityUid sourceEnt || sourceEnt != targetEnt)
                return BuiltIn_him(targetEnt);

            // Handle "yourself"/"herself".
            var gameSession = IoCManager.Resolve<IGameSessionManager>();

            if (gameSession.IsPlayer(targetEnt))
            {
                return "yourself";
            }

            var entMan = IoCManager.Resolve<IEntityManager>();

            if (entMan.TryGetComponent(targetEnt, out CharaComponent chara))
            {
                switch (chara.Gender)
                {
                    case Gender.Female:
                        return "herself";
                    case Gender.Male:
                        return "himself";
                    default:
                        return "themselves";
                }
            }

            if (entMan.TryGetComponent(targetEnt, out StackComponent stack))
            {
                if (stack.Count != 1)
                    return "themselves";
            }

            return "itself";
        }
    }
}
